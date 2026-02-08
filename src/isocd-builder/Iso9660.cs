using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

#if !ACTUAL_RELEASE
using System.IO.Abstractions;
#else
using FileInfoBase = System.IO.FileInfo;
using _fileSystem = System.IO;
#endif

namespace isocd_builder
{
//new
    class OrderValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
    }
//new END

    /// <summary>
    /// This class provides the core functionality to produce an ISO 9660 file system ISO image compatible with AmigaDOS.
    /// </summary>
    public class Iso9660
    {

#if !ACTUAL_RELEASE
        readonly IFileSystem _fileSystem;
#endif

        bool _usingMockFileSystem = false;

        int indexCounter = 0;
        ushort directoryNumber = 1;

        readonly List<Iso9660Entry> fullEntries = new List<Iso9660Entry>();
        readonly Queue<DirectoryQueueEntry> dirQueue = new Queue<DirectoryQueueEntry>();
        readonly WorkerUpdateStatus reportProgressUserState = new WorkerUpdateStatus();

        readonly Options options;

#if !ACTUAL_RELEASE

        // This is our standard constructor in production which uses the the normal System.IO namespace
        public Iso9660(Options options) : this(new FileSystem(), options)
        {
        }

        // This is our testing constructor which uses the System.IO.Abstractions namespace to allow us to use a mock file system for unit testing
        public Iso9660(IFileSystem fileSystem, Options options)
        {
            if (fileSystem == null)
            {
                throw new NullReferenceException("The fileSystem object cannot be null.");
            }

            if (options == null)
            {
                throw new NullReferenceException("The options object cannot be null.");
            }

            _fileSystem = fileSystem;
            this.options = options;
            _usingMockFileSystem = true;
        }

#else

        public Iso9660(Options options)
        {
            this.options = options;
        }

#endif

        /// <summary>
        /// Recursively scans a folder structure to find all files and directories present and generate appropriate records for the ISO 9660 filesystem.
        /// </summary>
        void TreeScan(DirectoryQueueEntry parent, ushort parentDirNumber, BuildIsoWorker worker)
        {

            //Encoding windowsEncoding = Encoding.GetEncoding(1250); // Windows-1250
            //Encoding isoEncoding = Encoding.GetEncoding("ISO-8859-1");

            var _parentDir = parent;

            while (parent != null)
            {
                worker.Token.ThrowIfCancellationRequested();

#if !ACTUAL_RELEASE
                var dirInfo = _fileSystem.DirectoryInfo.New(parent.Path);
#else
                var dirInfo = new DirectoryInfo(parent.Path);
#endif
                var entries = dirInfo.EnumerateFileSystemInfos()
                    .Select(f => CreateEntry(f, parent, parentDirNumber, _parentDir))
                    .Where(e => e.Identifier != isocd_builder_constants.WINUAE_ATTRIBUTES_FILE)
                    .Where(e => e.Name != ("ISOCD_" + options.VolumeId + ".txt"))
                    .Where(e => e.Name != ("ISOCD_" + options.VolumeId + "_dump.txt"))
                    .OrderBy(e => e.Type)
                    .ThenBy(e => e.Path, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var e in entries)
                {
                    e.Index = indexCounter++;

                    bool isValid = Regex.IsMatch(e.Name, @"^[a-zA-Z0-9 ()!_.+\-\[\]\{\}&#$@]+$");
                    if (!isValid)
                        throw new InvalidOperationException(
                        isocd_builder_constants.ERROR_MESSAGE_ILLEGAL_CHARACTER +
                        e.Path
                        );


                    //if (e.Name.Contains("espa"))
                    //    ;

                    //byte[] windowsBytes = windowsEncoding.GetBytes(e.Name);
                    //e.Name = isoEncoding.GetString(windowsBytes);
                }

                fullEntries.AddRange(entries);

                foreach (var d in entries.Where(e => e.Type == EntryType.Directory))
                {
                    dirQueue.Enqueue(new DirectoryQueueEntry { Path = d.Path, Index = d.Index });
                }

                parent = dirQueue.Count > 0 ? dirQueue.Dequeue() : null;
                parentDirNumber++;
            }
        }

        public static string ReplaceInvalidChars(string input)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in input)
            {
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            }

            return sb.ToString();
        }

#if !ACTUAL_RELEASE
        Iso9660Entry CreateEntry(IFileSystemInfo f, DirectoryQueueEntry parent, ushort parentDirNumber, DirectoryQueueEntry _parentDir)
#else
        Iso9660Entry CreateEntry(FileSystemInfo f, DirectoryQueueEntry parent, ushort parentDirNumber, DirectoryQueueEntry _parentDir)
#endif
        {
            var entry = new Iso9660Entry
            {
                ParentDirectoryIndex = parent.Index,
                ParentDirectoryNumber = parentDirNumber,
                Path = f.FullName,
                Name = f.Name,
                DateStamp = f.LastWriteTimeUtc
            };

            string _rootSubdirectory = f.FullName.Replace(_parentDir.Path, "");

            if (_rootSubdirectory.Count() > 255)
            {
                throw new InvalidOperationException(isocd_builder_constants.ERROR_MESSAGE_PATH_LENGTH_TO_LONG);
            }

            if (f is FileInfoBase)
            {
                entry.Type = EntryType.File;
                entry.Size = ((FileInfoBase)f).Length;
            }
            else
            {
                if (_rootSubdirectory.Count(testChar => testChar == '\\') > 8)
                {
                    throw new InvalidOperationException(isocd_builder_constants.ERROR_MESSAGE_SUBDIRECTORY_LIMIT_EXCEEDED);
                }

                entry.Type = EntryType.Directory;
                entry.DirectoryNumber = ++directoryNumber;
                entry.PathTableRecordSize =
                    isocd_builder_constants.MINIMUM_PATH_TABLE_RECORD_SIZE +
                    entry.Identifier.Length - 1 +
                    ((entry.Identifier.Length & 1) == 1 ? 1 : 0);
            }

            entry.DirectoryRecordSize =
                isocd_builder_constants.MINIMUM_DIR_RECORD_SIZE +
                entry.Identifier.Length - 1 +
                ((entry.Identifier.Length & 1) == 0 ? 1 : 0);

            return entry;
        }

        // ----------------------------------------------------
        // FILE ORDER (CD32)
        // ----------------------------------------------------
        void GenerateOrderFile(string path)
        {
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.WriteLine("# ISO9660 CD32 FILE ORDER");
                sw.WriteLine();

                foreach (var e in fullEntries.Skip(1))
                {
                    var prefix = e.Type == EntryType.Directory ? "D:" : "F:";
                    sw.WriteLine(prefix + GetIsoPath(e));
                }
            }
        }

        void ApplyOrderFile(string path)
        {
            var entry = new Iso9660Entry();

            var order = File.ReadAllLines(path)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith("#"))
                .ToList();

            var root = fullEntries[0];

            var map = fullEntries.Skip(1)
                .ToDictionary(
                    e => (e.Type == EntryType.Directory ? "D:" : "F:") + GetIsoPath(e),
                    e => e
                );

            var reordered = new List<Iso9660Entry>();

            foreach (var k in order)
                if (map.TryGetValue(k, out entry))
                    reordered.Add(entry);

            foreach (var e in map.Values)
                if (!reordered.Contains(e))
                    reordered.Add(e);

            RebuildIndexes(root, reordered);
        }

        void RebuildIndexes(Iso9660Entry root, List<Iso9660Entry> reordered)
        {
            int p;
            var indexMap = new Dictionary<int, int>();

            fullEntries.Clear();
            fullEntries.Add(root);

            int idx = 1;
            foreach (var e in reordered)
            {
                indexMap[e.Index] = idx;
                e.Index = idx++;
                fullEntries.Add(e);
            }

            foreach (var e in fullEntries.Skip(1))
                if (indexMap.TryGetValue(e.ParentDirectoryIndex, out p))
                    e.ParentDirectoryIndex = p;
        }

        string GetIsoPath(Iso9660Entry e)
        {
            var parts = new Stack<string>();
            var cur = e;
            while (cur.Index != 0)
            {
                parts.Push(cur.Name);
                cur = fullEntries[cur.ParentDirectoryIndex];
            }
            return "/" + string.Join("/", parts);
        }

        OrderValidationResult ValidateOrderFile(string path)
        {
            var result = new OrderValidationResult();

            if (!File.Exists(path))
            {
                result.Errors.Add($"Order file not found: {path}");
                return result;
            }

            var lines = File.ReadAllLines(path)
                .Select((l, i) => new { Line = l.Trim(), LineNo = i + 1 })
                .Where(x => x.Line.Length > 0 && !x.Line.StartsWith("#") && !x.Line.Contains("ISOCD_" + options.VolumeId + "_output.txt"))
                .ToList();

            var seen = new HashSet<string>();

            var isoMap = fullEntries
                .Skip(1)
                .Where(e => e.Name != ("ISOCD_" + options.VolumeId + "_output.txt"))
                .ToDictionary(
                    e => (e.Type == EntryType.Directory ? "D:" : "F:") + GetIsoPath(e),
                    e => e
                );

            foreach (var l in lines)
            {
                if (!(l.Line.StartsWith("D:") || l.Line.StartsWith("F:")))
                {
                    result.Errors.Add($"Line {l.LineNo}: must start with D: or F:");
                    continue;
                }

                if (l.Line.Length <= 2 || l.Line[2] != '/')
                {
                    result.Errors.Add($"Line {l.LineNo}: invalid ISO path");
                    continue;
                }

                if (!seen.Add(l.Line))
                {
                    result.Errors.Add($"Line {l.LineNo}: duplicate entry '{l.Line}'");
                    continue;
                }

                var entry = new Iso9660Entry();
                if (!isoMap.TryGetValue(l.Line, out entry))
                {
                    result.Errors.Add($"Line {l.LineNo}: entry not found in ISO: {l.Line}");
                    continue;
                }

                if (l.Line.StartsWith("D:") && entry.Type != EntryType.Directory)
                {
                    result.Errors.Add($"Line {l.LineNo}: expected DIRECTORY but found FILE: {l.Line}");
                }

                if (l.Line.StartsWith("F:") && entry.Type != EntryType.File)
                {
                    result.Errors.Add($"Line {l.LineNo}: expected FILE but found DIRECTORY: {l.Line}");
                }
            }

            foreach (var k in isoMap.Keys)
            {
                if (!seen.Contains(k))
                    result.Warnings.Add($"Missing entry in order file: {k}");
            }

            return result;
        }

        /// <summary>
        /// Gets the info for a file or directory from the source file system.
        /// </summary>
        void GetEntryInfo(Iso9660Entry entry)
        {
#if !ACTUAL_RELEASE
            var fileInfo = _fileSystem.FileInfo.New(entry.Path);
#else
            var fileInfo = new FileInfoBase(entry.Path);
#endif

            // Store date
            entry.DateStamp = fileInfo.LastWriteTimeUtc;

            // And size if a file
            if (entry.Type == EntryType.File)
            {
                entry.Size = fileInfo.Length;
            }
            else
            {
                // Calculate the path table record size for directories
                entry.PathTableRecordSize = isocd_builder_constants.MINIMUM_PATH_TABLE_RECORD_SIZE + (entry.Identifier.Length - 1);

                // Padding is only required if the entry identifier length is odd
                if ((entry.Identifier.Length & 1) == 1)
                {
                    entry.PathTableRecordSize++;
                }
            }

            // Calculate the ISO9660 directory record size for each entry
            entry.DirectoryRecordSize = isocd_builder_constants.MINIMUM_DIR_RECORD_SIZE + (entry.Identifier.Length - 1);

            // Padding is only required if the entry identifier length is even
            if ((entry.Identifier.Length & 1) == 0)
            {
                entry.DirectoryRecordSize++;
            }
        }

        /// <summary>
        /// Writes the CDFS (Compact Disc File System) options to the provided binary stream. Also includes the custom trademark file if provided
        /// to allow booting of the CD on the CD32 or CDTV.
        /// </summary>
        int WriteCDFSOptions(BinaryWriter binWriter, int tmSize, int tmStartSector)
        {
            var bytesWritten = 0;

            binWriter.Write((byte)0x00);
            bytesWritten++;

            bytesWritten += WriteNumericCDFSOption(binWriter, options.DataCache, isocd_builder_constants.DEFAULT_DATA_CACHE, isocd_builder_constants.CACHE_DATA_NAME);
            bytesWritten += WriteNumericCDFSOption(binWriter, options.DirCache, isocd_builder_constants.DEFAULT_DIR_CACHE, isocd_builder_constants.CACHE_DIR_NAME);
            bytesWritten += WriteNumericCDFSOption(binWriter, options.FileLock, isocd_builder_constants.DEFAULT_FILE_LOCK, isocd_builder_constants.FILE_LOCK_NAME);
            bytesWritten += WriteNumericCDFSOption(binWriter, options.FileHandle, isocd_builder_constants.DEFAULT_FILE_HANDLE, isocd_builder_constants.FILE_HANDLE_NAME);
            bytesWritten += WriteNumericCDFSOption(binWriter, options.Retries, isocd_builder_constants.DEFAULT_RETRIES, isocd_builder_constants.RETRIES_NAME);

            bytesWritten += WriteBooleanCDFSOption(binWriter, options.DirectRead, isocd_builder_constants.DIRECT_READ_NAME);
            bytesWritten += WriteBooleanCDFSOption(binWriter, options.FastSearch, isocd_builder_constants.FAST_SEARCH_NAME);
            bytesWritten += WriteBooleanCDFSOption(binWriter, options.SpeedIndependent, isocd_builder_constants.SPEED_INDEPENDENT_NAME);

            // Include the trademark file if provided
            if (tmSize > 0)
            {
                binWriter.Write(isocd_builder_constants.TRADEMARK_NAME.ToCharArray());

                // All of these need to be written in big-endian for the Amiga
                binWriter.Write(EndianHelper.ChangeEndian(0x14));
                binWriter.Write(EndianHelper.ChangeEndian((uint)tmSize));
                binWriter.Write(EndianHelper.ChangeEndian((uint)tmStartSector));
                bytesWritten += 12;
            }

            return bytesWritten;
        }

        /// <summary>
        /// Writes a numeric CDFS option to the binary stream.
        /// </summary>
        int WriteNumericCDFSOption(BinaryWriter binWriter, int value, int defaultValue, string name)
        {
            // Only write the value to the stream if it differs from the system default
            if (value != defaultValue)
            {
                binWriter.Write(name.ToCharArray());
                binWriter.Write(EndianHelper.ChangeEndian((ushort)0x02));
                binWriter.Write(EndianHelper.ChangeEndian((ushort)value));
                return 6;
            }
            return 0;
        }

        /// <summary>
        /// Writes a boolean CDFS option to the binary stream.
        /// </summary>
        int WriteBooleanCDFSOption(BinaryWriter binWriter, bool value, string name)
        {
            // Only write to the stream if the option is true
            if (value)
            {
                binWriter.Write(name.ToCharArray());
                binWriter.Write(EndianHelper.ChangeEndian((ushort)0x00));
                return 4;
            }
            return 0;
        }

        /// <summary>
        /// Writes three volume descriptors specific to ISO9660:
        /// 1. Primary volume descriptor
        /// 2. Supplementary volume descriptor
        /// 3. Volume descriptor set terminator
        /// </summary>
        void WriteVolumeDescriptors(BinaryWriter binWriter,
                                            uint totalSectors,
                                            uint pathTableSize,
                                            uint bigEndianPathTableStartSector,
                                            uint littleEndianPathTableStartSector,
                                            int trademarkSize,
                                            int trademarkStartSector)
        {
            var volumeDescriptor = new MemoryStream(2048);

            using (var volumeDescriptorWriter = new BinaryWriter(volumeDescriptor))
            {
                // Type Code
                volumeDescriptorWriter.Write((byte)0x01);

                // Standard Identifier 
                volumeDescriptorWriter.Write(isocd_builder_constants.STANDARD_IDENTIFIER.ToCharArray());

                // Version
                volumeDescriptorWriter.Write((byte)0x01);

                // Unused 
                volumeDescriptorWriter.Write((byte)0x00);

                // System Identifier
                volumeDescriptorWriter.Write(isocd_builder_constants.SYSTEM_IDENTIFIER.ToCharArray());

                // Volume Identifier 
                volumeDescriptorWriter.Write(options.VolumeId.ToCharArray());
                volumeDescriptorWriter.Seek(32 - options.VolumeId.Length, SeekOrigin.Current);

                // Unused
                volumeDescriptorWriter.Seek(8, SeekOrigin.Current);

                // Volume Space Size
                volumeDescriptorWriter.Write(EndianHelper.BothEndian(totalSectors));

                // Unused
                volumeDescriptorWriter.Seek(32, SeekOrigin.Current);

                // Volume Set Size
                volumeDescriptorWriter.Write(EndianHelper.BothEndian((ushort)0x01));

                // Volume Sequence Number
                volumeDescriptorWriter.Write(EndianHelper.BothEndian((ushort)0x01));

                // Logical Block Size
                volumeDescriptorWriter.Write(EndianHelper.BothEndian((ushort)isocd_builder_constants.SECTOR_SIZE));

                // Path Table Size
                volumeDescriptorWriter.Write(EndianHelper.BothEndian(pathTableSize));

                // Location of Type-L (little endian) Path Table (LBA)
                volumeDescriptorWriter.Write(littleEndianPathTableStartSector);

                // Location of Optional Type-L (little endian) Path Table (LBA)
                // ISOCD uses the location of the primary path table again
                volumeDescriptorWriter.Write(littleEndianPathTableStartSector);

                // Location of Type-M (big endian) Path Table (LBA)
                volumeDescriptorWriter.Write(EndianHelper.ChangeEndian(bigEndianPathTableStartSector));

                // Location of Optional Type-M (big endian) Path Table (LBA)
                // ISOCD uses the location of the primary path table again
                volumeDescriptorWriter.Write(EndianHelper.ChangeEndian(bigEndianPathTableStartSector));

                // Directory entry for the root directory
                WriteDirectoryRecord(fullEntries[0], volumeDescriptorWriter, WriteDirectoryType.FirstDirectoryRecord);

                // Volume Set Identifier (128 bytes)
                volumeDescriptorWriter.Write(options.VolumeSetId.ToCharArray());
                volumeDescriptorWriter.Seek(128 - options.VolumeSetId.Length, SeekOrigin.Current);

                // Publisher Identifier (128 bytes)
                volumeDescriptorWriter.Write(options.PublisherId.ToCharArray());
                volumeDescriptorWriter.Seek(128 - options.PublisherId.Length, SeekOrigin.Current);

                // Data Preparer Identifier (128 bytes)
                // user defined part first, followed by our own
                volumeDescriptorWriter.Write(options.DataPreparerId.ToCharArray());
                volumeDescriptorWriter.Write(isocd_builder_constants.ISOCDWIN_DATA_PREPARER_IDENTIFIER.ToCharArray());
                volumeDescriptorWriter.Seek(128 - options.DataPreparerId.Length - isocd_builder_constants.ISOCDWIN_DATA_PREPARER_IDENTIFIER.Length, SeekOrigin.Current);

                // Application Identifier (128 bytes)
                volumeDescriptorWriter.Write(options.ApplicationId.ToCharArray());
                volumeDescriptorWriter.Seek(128 - options.ApplicationId.Length, SeekOrigin.Current);

                // All zeroed:
                // Copyright File Identifier (38 bytes)
                // Abstract File Identifier (36 bytes)
                // Bibliographic File Identifier (37 bytes)
                volumeDescriptorWriter.Seek(111, SeekOrigin.Current);

                var now = DateTime.Now;

                if (_usingMockFileSystem)
                {
                    // As we know we're under test, just set an arbitrary date and time to allow the hash checks to pass
                    now = new DateTime(2000, 01, 01, 00, 00, 00);
                }

                // Volume Creation Date and Time
                volumeDescriptorWriter.Write(
                    string.Format(
                        "{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}{6:D2}{7}",
                        now.Year,
                        now.Month,
                        now.Day,
                        now.Hour,
                        now.Minute,
                        now.Second,
                        // ISOCD always stores hundredths of a second as zero
                        0,
                        // ISOCD ignores the GMT timezone offset and zeroes it
                        '\x00'
                    ).ToCharArray()
                );

                // All zeroed:
                // Volume Modification Date and Time (17 bytes)
                // Volume Expiration Date and Time (17 bytes)
                // Volume Effective Date and Time (17 bytes)
                volumeDescriptorWriter.Seek(51, SeekOrigin.Current);

                // File Structure Version
                volumeDescriptorWriter.Write((byte)0x01);

                // Unused 
                volumeDescriptorWriter.Write((byte)0x00);

                // Application Used
                // ISOCD specific CDFS options
                var bytesWritten = WriteCDFSOptions(volumeDescriptorWriter, trademarkSize, trademarkStartSector);
                volumeDescriptorWriter.Seek(512 - bytesWritten, SeekOrigin.Current);

                // Write the primary and supplementary volume descriptors
                // ISOCD uses the same descriptor twice
                var buf = volumeDescriptor.GetBuffer();
                binWriter.Write(buf);
                binWriter.Write(buf);

                // Write Volume Descriptor Set Terminator
                binWriter.Write((byte)0xFF);
                binWriter.Write(isocd_builder_constants.STANDARD_IDENTIFIER.ToCharArray());
                binWriter.Write((byte)0x01);
                binWriter.Seek(isocd_builder_constants.SECTOR_SIZE - 7, SeekOrigin.Current);
            }
        }

        /// <summary>
        /// Calculates the space required for the ISO9660 path table. The ISO will contain two copies of this, one in big-endian format for the Amiga and
        /// another in little-endian for other systems, both of which are aligned to a sector size of 2048 bytes as per the ISO9660 spec.
        /// </summary>
        int CalcPathTableSize()
        {
            return fullEntries.Where(e => e.Type == EntryType.Directory).Sum(e => e.PathTableRecordSize);
        }

        /// <summary>
        /// Generates the path table in either big or little-endian format.
        /// </summary>
        byte[] GeneratePathTable(bool littleEndian)
        {
            var pathTable = new MemoryStream(2048);

            using (var pathTableWriter = new BinaryWriter(pathTable))
            {

                foreach (var entry in fullEntries.Where(f => f.Type == EntryType.Directory))
                {
                    var paddingByteRequired = entry.Identifier.Length % 2 > 0;

                    // ISOCD stores the directory names in the path table in uppercase (actual names are left intact)
                    var dirId = entry.Identifier.ToUpper();

                    // Length of Directory Identifier
                    pathTableWriter.Write((byte)dirId.Length);

                    // Extended Attribute Record Length
                    pathTableWriter.Write((byte)0x00);

                    // Location of Extent (LBA)
                    if (littleEndian)
                    {
                        pathTableWriter.Write((uint)entry.StartingSector);
                    }
                    else
                    {
                        pathTableWriter.Write(EndianHelper.ChangeEndian((uint)entry.StartingSector));
                    }

                    // Directory number of parent directory
                    if (littleEndian)
                    {
                        pathTableWriter.Write(entry.ParentDirectoryNumber);
                    }
                    else
                    {
                        pathTableWriter.Write(EndianHelper.ChangeEndian(entry.ParentDirectoryNumber));
                    }

                    // Directory Identifier (name)
                    pathTableWriter.Write(dirId.ToCharArray());

                    if (paddingByteRequired)
                    {
                        pathTableWriter.Write((byte)0x00);
                    }
                }

                // Any unused part of the last sector is filled with zeroes,
                // so we must align the buffer to the sector size
                var size = AlignToSectorBoundary((int)pathTable.Length);
                var buf = new byte[size];
                pathTable.Seek(0, SeekOrigin.Begin);
                pathTable.Read(buf, 0, (int)pathTable.Length);

                return buf;
            }
        }

        /// <summary>
        /// Determines the starting sector for each file or directory in the file system. This information is used when generating the path tables.
        /// </summary>
        void GetEntryPositions(int startingSector)
        {
            long sectorPos;
            var currentSector = startingSector;

            foreach (var entry in fullEntries)
            {
                if (entry.Type == EntryType.File)
                {
                    entry.SectorAlignedSize = AlignToSectorBoundary((int)entry.Size);
                    entry.NumberOfSectors = (int)(entry.SectorAlignedSize / isocd_builder_constants.SECTOR_SIZE);
                }
                else if (entry.Type == EntryType.Directory)
                {
                    // Allow for the first and second directories ("." and "..")     
                    var totalSize = 2 * isocd_builder_constants.MINIMUM_DIR_RECORD_SIZE;
                    sectorPos = totalSize;

                    var children = GetChildEntriesForDirectory(entry.DirectoryNumber);

                    // Calculate the size of the child files and directories
                    foreach (var child in children)
                    {
                        // Directory entries must not cross a sector boundary, so pad the current sector
                        // with zeroes so that the directory entry starts on the next consecutive sector
                        if (sectorPos + child.DirectoryRecordSize > isocd_builder_constants.SECTOR_SIZE)
                        {
                            totalSize += (int)(isocd_builder_constants.SECTOR_SIZE - sectorPos);
                            sectorPos = 0;
                        }

                        sectorPos += child.DirectoryRecordSize;
                        totalSize += child.DirectoryRecordSize;
                    }

                    entry.Size = totalSize;
                    entry.SectorAlignedSize = AlignToSectorBoundary((int)entry.Size);
                    entry.NumberOfSectors = (int)(entry.SectorAlignedSize / isocd_builder_constants.SECTOR_SIZE);
                }

                sectorPos = 0;

                entry.StartingSector = currentSector;
                currentSector += entry.NumberOfSectors;
            }
        }

        /// <summary>
        /// Writes the files and directories to the binary stream.
        /// </summary>
        void WriteFilesAndDirectories(Stream isostream, BinaryWriter binWriter, BuildIsoWorker worker)
        {
            long sectorPos = 0;

            reportProgressUserState.TotalEntries = fullEntries.Count;
            reportProgressUserState.CurrentEntry = 0;

            foreach (var entry in fullEntries)
            {
                worker.Token.ThrowIfCancellationRequested();

                reportProgressUserState.CurrentEntry++;

                if (entry.Type == EntryType.File)
                {
                    // Empty files still occupy a single sector
                    if (entry.Size == 0)
                    {
                        isostream.Seek(isocd_builder_constants.SECTOR_SIZE, SeekOrigin.Current);
                    }
                    else
                    {
#if !ACTUAL_RELEASE
                        using (var entrystream = _fileSystem.File.OpenRead(entry.Path))
#else
                        using (var entrystream = File.OpenRead(entry.Path))
#endif
                        {
                            entrystream.CopyToWithCancel(isostream, worker.Token);
                        }

                        sectorPos = entry.Size % isocd_builder_constants.SECTOR_SIZE;
                    }
                }
                else
                {
                    // Add the first and second directories ("." and "..")                
                    WriteDirectoryRecord(fullEntries[entry.Index], binWriter, WriteDirectoryType.FirstDirectoryRecord);
                    WriteDirectoryRecord(fullEntries[entry.ParentDirectoryIndex], binWriter, WriteDirectoryType.SecondDirectoryRecord);
                    sectorPos = 2 * isocd_builder_constants.MINIMUM_DIR_RECORD_SIZE;

                    var children = GetChildEntriesForDirectory(entry.DirectoryNumber);

                    foreach (var child in children)
                    {
                        worker.Token.ThrowIfCancellationRequested();

                        // Directory entries must not cross a sector boundary, so pad the current sector
                        // with zeroes so that the directory entry starts on the next consecutive sector
                        if (sectorPos + child.DirectoryRecordSize > isocd_builder_constants.SECTOR_SIZE)
                        {
                            binWriter.Seek((int)(isocd_builder_constants.SECTOR_SIZE - sectorPos), SeekOrigin.Current);
                            sectorPos = 0;
                        }

                        sectorPos += child.DirectoryRecordSize;

                        WriteDirectoryRecord(child, binWriter);
                    }
                }

                if (sectorPos > 0)
                {
                    // Pad to the sector boundary with zeroes if necessary
                    binWriter.Seek((int)(isocd_builder_constants.SECTOR_SIZE - sectorPos), SeekOrigin.Current);
                }

                sectorPos = 0;
                worker.ReportProgress(reportProgressUserState);
            }
        }

        /// <summary>
        /// Writes a directory record to the binary stream.
        /// </summary>
        void WriteDirectoryRecord(Iso9660Entry entry, BinaryWriter binWriter, WriteDirectoryType writeDirectoryType = WriteDirectoryType.Normal)
        {
            // Length of Directory Record
            if (writeDirectoryType == WriteDirectoryType.Normal)
            {
                binWriter.Write((byte)entry.DirectoryRecordSize);
            }
            else
            {
                binWriter.Write((byte)isocd_builder_constants.MINIMUM_DIR_RECORD_SIZE);
            }

            // Extended Attribute Record Length
            binWriter.Write((byte)0x00);

            // Location of Extent (LBA)
            binWriter.Write(EndianHelper.BothEndian((uint)entry.StartingSector));

            // Data Length
            if (entry.Type == EntryType.Directory)
            {
                binWriter.Write(EndianHelper.BothEndian((uint)entry.SectorAlignedSize));
            }
            else
            {
                binWriter.Write(EndianHelper.BothEndian((uint)entry.Size));
            }

            // Recording Date and Time
            binWriter.Write(entry.BinaryDate);

            // File Flags
            if (entry.Type == EntryType.Directory)
            {
                binWriter.Write((byte)isocd_builder_constants.DIR_FLAG);
            }
            else
            {
                binWriter.Write((byte)0x00);
            }

            // All zeroed:
            // File Unit Size (1 byte)
            // Interleave Gap Size (1 byte)
            // Volume Sequence Number (4 bytes)
            binWriter.Seek(6, SeekOrigin.Current);

            // Length of File Identifier
            if (writeDirectoryType == WriteDirectoryType.Normal)
            {
                binWriter.Write((byte)entry.Identifier.Length);
            }
            else
            {
                binWriter.Write((byte)0x01);
            }

            // File Identifier
            switch (writeDirectoryType)
            {
                case WriteDirectoryType.FirstDirectoryRecord:
                    binWriter.Write((byte)0x00);
                    break;
                case WriteDirectoryType.SecondDirectoryRecord:
                    binWriter.Write((byte)0x01);
                    break;
                case WriteDirectoryType.Normal:
                default:
                    binWriter.Write(entry.Identifier.ToCharArray());

                    // Pad record if the identifier length is even
                    if ((entry.Identifier.Length & 1) == 0)
                    {
                        binWriter.Write((byte)0x00);
                    }
                    break;
            }
        }

        /// <summary>
        /// Returns all child entries associated with a given directory number.
        /// </summary>
        IEnumerable<Iso9660Entry> GetChildEntriesForDirectory(int directoryNumber)
        {
            // Excludes root if necessary with Index 0 (which has both ParentDirectoryNumber and DirectoryNumber set to 1)
            return fullEntries.Where(
                e => e.ParentDirectoryNumber == directoryNumber &&
                e.Index != 0
            );
        }

        /// <summary>
        /// Calculates the required sector aligned size to store data of the provided size.
        /// The default sector size for ISO9660 is 2048 bytes.
        /// </summary>
        int AlignToSectorBoundary(int size)
        {
            // Empty files still occupy a single sector
            if (size == 0)
            {
                return isocd_builder_constants.SECTOR_SIZE;
            }

            return ((size + (isocd_builder_constants.SECTOR_SIZE - 1)) / isocd_builder_constants.SECTOR_SIZE) * isocd_builder_constants.SECTOR_SIZE;
        }

        /// <summary>
        /// Builds the ISO image in accordance with the options provided when the class was instantiated.
        /// </summary>
        public void BuildIso(BuildIsoWorker worker)
        {
            // Check provided input folder exists
#if !ACTUAL_RELEASE
            if (!_fileSystem.Directory.Exists(options.InputFolder))
            {
                throw new InvalidOperationException(isocd_builder_constants.ERROR_MESSAGE_INPUT_FOLDER_MUST_EXIST);
            }

            // Check provided output folder exists
            if (!_fileSystem.Directory.Exists(_fileSystem.Path.GetDirectoryName(options.OutputFile)))
            {
                throw new InvalidOperationException(isocd_builder_constants.ERROR_MESSAGE_OUTPUT_FOLDER_MUST_EXIST);
            }

            // Check provided trademark file exists
            if (!_fileSystem.File.Exists(options.TrademarkFile) && options.Trademark)
            {
                throw new InvalidOperationException(isocd_builder_constants.ERROR_MESSAGE_TRADEMARK_FILE_MUST_EXIST);
            }
#else
            if (!Directory.Exists(options.InputFolder))
            {
                throw new InvalidOperationException(isocd_builder_constants.ERROR_MESSAGE_INPUT_FOLDER_MUST_EXIST);
            }

            if (!Directory.Exists(Path.GetDirectoryName(options.OutputFile)))
            {
                throw new InvalidOperationException(isocd_builder_constants.ERROR_MESSAGE_OUTPUT_FOLDER_MUST_EXIST);
            }

            if (options.Trademark && !File.Exists(options.TrademarkFile))
            {
                throw new InvalidOperationException(isocd_builder_constants.ERROR_MESSAGE_TRADEMARK_FILE_MUST_EXIST);
            }
#endif

            fullEntries.Clear();
            indexCounter = 0;
            directoryNumber = 1;
            var useTmFile = options.Trademark & !string.IsNullOrEmpty(options.TrademarkFile);
            byte[] tmBytes = null;

            // Add root record before we begin scanning
            var rootEntry = new Iso9660Entry
            {
                Index = indexCounter++,
                Type = EntryType.Directory,
                ParentDirectoryNumber = 1,
                DirectoryNumber = 1,
                Path = options.InputFolder,
                Name = "\x01"
            };

            GetEntryInfo(rootEntry);
            fullEntries.Add(rootEntry);

            TreeScan(new DirectoryQueueEntry
            {
                Path = rootEntry.Path,
                Index = rootEntry.Index
            }, 1, worker);

            if (options.GenerateOrderFile)
            {
                GenerateOrderFile(options.InputFolder + '\\' + "ISOCD_" + options.VolumeId + ".txt");
                return;
            }

            if (options.UseOrderFile)
            {
                var validation = ValidateOrderFile(options.InputFolder + '\\' + "ISOCD_" + options.VolumeId + ".txt");

                if (!validation.IsValid)
                {
                    throw new InvalidOperationException(
                        isocd_builder_constants.VALIDATION_ORDER_FILE_FAILED +
                        string.Join("\n", validation.Errors)
                    );
                }

                foreach (var w in validation.Warnings)
                    worker.ReportProgress(new WorkerUpdateStatus { StatusMessage = "Order warning: " + w });

                ApplyOrderFile(options.InputFolder + '\\' + "ISOCD_" + options.VolumeId + ".txt");
            }

            // Check provided input folder isn't empty
            if (fullEntries.Count() == 1)
            {
                throw new InvalidOperationException(isocd_builder_constants.ERROR_MESSAGE_INPUT_FOLDER_IS_EMPTY);
            }

            var pathTableSize = CalcPathTableSize();

            if (useTmFile)
            {
#if !ACTUAL_RELEASE
                using (var tmStream = _fileSystem.File.OpenRead(options.TrademarkFile))
#else
                using (var tmStream = File.OpenRead(options.TrademarkFile))
#endif
                {
                    tmBytes = new byte[tmStream.Length];
                    tmStream.Read(tmBytes, 0, (int)tmStream.Length);
                }
            }
            else
            {
                tmBytes = new byte[0];
            }

            var trademarkStartingSector =
                // System Area
                16 +
                // 2 * PVDs
                2 +
                // TVD
                1 +
                // Path Tables (big and little-endian)
                2 * (AlignToSectorBoundary(pathTableSize) / isocd_builder_constants.SECTOR_SIZE);

            var directoriesStartingSector = trademarkStartingSector +
                // CDTV.TM / CD32.TM file
                (useTmFile ? AlignToSectorBoundary(tmBytes.Length) : 0) / isocd_builder_constants.SECTOR_SIZE;

            GetEntryPositions(directoriesStartingSector);

            var totalSectors =
                directoriesStartingSector +
                // Total sectors for all directories and files
                fullEntries.Sum(f => f.NumberOfSectors) +
                // 32 sectors of padding (64kb) at the end of the image
                32;

//debbugger
            /*using (var sw = new StreamWriter(options.InputFolder + '\\' + "ISOCD_" + options.VolumeId + "_dump.txt", false, Encoding.UTF8))
            {
                sw.WriteLine("# ISO9660 CD32 FILE DUMP");
                sw.WriteLine();

                foreach (var e in fullEntries)
                {
                    sw.WriteLine("Name: " + e.Name);
                    sw.WriteLine("Path: " + e.Path);
                    sw.WriteLine("Identifier: " + e.Identifier);
                    sw.WriteLine("Type: " + e.Type);

                    sw.WriteLine("Index   : " + e.Index);
                    byte[] bytes = BitConverter.GetBytes(e.Index);
                    //string hexLittle = BitConverter.ToString(bytes).Replace("-", "");
                    string hexBig = BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", "");
                    sw.WriteLine("Index 0x: " + hexBig);

                    sw.WriteLine("DirectoryNumber   : " + e.DirectoryNumber);
                    bytes = BitConverter.GetBytes(e.DirectoryNumber);
                    hexBig = BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", "");
                    sw.WriteLine("DirectoryNumber 0x: " + hexBig);

                    sw.WriteLine("ParentDirectoryIndex   : " + e.ParentDirectoryIndex);
                    bytes = BitConverter.GetBytes(e.ParentDirectoryIndex);
                    hexBig = BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", "");
                    sw.WriteLine("ParentDirectoryIndex 0x: " + hexBig);

                    sw.WriteLine("ParentDirectoryNumber   : " + e.ParentDirectoryNumber);
                    bytes = BitConverter.GetBytes(e.ParentDirectoryNumber);
                    hexBig = BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", "");
                    sw.WriteLine("ParentDirectoryNumber 0x: " + hexBig);

                    sw.WriteLine("DirectoryRecordSize   : " + e.DirectoryRecordSize);
                    bytes = BitConverter.GetBytes(e.DirectoryRecordSize);
                    hexBig = BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", "");
                    sw.WriteLine("DirectoryRecordSize 0x: " + hexBig);

                    sw.WriteLine("StartingSector   : " + e.StartingSector);
                    bytes = BitConverter.GetBytes(e.StartingSector);
                    hexBig = BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", "");
                    sw.WriteLine("StartingSector 0x: " + hexBig);

                    sw.WriteLine("NumberOfSectors: " + e.NumberOfSectors);
                    bytes = BitConverter.GetBytes(e.NumberOfSectors);
                    hexBig = BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", "");
                    sw.WriteLine("NumberOfSectors 0x: " + hexBig);

                    sw.WriteLine("SectorAlignedSize   : " + e.SectorAlignedSize);
                    bytes = BitConverter.GetBytes(e.SectorAlignedSize);
                    hexBig = BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", "");
                    sw.WriteLine("SectorAlignedSize 0x: " + hexBig);

                    sw.WriteLine("Size   : " + e.Size);
                    bytes = BitConverter.GetBytes(e.Size);
                    hexBig = BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", "");
                    sw.WriteLine("Size 0x: " + hexBig);
                    sw.WriteLine("-----------------------------------------------------");
                }
            }*/
//end of debbugger

            var maxSectors = 0;

            switch (options.PadSize)
            {
                case PadSizeType.Cdr74:
                    maxSectors = isocd_builder_constants.MAX_SECTORS_CDR74;
                    break;
                case PadSizeType.Cdr80:
                    maxSectors = isocd_builder_constants.MAX_SECTORS_CDR80;
                    break;
                case PadSizeType.Cdr90:
                    maxSectors = isocd_builder_constants.MAX_SECTORS_CDR90;
                    break;
                default:
                    maxSectors = isocd_builder_constants.MAX_SECTORS_CDR90;
                    break;

            }

            // Check data will not exceed max sectors
            if (totalSectors > maxSectors)
            {
                throw new InvalidOperationException(isocd_builder_constants.ERROR_MESSAGE_ISO_IMAGE_TOO_BIG);
            }

            var paddingSectors = 0;

            // Pad image so as to fill a CD-R 74 or CD-R 80 disc if requested
            // This is done to maximize the performance of double speed reading on the CD32 drive

            if (options.PadSize != PadSizeType.None && options.PadSize != PadSizeType.Min1 && options.PadSize != PadSizeType.Min10)
            {
                paddingSectors = maxSectors - totalSectors - 200;
                if (paddingSectors < 0) paddingSectors = 0;
                totalSectors = maxSectors;
            }
            else if (options.PadSize == PadSizeType.Min1)
            {
                paddingSectors = 4500 - 200;
                totalSectors = maxSectors;
            }
            else if (options.PadSize == PadSizeType.Min10)
            {
                paddingSectors = 45000 - 200;
                totalSectors = maxSectors;
            }

            if (options.PadSize != PadSizeType.None)
            {
                foreach (var entry in fullEntries)
                {
                    entry.StartingSector += paddingSectors;
                }
            }



            var pathTableLittleEndian = GeneratePathTable(true);
            var pathTableBigEndian = GeneratePathTable(false);

#if !ACTUAL_RELEASE
            using (var isoStream = _fileSystem.File.Open(options.OutputFile, FileMode.Create))
#else
            using (var isoStream = File.Open(options.OutputFile, FileMode.Create))
#endif
            // Use the same character encoding as AmigaDOS
            using (var binWriter = new BinaryWriter(isoStream, Encoding.GetEncoding("ISO-8859-1")))
            {
                // Write out the System Area blank sectors at the start of the image (32kb)
                isoStream.Seek(16 * isocd_builder_constants.SECTOR_SIZE, SeekOrigin.Begin);

                WriteVolumeDescriptors(
                    binWriter,
                    (uint)totalSectors,
                    (uint)pathTableSize,
                    // ISOCD always writes the big endian path table first, so the offset is known in advance (0x9800)
                    isocd_builder_constants.BIG_ENDIAN_PATH_TABLE_SECTOR,
                    (uint)(isocd_builder_constants.BIG_ENDIAN_PATH_TABLE_SECTOR + (pathTableBigEndian.Length / isocd_builder_constants.SECTOR_SIZE)),
                    tmBytes.Length,
                    trademarkStartingSector
                );

                // Write big endian path table
                binWriter.Write(pathTableBigEndian);

                // Write little endian path table
                binWriter.Write(pathTableLittleEndian);

                if (useTmFile)
                {
                    // Write the CDTV.TM / CD32.TM file and align to sector boundary
                    binWriter.Write(tmBytes);
                    binWriter.Seek(AlignToSectorBoundary(tmBytes.Length) - tmBytes.Length, SeekOrigin.Current);
                }

                if (paddingSectors > 0)
                {
                    worker.ReportProgress(new WorkerUpdateStatus { StatusMessage = "Adding padding to start of image..." });

                    // Add empty space at the beginning of the image to speed up CD reading with doublespeed
                    binWriter.Seek(paddingSectors * isocd_builder_constants.SECTOR_SIZE, SeekOrigin.Current);
                }

                WriteFilesAndDirectories(isoStream, binWriter, worker);

                // Pad out file with 64kb of zeroes
                isoStream.Seek(0xFFFF, SeekOrigin.Current);
                binWriter.Write((byte)0x00);
            }
        }
    }
}

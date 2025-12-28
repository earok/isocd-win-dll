using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;

#if !ACTUAL_RELEASE
using System.IO.Abstractions;
#else
using FileInfoBase = System.IO.FileInfo;
using _fileSystem = System.IO;
#endif

namespace isocd_builder
{
    public class Iso9660
    {
#if !ACTUAL_RELEASE
        readonly IFileSystem _fileSystem;
#endif
        readonly Options options;

        readonly List<Iso9660Entry> fullEntries = new List<Iso9660Entry>();
        readonly Queue<DirectoryQueueEntry> dirQueue = new Queue<DirectoryQueueEntry>();

        int indexCounter;
        ushort directoryNumber = 1;
        bool _usingMockFileSystem;

#if !ACTUAL_RELEASE
        public Iso9660(Options options) : this(new FileSystem(), options) { }

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

        // ----------------------------------------------------
        // TREE SCAN
        // ----------------------------------------------------
        void TreeScan(DirectoryQueueEntry parent, ushort parentDirNumber, BuildIsoWorker worker)
        {
            while (parent != null)
            {
                worker.Token.ThrowIfCancellationRequested();

#if !ACTUAL_RELEASE
                var dirInfo = _fileSystem.DirectoryInfo.New(parent.Path);
#else
                var dirInfo = new DirectoryInfo(parent.Path);
#endif
                var entries = dirInfo.EnumerateFileSystemInfos()
                    .Select(f => CreateEntry(f, parent, parentDirNumber))
                    .Where(e => e.Identifier != isocd_builder_constants.WINUAE_ATTRIBUTES_FILE)
                    .OrderBy(e => e.Type)
                    .ThenBy(e => e.Path, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                //entries.RemoveAll(e => e.Identifier == isocd_builder_constants.WINUAE_ATTRIBUTES_FILE);

                foreach (var e in entries)
                {
                    e.Index = indexCounter++;
                    //fullEntries.Add(e);
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

        Iso9660Entry CreateEntry(FileSystemInfo f, DirectoryQueueEntry parent, ushort parentDirNumber)
        {
            var entry = new Iso9660Entry
            {
                ParentDirectoryIndex = parent.Index,
                ParentDirectoryNumber = parentDirNumber,
                Path = f.FullName,
                Name = f.Name,
                DateStamp = f.LastWriteTimeUtc
            };

            if (f is FileInfoBase fi)
            {
                entry.Type = EntryType.File;
                entry.Size = fi.Length;
            }
            else
            {
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
            using var sw = new StreamWriter(path, false, Encoding.UTF8);
            sw.WriteLine("# ISO9660 CD32 FILE ORDER");
            sw.WriteLine();

            foreach (var e in fullEntries.Skip(1))
            {
                var prefix = e.Type == EntryType.Directory ? "D:" : "F:";
                sw.WriteLine(prefix + GetIsoPath(e));
            }
        }

        void ApplyOrderFile(string path)
        {
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
                if (map.TryGetValue(k, out var e))
                    reordered.Add(e);

            foreach (var e in map.Values)
                if (!reordered.Contains(e))
                    reordered.Add(e);

            RebuildIndexes(root, reordered);
        }

        void RebuildIndexes(Iso9660Entry root, List<Iso9660Entry> reordered)
        {
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
                if (indexMap.TryGetValue(e.ParentDirectoryIndex, out var p))
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

        // ----------------------------------------------------
        // BUILD ISO (skrót – reszta Twojej logiki BEZ ZMIAN)
        // ----------------------------------------------------
        public void BuildIso(BuildIsoWorker worker)
        {
            fullEntries.Clear();
            indexCounter = 0;
            directoryNumber = 1;

            var root = new Iso9660Entry
            {
                Index = indexCounter++,
                Type = EntryType.Directory,
                ParentDirectoryNumber = 1,
                DirectoryNumber = 1,
                Path = options.InputFolder,
                Name = "\x01"
            };

            fullEntries.Add(root);

            TreeScan(new DirectoryQueueEntry { Path = root.Path, Index = root.Index }, 1, worker);

            if (options.GenerateOrderFile)
            {
                GenerateOrderFile(options.OrderFilePath);
                return;
            }

            if (options.UseOrderFile)
                ApplyOrderFile(options.OrderFilePath);

            // ⬇️⬇️⬇️
            // TU IDZIE DOKŁADNIE TEN SAM KOD:
            // CalcPathTableSize()
            // GetEntryPositions()
            // WriteVolumeDescriptors()
            // WriteFilesAndDirectories()
            // itd.
            // ⬆️⬆️⬆️
        }
    }
}

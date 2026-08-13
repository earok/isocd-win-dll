using System;

namespace IsoCd {
    /// <summary>
    /// Every setting the ISO builder supports, with the same defaults the ISOCD-Win GUI starts with.
    /// Pass one of these to <see cref="IsoCdBuilder.Build(IsoBuildOptions, Action{IsoBuildProgress}, System.Threading.CancellationToken)"/>.
    /// </summary>
    public class IsoBuildOptions {

        // ---------------------------------------------------------------------
        // Paths (required)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Folder whose contents become the root of the disc. Required. Relative paths are resolved
        /// against the current working directory.
        /// </summary>
        public string InputFolder { get; set; }

        /// <summary>
        /// Path of the .iso file to write. Required. Relative paths are resolved against the current
        /// working directory. An existing file is overwritten.
        /// </summary>
        public string OutputFile { get; set; }

        // ---------------------------------------------------------------------
        // Target system and trademark
        // ---------------------------------------------------------------------

        /// <summary>
        /// Machine the image is built for. Defaults to <see cref="IsoTargetSystem.CD32"/>.
        /// Setting this to <see cref="IsoTargetSystem.Amiga"/> also forces <see cref="Trademark"/> off,
        /// matching the behaviour of the GUI.
        /// </summary>
        public IsoTargetSystem TargetSystem { get; set; }

        /// <summary>
        /// Inject a trademark file so the disc boots on a CD32 or CDTV. Defaults to true, and is
        /// forced to false when <see cref="TargetSystem"/> is <see cref="IsoTargetSystem.Amiga"/>.
        /// </summary>
        public bool Trademark { get; set; }

        /// <summary>
        /// Explicit path to the trademark file (CD32.TM or CDTV.TM) to inject. Leave null or empty to
        /// use the copy in <see cref="IsoCdBuilder.DataFolder"/> that matches <see cref="TargetSystem"/>,
        /// fetching it first if <see cref="AutoDownloadTrademarkFiles"/> allows.
        /// </summary>
        public string TrademarkFile { get; set; }

        /// <summary>
        /// When a trademark file is needed but not present in <see cref="IsoCdBuilder.DataFolder"/>,
        /// download it. Defaults to true. This performs network requests to third party archives
        /// listed in TmFileSources.json; set it to false to keep the build entirely offline, in which
        /// case a missing trademark file fails the build instead.
        /// </summary>
        public bool AutoDownloadTrademarkFiles { get; set; }

        // ---------------------------------------------------------------------
        // ISO 9660 volume descriptor identifiers
        // ---------------------------------------------------------------------

        /// <summary>Volume identifier, maximum 32 characters. Defaults to "CD32_TEST".</summary>
        public string VolumeId { get; set; }

        /// <summary>Publisher identifier, maximum 128 characters. Defaults to empty.</summary>
        public string PublisherId { get; set; }

        /// <summary>Application identifier, maximum 128 characters. Defaults to empty.</summary>
        public string ApplicationId { get; set; }

        /// <summary>Volume set identifier, maximum 128 characters. Defaults to empty.</summary>
        public string VolumeSetId { get; set; }

        /// <summary>Data preparer identifier, maximum 128 characters. Defaults to empty.</summary>
        public string DataPreparerId { get; set; }

        // ---------------------------------------------------------------------
        // AmigaDOS CDFS tuning, written into the boot block
        // ---------------------------------------------------------------------

        /// <summary>CDFS data cache size, 1 - 127. Defaults to 8.</summary>
        public int DataCache { get; set; }

        /// <summary>CDFS directory cache size, 1 - 127. Defaults to 16.</summary>
        public int DirCache { get; set; }

        /// <summary>File lock cache size, 1 - 9999. Defaults to 40.</summary>
        public int FileLock { get; set; }

        /// <summary>File handle cache size, 1 - 9999. Defaults to 16.</summary>
        public int FileHandle { get; set; }

        /// <summary>Number of read retries, 0 - 127. Defaults to 32.</summary>
        public int Retries { get; set; }

        /// <summary>Use the direct read optimisation. CDTV only. Defaults to false.</summary>
        public bool DirectRead { get; set; }

        /// <summary>Use the fast search optimisation. Defaults to false.</summary>
        public bool FastSearch { get; set; }

        /// <summary>Allow newer drives to read the disc at higher speeds. Defaults to false.</summary>
        public bool SpeedIndependent { get; set; }

        // ---------------------------------------------------------------------
        // Layout
        // ---------------------------------------------------------------------

        /// <summary>
        /// Pad the start of the image so the data sits on the faster outer tracks of the disc.
        /// Defaults to <see cref="IsoPadSize.None"/>.
        /// </summary>
        public IsoPadSize PadSize { get; set; }

        /// <summary>
        /// Write an ISOCD_&lt;VolumeId&gt;.txt order file into <see cref="InputFolder"/> listing every entry,
        /// so it can be hand-edited and fed back in via <see cref="UseOrderFile"/>. Defaults to false.
        /// </summary>
        public bool GenerateOrderFile { get; set; }

        /// <summary>
        /// Lay the disc out according to the ISOCD_&lt;VolumeId&gt;.txt order file in <see cref="InputFolder"/>,
        /// which lets you place frequently read files together to speed up loading. Defaults to false.
        /// The build fails if the order file is missing or invalid.
        /// </summary>
        public bool UseOrderFile { get; set; }

        // ---------------------------------------------------------------------
        // Convenience behaviour of this wrapper (not part of the ISO format)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Create the directory of <see cref="OutputFile"/> if it does not exist, rather than failing.
        /// Defaults to true.
        /// </summary>
        public bool CreateOutputDirectory { get; set; }

        /// <summary>
        /// Creates an instance with all the standard ISOCD-Win defaults applied.
        /// </summary>
        public IsoBuildOptions() {
            TargetSystem = IsoTargetSystem.CD32;
            Trademark = true;
            AutoDownloadTrademarkFiles = true;

            VolumeId = isocd_builder.isocd_builder_constants.VOLUME_IDENTIFIER;
            PublisherId = string.Empty;
            ApplicationId = string.Empty;
            VolumeSetId = string.Empty;
            DataPreparerId = string.Empty;

            DataCache = isocd_builder.isocd_builder_constants.DEFAULT_DATA_CACHE;
            DirCache = isocd_builder.isocd_builder_constants.DEFAULT_DIR_CACHE;
            FileLock = isocd_builder.isocd_builder_constants.DEFAULT_FILE_LOCK;
            FileHandle = isocd_builder.isocd_builder_constants.DEFAULT_FILE_HANDLE;
            Retries = isocd_builder.isocd_builder_constants.DEFAULT_RETRIES;

            DirectRead = false;
            FastSearch = false;
            SpeedIndependent = false;

            PadSize = IsoPadSize.None;
            GenerateOrderFile = false;
            UseOrderFile = false;

            CreateOutputDirectory = true;
        }

        /// <summary>
        /// Returns a shallow copy, so a caller-supplied options object is never mutated by a build.
        /// </summary>
        public IsoBuildOptions Clone() {
            return (IsoBuildOptions)MemberwiseClone();
        }
    }
}

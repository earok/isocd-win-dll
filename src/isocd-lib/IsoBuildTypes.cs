using System;

namespace IsoCd {
    /// <summary>
    /// The machine the generated image is intended to boot on. This drives which trademark file is
    /// injected into the image; <see cref="Amiga"/> produces a plain data disc with no trademark.
    /// </summary>
    public enum IsoTargetSystem {
        /// <summary>A plain data CD with no trademark file (not bootable on a CD32 or CDTV).</summary>
        Amiga = 0,
        /// <summary>Amiga CD32. Uses the 2,048 byte CD32.TM trademark file.</summary>
        CD32 = 1,
        /// <summary>Commodore CDTV. Uses the 22,152 byte CDTV.TM trademark file.</summary>
        CDTV = 2
    }

    /// <summary>
    /// Amount of blank padding written at the start of the image, pushing the real data towards the
    /// outside of the disc where a CD32 drive reads it faster.
    /// </summary>
    public enum IsoPadSize {
        /// <summary>No padding.</summary>
        None = 0,
        /// <summary>Pad out to the capacity of a 74 minute disc.</summary>
        Cdr74 = 1,
        /// <summary>Pad out to the capacity of an 80 minute disc.</summary>
        Cdr80 = 2,
        /// <summary>Pad out to the capacity of a 90 minute disc.</summary>
        Cdr90 = 3,
        /// <summary>Pad by one minute of sectors.</summary>
        Min1 = 4,
        /// <summary>Pad by ten minutes of sectors.</summary>
        Min10 = 5
    }

    /// <summary>
    /// How a build finished.
    /// </summary>
    public enum IsoBuildStatus {
        /// <summary>The image was written successfully.</summary>
        Success = 1,
        /// <summary>The build failed. See <see cref="IsoBuildResult.Message"/> and <see cref="IsoBuildResult.Exception"/>.</summary>
        Error = 2,
        /// <summary>The build was cancelled by the caller. The partial output file is deleted.</summary>
        Cancelled = 3,
        /// <summary>The supplied options failed validation, so no build was attempted.</summary>
        InvalidOptions = 4
    }

    /// <summary>
    /// A progress report raised while an image is being built.
    /// </summary>
    public class IsoBuildProgress {
        /// <summary>Completion percentage, 0 - 100.</summary>
        public int Percent { get; set; }

        /// <summary>Index of the file system entry currently being processed.</summary>
        public int CurrentEntry { get; set; }

        /// <summary>Total number of file system entries in the image.</summary>
        public int TotalEntries { get; set; }

        /// <summary>Human readable description of the current step, if the builder supplied one.</summary>
        public string Message { get; set; }

        public override string ToString() {
            return string.IsNullOrEmpty(Message)
                ? Percent + "% (" + CurrentEntry + "/" + TotalEntries + ")"
                : Percent + "% (" + CurrentEntry + "/" + TotalEntries + ") - " + Message;
        }
    }

    /// <summary>
    /// The outcome of a build. Never null and never throws out of <see cref="IsoCdBuilder.Build(IsoBuildOptions, Action{IsoBuildProgress}, System.Threading.CancellationToken)"/>;
    /// failures are reported here instead.
    /// </summary>
    public class IsoBuildResult {
        /// <summary>How the build finished.</summary>
        public IsoBuildStatus Status { get; set; }

        /// <summary>True only when <see cref="Status"/> is <see cref="IsoBuildStatus.Success"/>.</summary>
        public bool Success {
            get { return Status == IsoBuildStatus.Success; }
        }

        /// <summary>A description of the outcome, including validation errors or the failure reason.</summary>
        public string Message { get; set; }

        /// <summary>The exception that caused an <see cref="IsoBuildStatus.Error"/>, if there was one.</summary>
        public Exception Exception { get; set; }

        /// <summary>The absolute path the image was written to.</summary>
        public string OutputFile { get; set; }

        /// <summary>Size of the written image in bytes, or 0 if the build did not succeed.</summary>
        public long OutputSizeBytes { get; set; }

        /// <summary>The trademark file that was injected, or null if none was used.</summary>
        public string TrademarkFileUsed { get; set; }

        public override string ToString() {
            return Status + ": " + Message;
        }
    }

    /// <summary>
    /// The result of checking for, or fetching, the Commodore trademark files.
    /// </summary>
    public class IsoTrademarkStatus {
        /// <summary>True when both CD32.TM and CDTV.TM are present in the data folder and hash correctly.</summary>
        public bool Available { get; set; }

        /// <summary>True if files were downloaded during this call.</summary>
        public bool Downloaded { get; set; }

        /// <summary>Description of the outcome.</summary>
        public string Message { get; set; }

        /// <summary>Full path to CD32.TM in the data folder (whether or not it currently exists).</summary>
        public string Cd32FilePath { get; set; }

        /// <summary>Full path to CDTV.TM in the data folder (whether or not it currently exists).</summary>
        public string CdtvFilePath { get; set; }

        public override string ToString() {
            return (Available ? "Available" : "Unavailable") + ": " + Message;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using isocd_builder;

namespace IsoCd {
    /// <summary>
    /// The public entry point of the library. Builds AmigaDOS compatible ISO 9660 images, optionally
    /// bootable on an Amiga CD32 or Commodore CDTV.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = IsoCdBuilder.Build(new IsoBuildOptions {
    ///     InputFolder  = @"C:\MyGame\disc",
    ///     OutputFile   = @"C:\MyGame\MyGame.iso",
    ///     TargetSystem = IsoTargetSystem.CD32,
    ///     VolumeId     = "MY_GAME",
    ///     PublisherId  = "MY STUDIO",
    ///     PadSize      = IsoPadSize.Cdr74
    /// }, p =&gt; Console.WriteLine(p));
    ///
    /// if(!result.Success) {
    ///     Console.Error.WriteLine(result.Message);
    /// }
    /// </code>
    /// </example>
    public static class IsoCdBuilder {

        static readonly object dataFolderLock = new object();

        // ---------------------------------------------------------------------
        // Configuration
        // ---------------------------------------------------------------------

        /// <summary>
        /// Folder the trademark files (CD32.TM, CDTV.TM) and TmFileSources.json are kept in.
        /// Defaults to the ISOCD-Win folder under the machine's shared documents directory, so it is
        /// shared with the GUI and console applications. Set it to redirect the library elsewhere -
        /// useful when the shared documents folder is not writable, or to keep the data alongside an
        /// application's own files.
        /// </summary>
        public static string DataFolder {
            get {
                lock(dataFolderLock) {
                    return isocd_builder_constants.ISOCDWIN_PUBLIC_DOCUMENTS_PATH;
                }
            }
            set {
                if(string.IsNullOrWhiteSpace(value)) {
                    throw new ArgumentException("DataFolder cannot be null or empty.", "value");
                }

                lock(dataFolderLock) {
                    var full = Path.GetFullPath(value);
                    isocd_builder_constants.ISOCDWIN_PUBLIC_DOCUMENTS_PATH = full;

                    // TmFileHelper caches the sources file path in a static field at type
                    // initialisation, so it has to be repointed as well.
                    TmFileHelper.ISOCDWIN_TMSOURCES_FILE_PATH =
                        Path.Combine(full, isocd_builder_constants.TRADEMARK_FILE_SOURCES_NAME);
                }
            }
        }

        /// <summary>
        /// Version of the underlying image builder.
        /// </summary>
        public static Version Version {
            get { return typeof(IsoCdBuilder).Assembly.GetName().Version; }
        }

        // ---------------------------------------------------------------------
        // Trademark files
        // ---------------------------------------------------------------------

        /// <summary>
        /// Full path of the trademark file for a target system inside <see cref="DataFolder"/>.
        /// Returns null for <see cref="IsoTargetSystem.Amiga"/>, which uses no trademark.
        /// The file is not guaranteed to exist; use <see cref="EnsureTrademarkFiles"/> for that.
        /// </summary>
        public static string GetTrademarkFilePath(IsoTargetSystem targetSystem) {
            switch(targetSystem) {
                case IsoTargetSystem.CD32:
                    return Path.Combine(DataFolder, isocd_builder_constants.CD32_TRADEMARK_FILE);
                case IsoTargetSystem.CDTV:
                    return Path.Combine(DataFolder, isocd_builder_constants.CDTV_TRADEMARK_FILE);
                default:
                    return null;
            }
        }

        /// <summary>
        /// True when both trademark files are present in <see cref="DataFolder"/> and match their
        /// expected SHA-1 hashes. Does not touch the network.
        /// </summary>
        public static bool HasTrademarkFiles() {
            try {
                return TmFileHelper.CheckTmFiles();
            }
            catch(Exception) {
                return false;
            }
        }

        /// <summary>
        /// Makes sure the Commodore trademark files are available in <see cref="DataFolder"/>.
        /// </summary>
        /// <param name="allowDownload">
        /// When true (the default) and the files are missing, they are downloaded from the third
        /// party archives listed in TmFileSources.json, which involves network requests. When false
        /// this call only reports what is already on disk.
        /// </param>
        /// <remarks>
        /// The trademark files are copyright Commodore and are not distributed with this library;
        /// they are extracted from publicly archived disc images. Calling this ahead of a build lets
        /// an application ask the user before any network access happens, then build with
        /// <see cref="IsoBuildOptions.AutoDownloadTrademarkFiles"/> set to false.
        /// </remarks>
        public static IsoTrademarkStatus EnsureTrademarkFiles(bool allowDownload = true) {
            var status = new IsoTrademarkStatus {
                Cd32FilePath = GetTrademarkFilePath(IsoTargetSystem.CD32),
                CdtvFilePath = GetTrademarkFilePath(IsoTargetSystem.CDTV)
            };

            try {
                if(!Directory.Exists(DataFolder)) {
                    Directory.CreateDirectory(DataFolder);
                }

                if(TmFileHelper.CheckTmFiles()) {
                    status.Available = true;
                    status.Message = "Trademark files are present in " + DataFolder + ".";
                    return status;
                }

                if(!allowDownload) {
                    status.Message =
                        "Trademark files were not found in " + DataFolder +
                        " and downloading is disabled. Supply IsoBuildOptions.TrademarkFile, place " +
                        isocd_builder_constants.CD32_TRADEMARK_FILE + " and " +
                        isocd_builder_constants.CDTV_TRADEMARK_FILE + " in that folder, or allow downloading.";
                    return status;
                }

                // Refreshes the local TmFileSources.json from the repo, then pulls the trademark
                // files out of the archived disc images it points at.
                TmFileHelper.InstallTmSourcesFile();

                status.Available = TmFileHelper.DownloadTmFiles();
                status.Downloaded = status.Available;
                status.Message = status.Available
                    ? "Trademark files downloaded to " + DataFolder + "."
                    : "Could not download the trademark files. Check the network connection, or supply IsoBuildOptions.TrademarkFile yourself.";
            }
            catch(Exception ex) {
                status.Available = false;
                status.Message = "Could not prepare the trademark files: " + ex.Message;
            }

            return status;
        }

        // ---------------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------------

        /// <summary>
        /// Checks the options without building anything. Returns an empty array when they are valid,
        /// otherwise one message per problem.
        /// </summary>
        public static string[] Validate(IsoBuildOptions options) {
            if(options == null) {
                return new[] { "No options were supplied." };
            }

            var errors = new List<string>();

            if(string.IsNullOrWhiteSpace(options.InputFolder)) {
                errors.Add("InputFolder - a path must be provided.");
            }

            if(string.IsNullOrWhiteSpace(options.OutputFile)) {
                errors.Add("OutputFile - a path must be provided.");
            }

            if(errors.Count > 0) {
                return errors.ToArray();
            }

            // Everything else (string lengths, numeric ranges) is enforced by the core Options type.
            var core = ToCoreOptions(options, options.InputFolder, options.OutputFile, options.TrademarkFile);
            var result = core.Validate();

            if(!result.Success) {
                errors.AddRange(result.Message.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            }

            return errors.ToArray();
        }

        // ---------------------------------------------------------------------
        // Build
        // ---------------------------------------------------------------------

        /// <summary>
        /// Builds an ISO image. Runs on the calling thread and does not throw; the outcome, including
        /// any failure, is described by the returned <see cref="IsoBuildResult"/>.
        /// </summary>
        /// <param name="options">The settings to build with. The instance is not modified.</param>
        /// <param name="onProgress">Optional progress callback, invoked on the calling thread.</param>
        /// <param name="cancellationToken">Cancels the build; the partial output file is deleted.</param>
        public static IsoBuildResult Build(
            IsoBuildOptions options,
            Action<IsoBuildProgress> onProgress = null,
            CancellationToken cancellationToken = default(CancellationToken)) {

            if(options == null) {
                return Failed(IsoBuildStatus.InvalidOptions, "No options were supplied.", null, null);
            }

            string inputFolder;
            string outputFile;

            try {
                if(string.IsNullOrWhiteSpace(options.InputFolder)) {
                    return Failed(IsoBuildStatus.InvalidOptions, "InputFolder - a path must be provided.", null, null);
                }

                if(string.IsNullOrWhiteSpace(options.OutputFile)) {
                    return Failed(IsoBuildStatus.InvalidOptions, "OutputFile - a path must be provided.", null, null);
                }

                inputFolder = Path.GetFullPath(options.InputFolder);
                outputFile = Path.GetFullPath(options.OutputFile);
            }
            catch(Exception ex) {
                return Failed(IsoBuildStatus.InvalidOptions, "The input or output path is not valid: " + ex.Message, ex, null);
            }

            // Resolve which trademark file, if any, gets injected.
            string trademarkFile = null;

            if(options.Trademark && options.TargetSystem != IsoTargetSystem.Amiga) {
                if(!string.IsNullOrWhiteSpace(options.TrademarkFile)) {
                    try {
                        trademarkFile = Path.GetFullPath(options.TrademarkFile);
                    }
                    catch(Exception ex) {
                        return Failed(IsoBuildStatus.InvalidOptions, "The trademark file path is not valid: " + ex.Message, ex, null);
                    }

                    if(!File.Exists(trademarkFile)) {
                        return Failed(IsoBuildStatus.InvalidOptions, isocd_builder_constants.ERROR_MESSAGE_TRADEMARK_FILE_MUST_EXIST + " (" + trademarkFile + ")", null, null);
                    }
                }
                else {
                    var tmStatus = EnsureTrademarkFiles(options.AutoDownloadTrademarkFiles);

                    if(!tmStatus.Available) {
                        return Failed(IsoBuildStatus.InvalidOptions, tmStatus.Message, null, null);
                    }

                    trademarkFile = GetTrademarkFilePath(options.TargetSystem);
                }
            }

            var core = ToCoreOptions(options, inputFolder, outputFile, trademarkFile);

            var validation = core.Validate();

            if(!validation.Success) {
                return Failed(IsoBuildStatus.InvalidOptions, validation.Message, null, outputFile);
            }

            if(!Directory.Exists(inputFolder)) {
                return Failed(IsoBuildStatus.InvalidOptions, isocd_builder_constants.ERROR_MESSAGE_INPUT_FOLDER_MUST_EXIST + " (" + inputFolder + ")", null, outputFile);
            }

            var outputFolder = Path.GetDirectoryName(outputFile);

            if(!string.IsNullOrEmpty(outputFolder) && !Directory.Exists(outputFolder)) {
                if(!options.CreateOutputDirectory) {
                    return Failed(IsoBuildStatus.InvalidOptions, isocd_builder_constants.ERROR_MESSAGE_OUTPUT_FOLDER_MUST_EXIST + " (" + outputFolder + ")", null, outputFile);
                }

                try {
                    Directory.CreateDirectory(outputFolder);
                }
                catch(Exception ex) {
                    return Failed(IsoBuildStatus.Error, "Could not create the output folder " + outputFolder + ": " + ex.Message, ex, outputFile);
                }
            }

            return Run(core, trademarkFile, onProgress, cancellationToken);
        }

        /// <summary>
        /// Builds an ISO image on a background thread.
        /// </summary>
        /// <param name="options">The settings to build with. The instance is not modified.</param>
        /// <param name="onProgress">
        /// Optional progress callback. If the calling thread has a synchronisation context - a UI
        /// thread, or a game engine's main thread - callbacks are posted back to it, so it is safe to
        /// touch UI state from them. Otherwise they arrive on the background thread.
        /// </param>
        /// <param name="cancellationToken">Cancels the build; the partial output file is deleted.</param>
        public static Task<IsoBuildResult> BuildAsync(
            IsoBuildOptions options,
            Action<IsoBuildProgress> onProgress = null,
            CancellationToken cancellationToken = default(CancellationToken)) {

            var context = SynchronizationContext.Current;
            Action<IsoBuildProgress> marshalled = onProgress;

            if(onProgress != null && context != null) {
                marshalled = progress => context.Post(state => onProgress((IsoBuildProgress)state), progress);
            }

            return Task.Run(() => Build(options, marshalled, cancellationToken), cancellationToken);
        }

        // ---------------------------------------------------------------------
        // Internals
        // ---------------------------------------------------------------------

        static IsoBuildResult Run(
            Options core,
            string trademarkFile,
            Action<IsoBuildProgress> onProgress,
            CancellationToken cancellationToken) {

            var worker = new BuildIsoWorker();
            IsoBuildResult result = null;

            BuildIsoWorker.WorkerUpdate updateHandler = (sender, e) => {
                if(onProgress == null || e == null || e.State == null) {
                    return;
                }

                var percent = e.State.Progress;

                onProgress(new IsoBuildProgress {
                    Percent = percent < 0 ? 0 : (percent > 100 ? 100 : percent),
                    CurrentEntry = e.State.CurrentEntry,
                    TotalEntries = e.State.TotalEntries,
                    Message = e.State.StatusMessage
                });
            };

            BuildIsoWorker.WorkerCompleted completedHandler = (sender, e) => {
                switch(e.Status) {
                    case WorkerCompletedStatus.Success:
                        result = new IsoBuildResult {
                            Status = IsoBuildStatus.Success,
                            Message = "The image was built successfully.",
                            OutputFile = core.OutputFile,
                            TrademarkFileUsed = trademarkFile
                        };
                        break;

                    case WorkerCompletedStatus.Cancelled:
                        result = Failed(IsoBuildStatus.Cancelled, "The build was cancelled.", null, core.OutputFile);
                        break;

                    default:
                        result = Failed(
                            IsoBuildStatus.Error,
                            e.Exception != null ? e.Exception.Message : "The build failed.",
                            e.Exception,
                            core.OutputFile);
                        break;
                }

                result.TrademarkFileUsed = trademarkFile;
            };

            worker.WorkerUpdateEvent += updateHandler;
            worker.WorkerCompletedEvent += completedHandler;

            // BuildIsoWorker owns its own cancellation source, so an external token is bridged across.
            var registration = cancellationToken.CanBeCanceled
                ? cancellationToken.Register(worker.StopWork)
                : default(CancellationTokenRegistration);

            try {
                // With no AsyncOperation in play the worker raises its events synchronously on this
                // thread, so `result` is guaranteed to be populated by the time StartWork returns.
                worker.StartWork(core);
            }
            catch(Exception ex) {
                return Failed(IsoBuildStatus.Error, ex.Message, ex, core.OutputFile);
            }
            finally {
                registration.Dispose();
                worker.WorkerUpdateEvent -= updateHandler;
                worker.WorkerCompletedEvent -= completedHandler;
            }

            if(result == null) {
                return Failed(IsoBuildStatus.Error, "The build finished without reporting a result.", null, core.OutputFile);
            }

            if(result.Success) {
                try {
                    result.OutputSizeBytes = new FileInfo(core.OutputFile).Length;
                }
                catch(Exception) {
                    // The image is written; not being able to stat it afterwards is not a build failure.
                }
            }

            return result;
        }

        /// <summary>
        /// Projects the public options onto the core builder's options type.
        /// </summary>
        static Options ToCoreOptions(IsoBuildOptions options, string inputFolder, string outputFile, string trademarkFile) {
            var core = new Options {
                InputFolder = inputFolder,
                OutputFile = outputFile,

                VolumeId = options.VolumeId ?? string.Empty,
                PublisherId = options.PublisherId ?? string.Empty,
                ApplicationId = options.ApplicationId ?? string.Empty,
                VolumeSetId = options.VolumeSetId ?? string.Empty,
                DataPreparerId = options.DataPreparerId ?? string.Empty,

                DataCache = options.DataCache,
                DirCache = options.DirCache,
                FileLock = options.FileLock,
                FileHandle = options.FileHandle,
                Retries = options.Retries,

                DirectRead = options.DirectRead,
                FastSearch = options.FastSearch,
                SpeedIndependent = options.SpeedIndependent,

                PadSize = (PadSizeType)options.PadSize,

                GenerateOrderFile = options.GenerateOrderFile,
                UseOrderFile = options.UseOrderFile
            };

            // Order matters: the TargetSystem setter overwrites Trademark, so it has to be assigned
            // before Trademark and TrademarkFile.
            core.TargetSystem = (TargetSystemType)options.TargetSystem;
            core.Trademark = options.Trademark && options.TargetSystem != IsoTargetSystem.Amiga;
            core.TrademarkFile = trademarkFile ?? string.Empty;

            return core;
        }

        static IsoBuildResult Failed(IsoBuildStatus status, string message, Exception exception, string outputFile) {
            return new IsoBuildResult {
                Status = status,
                Message = message,
                Exception = exception,
                OutputFile = outputFile
            };
        }
    }
}

# IsoCd.Builder

A redistributable, dependency-free build of the ISOCD image builder, packaged so it can be called
from any .NET project — a game engine such as Unity, a build script, a pipeline tool, or another
desktop application.

It is the same image builder the ISOCD-Win GUI and ISOCD-Con command-line tool use, just wrapped in
a single small API surface instead of a command line.

## Building the DLL

```
dotnet build src/isocd-lib/isocd-lib.csproj -c Release
```

The output is one file with nothing to copy alongside it:

```
src/isocd-lib/bin/Release/IsoCd.Builder.dll
```

`IsoCd.Builder.xml` is emitted next to it and carries the API documentation, so IntelliSense picks
it up if you copy both.

## Compatibility

The project targets **.NET Standard 2.0** and its only external assembly reference is
`netstandard, Version=2.0.0.0`, so the DLL loads as-is on:

| Host | Notes |
| --- | --- |
| Unity 2018.1+ | Drop `IsoCd.Builder.dll` into `Assets/Plugins/`. Works on either *Api Compatibility Level* (`.NET Standard 2.1` or `.NET Framework`). Nothing in the library references UnityEngine. |
| .NET Framework 4.6.1+ | Direct assembly reference. |
| .NET Core 2.0+ / .NET 5 - 9 | Direct assembly reference. |
| Mono | Direct assembly reference. |

The image builder itself is platform-neutral, but the trademark-file cache defaults to a Windows
shared-documents path. Set [`IsoCdBuilder.DataFolder`](#trademark-files) on other platforms.

## Quick start

```csharp
using IsoCd;

var result = IsoCdBuilder.Build(new IsoBuildOptions {
    InputFolder  = @"C:\MyGame\disc",
    OutputFile   = @"C:\MyGame\build\MyGame.iso",
    TargetSystem = IsoTargetSystem.CD32,
    VolumeId     = "MY_GAME",
    PublisherId  = "MY STUDIO",
    PadSize      = IsoPadSize.Cdr74
}, progress => Console.WriteLine(progress));

if (!result.Success) {
    Console.Error.WriteLine(result.Message);
}
```

`Build` never throws. Every outcome — success, invalid options, a failed build, a cancellation — is
reported through the returned `IsoBuildResult`.

There is also an async form, for keeping a UI or game loop responsive:

```csharp
var result = await IsoCdBuilder.BuildAsync(options, OnProgress, cancellationToken);
```

`BuildAsync` runs the build on a background thread. If the calling thread has a synchronization
context — a UI thread, or a game engine's main thread — progress callbacks are posted back to it, so
they are safe to touch UI state from.

## Trademark files

Booting on a real CD32 or CDTV requires a trademark file (`CD32.TM` / `CDTV.TM`) to be injected into
the image. These are Commodore copyright and are **not** shipped with this library; they are
extracted from publicly archived disc images listed in `TmFileSources.json`.

By default the library manages this itself: if a trademark file is needed and not already cached, it
is downloaded on the first build. Two things to know:

* **Where they are cached** — `IsoCdBuilder.DataFolder`, which defaults to
  `%PUBLIC%\Documents\Amiga Files\ISOCD-Win` so the cache is shared with the GUI and console apps.
  Set it to redirect the library somewhere writable, or to keep the files with your own application:

  ```csharp
  IsoCdBuilder.DataFolder = Path.Combine(Application.persistentDataPath, "isocd");
  ```

* **The download is a network call.** To keep a build entirely offline, either point at a file you
  already have, or pre-fetch once and then disable downloading:

  ```csharp
  // Option A: supply the file yourself, no network access at all.
  options.TrademarkFile = @"C:\amiga\CD32.TM";

  // Option B: ask the user first, fetch once, then build offline from then on.
  if (!IsoCdBuilder.HasTrademarkFiles() && UserAgreed()) {
      var status = IsoCdBuilder.EnsureTrademarkFiles(allowDownload: true);
      if (!status.Available) Console.Error.WriteLine(status.Message);
  }
  options.AutoDownloadTrademarkFiles = false;
  ```

Setting `TargetSystem` to `IsoTargetSystem.Amiga`, or `Trademark` to `false`, builds a plain data
disc and never touches a trademark file.

## All supported options

Every setting on `IsoBuildOptions`, with its default.

### Paths (required)

| Property | Default | Description |
| --- | --- | --- |
| `InputFolder` | — | Folder whose contents become the root of the disc. Relative paths are resolved against the working directory. |
| `OutputFile` | — | Path of the `.iso` to write. An existing file is overwritten. |

### Target system and trademark

| Property | Default | Description |
| --- | --- | --- |
| `TargetSystem` | `CD32` | `CD32`, `CDTV`, or `Amiga` (plain data disc, no trademark). |
| `Trademark` | `true` | Inject a trademark file so the disc boots. Forced off for `Amiga`. |
| `TrademarkFile` | `null` | Explicit `.TM` file to inject. When null, the cached file matching `TargetSystem` is used. |
| `AutoDownloadTrademarkFiles` | `true` | Allow fetching a missing trademark file over the network. |

### Volume descriptor identifiers

| Property | Default | Limit |
| --- | --- | --- |
| `VolumeId` | `"CD32_TEST"` | 32 characters |
| `PublisherId` | `""` | 128 characters |
| `ApplicationId` | `""` | 128 characters |
| `VolumeSetId` | `""` | 128 characters |
| `DataPreparerId` | `""` | 128 characters |

### AmigaDOS CDFS tuning

Written into the boot block. Each is only emitted into the image when it differs from the AmigaDOS
default, matching the original ISOCD behaviour.

| Property | Default | Range |
| --- | --- | --- |
| `DataCache` | `8` | 1 - 127 |
| `DirCache` | `16` | 1 - 127 |
| `FileLock` | `40` | 1 - 9999 |
| `FileHandle` | `16` | 1 - 9999 |
| `Retries` | `32` | 0 - 127 |
| `DirectRead` | `false` | Direct read optimisation. CDTV only. |
| `FastSearch` | `false` | Fast search optimisation. |
| `SpeedIndependent` | `false` | Let newer drives read at higher speeds. |

### Layout

| Property | Default | Description |
| --- | --- | --- |
| `PadSize` | `None` | `None`, `Cdr74`, `Cdr80`, `Cdr90`, `Min1`, `Min10`. Pads the start of the image so data sits on the faster outer tracks. |
| `GenerateOrderFile` | `false` | Write `ISOCD_<VolumeId>.txt` into `InputFolder`, listing every entry, so it can be hand-edited. |
| `UseOrderFile` | `false` | Lay the disc out according to that file. Fails the build if it is missing or invalid. |

### Wrapper convenience (not part of the ISO format)

| Property | Default | Description |
| --- | --- | --- |
| `CreateOutputDirectory` | `true` | Create the output folder if missing, rather than failing. |

## Reading the result

```csharp
public class IsoBuildResult {
    IsoBuildStatus Status;            // Success | Error | Cancelled | InvalidOptions
    bool           Success;           // Status == Success
    string         Message;           // outcome, or the validation / failure reason
    Exception      Exception;         // set when Status == Error
    string         OutputFile;        // absolute path
    long           OutputSizeBytes;
    string         TrademarkFileUsed; // null when none was injected
}
```

Options can also be checked ahead of time, which is handy for validating a form as it is filled in:

```csharp
string[] errors = IsoCdBuilder.Validate(options);   // empty when valid
```

## Cancellation

Pass a `CancellationToken` to either `Build` or `BuildAsync`. On cancellation the partial `.iso` is
deleted and `Status` comes back as `Cancelled`.

```csharp
var cts = new CancellationTokenSource();
// ... from elsewhere: cts.Cancel();
var result = await IsoCdBuilder.BuildAsync(options, OnProgress, cts.Token);
```

## Threading

Run **one build at a time per process**. The underlying stream copy uses a single shared static
buffer, so two builds running concurrently in the same process will corrupt each other's output.
Sequential builds, and `BuildAsync` awaited one at a time, are fine.

`IsoCdBuilder.DataFolder` is process-wide state shared with the rest of the builder library; set it
once during start-up rather than per build.

## Relationship to the rest of the repo

This project compiles the `isocd-builder` sources directly rather than referencing that project, with
`ACTUAL_RELEASE` defined. That switches `Iso9660` from `System.IO.Abstractions` (needed only by the
unit tests) over to plain `System.IO`, which is what makes the resulting DLL dependency-free. The
image-writing code is shared and unmodified, so images built through this API are byte-for-byte what
the GUI produces from the same settings.

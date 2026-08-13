# ISOCD-Win

ISOCD-Win is a C#/.NET Windows replacement for the native Amiga ISOCD application written by [Carl Sassenrath](https://en.wikipedia.org/wiki/Carl_Sassenrath) and other developers at Commodore. It creates bootable ISO image files which are compatible with the Amiga CD32 and CDTV. It was written to avoid the need to run the original ISOCD application either on a real or emulated Amiga, thus greatly simplifying and speeding up the process for the creators of new CDs for Amiga computers and consoles.

![isocd-win-screenshot](https://github.com/fuseoppl/isocd-win/blob/master/isocd-win.jpg)

![isocd-win-options-screenshot](https://github.com/fuseoppl/isocd-win/blob/master/isocd-win-options.jpg)

![isocd-con-screenshot](https://github.com/fuseoppl/isocd-win/blob/master/isocd-con.jpg)

## Features

* Has a simple, user-friendly GUI application
* Also includes a console (command-line) application which supports batch processing
* Creates ISO files compatible with the ISO 9660 file system specification, to be used on big-endian (like the Amiga) and little-endian architectures
* Supports injection of the original trademark files from Commodore to allow discs to be booted on the Amiga CD32 and CDTV
* Uses ISO-8859-1 encoding just like AmigaDOS
* Uses uppercase filenames in the generated ISO 9660 path table just like the original ISOCD (actual filenames are left intact) to make the ISO compatible with AmigaDOS
* Uses a case insensitive sort for the file system entries based on path to make the ISO compatible with AmigaDOS
* Uses sorting via a generated text file with the contents, which can be edited to speed up reading the disc's contents.
* Supports image padding, which adds blank space at the start of the CDR-74, CDR-80 or CDR-90 image to improve the performance of double speed reading on the Amiga CD32 drive
* Building can be aborted mid-process if needed (multi-threaded)
* Supports launching of WinUAE to test built ISO files before burning
* The image building library is a self-contained assembly (DLL) and could easily be used in other .NET applications

## Using ISOCD as a library

`src/isocd-lib` packages the image builder as **IsoCd.Builder.dll**, a single .NET Standard 2.0
assembly whose only external reference is `netstandard 2.0`. It has no NuGet dependencies and nothing
to copy alongside it, so it can be referenced from .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5-9,
Mono, or dropped into a Unity project's `Assets/Plugins` folder.

```
dotnet build src/isocd-lib/isocd-lib.csproj -c Release
```

```csharp
using IsoCd;

var result = IsoCdBuilder.Build(new IsoBuildOptions {
    InputFolder  = @"C:\MyGame\disc",
    OutputFile   = @"C:\MyGame\build\MyGame.iso",
    TargetSystem = IsoTargetSystem.CD32,
    VolumeId     = "MY_GAME",
    PadSize      = IsoPadSize.Cdr74
}, progress => Console.WriteLine(progress));

if(!result.Success) {
    Console.Error.WriteLine(result.Message);
}
```

Every option the GUI exposes is available, including trademark file injection for CD32 and CDTV
booting. See [src/isocd-lib/README.md](src/isocd-lib/README.md) for the full API, the complete
options reference, and notes on managing the trademark files offline.

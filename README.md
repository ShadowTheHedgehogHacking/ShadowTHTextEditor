<div align="center">
<h1>Shadow The Hedgehog Text Editor</h1>
<h3>FNT editor (Subtitles & Voice Associations) for GameCube/Xbox versions of Shadow The Hedgehog</h3>
<img src="https://raw.githubusercontent.com/ShadowTheHedgehogHacking/ShadowTHTextEditor/master/res/preview.jpg" align="center" />
</div>


## Dependencies:
* Requires [.NET 9 Desktop Runtime](https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=x64&apphost_version=9.0.0&gui=true)

## Features:
* Handles the entire /font folder (with locale support) for both the GameCube and Xbox versions
* Add / Edit / Delete subtitle entries
* Filter/search by subtitle in FNT and/or audio filename from AFS archive.
* Support for previewing, linking, replacing, and extracting associated audio from AFS archive. Import/Export as `.wav` or `.adx`. Warnings will be displayed if any issues are detected with replacement files.

## Note:
* By default ShadowTH uses an ampersand (&) to represent the trademark symbol (™). Our 'universal' EN locale .met file makes it so the trademark symbol is represented by the copyright symbol (©️), freeing & for use.
* Shadow the Hedgehog only includes the letters it uses for a particular font file in the .TXD. We handle this automatically for you with EN language, overwritting any modified text's associated MET and TXD by default. If you are editing other languages you will need to manually edit MET and TXD to add any missing letters.
* We provide a universal English (EN) MET & TXD file to support all letters. This file is used unless you uncheck `Do not replace .met and .txd`. If editing other languages, you will need to manually edit these if using letters not used by an edited fnt file.

## Projects that have used this program
* [Shadow the Hedgehog: Reloaded](https://github.com/ShadowTheHedgehogHacking/ShdTH-Reloaded)
* [Tradução PT-BR Shadow The Hedgehog](https://gamebanana.com/mods/529265)
* [Shadow the Hedgehog: Russian Version (GC Port)](https://gamebanana.com/mods/49299)

## Credits:
* Created by dreamsyntax
* .fnt struct reversal done by LimblessVector & dreamsyntax
* .met/.txd EN universal map and design work by TheHatedGravity
* Uses [AfsLib](https://github.com/Sewer56/AfsLib) by Sewer56 for AFS support
* Uses [VGAudio](https://github.com/Thealexbarney/VGAudio) by Alex Barney for ADX playback
* Uses modified version of DarkTheme by [Otiel](https://github.com/Otiel)
* Uses [Ookii.Dialogs](https://github.com/ookii-dialogs/ookii-dialogs-wpf) for dialogs

## Dev / How to build
Release build with
`dotnet publish -c Release --self-contained false /p:PublishSingleFile=true -p:TrimMode=CopyUsed --framework net9.0-windows7.0`
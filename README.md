<div align="center"><h1>Shadow The Hedgehog Text Editor</h1>
<img src="https://raw.githubusercontent.com/ShadowTheHedgehogHacking/ShadowTHTextEditor/master/res/preview.jpg" align="center" />
</div>





FNT editor for GameCube/Xbox versions of Shadow The Hedgehog.

## Dependencies:
* Requires [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime) (or higher).

## Features:
* Handles the entire /font folder (with locale support).
* Support for linking, replacing, and extracting associated audio from AFS archive.
* Filter by subtitle in FNT and/or audio filename from AFS archive.
* Playback of associated audio file
* Switcher for different locales (CURRENTLY ONLY THE ENGLISH (EN) LOCALE CAN SAVE CORRECTLY).
* Automatic SubtitleAddress calculation
* Add / Delete subtitle entries
* Dark Mode


## Note:
By default ShadowTH uses an ampersand (&) to represent the trademark symbol (™).

The provided .met file makes it so the trademark symbol is represented by the copyright symbol (©️), freeing & for use.

## Credits:
* Created by dreamsyntax
* .fnt struct reversal done by LimblessVector & dreamsyntax
* .met/.txd universal map and design work by TheHatedGravity
* Uses [AfsLib](https://github.com/Sewer56/AfsLib) by Sewer56 for AFS support
* Uses [VGAudio](https://github.com/Thealexbarney/VGAudio) by Alex Barney for ADX playback
* Uses modified version of DarkTheme by [Otiel](https://github.com/Otiel)
* Uses [Ookii.Dialogs](https://github.com/ookii-dialogs/ookii-dialogs-wpf) for dialogs
# FortRise - TowerFall Mod Loader
It's a mod loader for [TowerFall Dark World](http://www.towerfall-game.com/) (created by Maddy Thorson). It's using [MonoMod](https://github.com/MonoMod/MonoMod) an open-source C# modding utility.



## Installation
### Full CLI (Recommended as for now)
+ Go to the Releases page, and download the latest installer in zip file.
+ + NoAnsi for Windows, OSXLinux for Mac and Linux.
+ Extract all of the contents and open the command line.
+ Type `Installer.NoAnsi.exe --patch "<Path to TowerFall>"` to do install and patch TowerFall.
+ + `Installer.NoAnsi.bin.osx` for Mac and `Installer.NoAnsi.bin.x86_64` for Linux.
+ Wait for it to patch.
+ And, it's done! Launch TowerFall and it should have a mods section in main menu.
### Pyrgos:
+ Download and Install [pyrgos](https://github.com/Terria-K/pyrgos).
+ Launch pyrgos.
+ Add a TowerFall directory to the launcher.
+ Click the hammer icon next to the play button.
+ Click the installer versions you have in the launcher, if you haven't had one, download an installer inside of the launcher.
+ Wait for it to patch.
+ Done! And click the play button to see the changes.

> [!NOTE]
> Installer.NoAnsi.exe must run through a terminal or a command prompt. It means, you must go to the directory where the Installer.NoAnsi.exe is on by
running `cd` command in the terminal.

## Mod Installation
+ For installing a mod, you must create a directory called `Mods`.
+ Then create a folder inside of it and name what it represent the mod is.
+ Paste all of the mod content and done.
+ Launch TowerFall to see the changes.


## Creating mods
For creating mods, check out [Creating Mods in the wiki](https://github.com/Terria-K/FortRise/wiki/Creating-Mods), for more information.

## FAQ (Frequently Ask Question)
### Can I use FortRise the game without the Dark World DLC?
Yes, you can use FortRise without the Dark World DLC, but it is not recommended to do so. Most mods relies on Dark World for it to work and you might experience a constant crashes without the Dark World DLC.

### The game crashes before it even loads! (Windows only)
If the game crashes before loading, or you see an error in the log file that says `An attempt was made to load an assembly from a network location which would have caused the assembly to be sandboxed in previous versions of the .NET Framework.` on Windows, right click for each DLLs on the root directory, click Properties, and hit the unblock checkbox at the bottom. You might also need to unblock every DLLs of the mod you've installed inside of the Mods folder.
### Pyrgos does not launch
If you are on Windows, you might not have Webview2 installed on your system. Pyrgos relies on Webview2 for UI. If you do not want to install Webview2, you better
go off using the CLI Installer instead. If your pc has it and it still does not work, you can report it into the official TowerFall Discord server under the `#modding` channel.

For Linux, you might need to run it on the terminal to see if there's a missing dependency issues, if you suddenly had one, you may install or update your package directly, depending on your distro. Alternatively, just use the CLI version, pyrgos are just an interface for the CLI anyway.
### Join the official TowerFall Discord Server, we can help you here!
<a href="https://discord.gg/m25mWsSv8P">
 <img alt="TowerFall" src="https://discordapp.com/api/guilds/248961953656078337/embed.png?style=banner2" />
</a>


# Mods Created in FortRise
## [Archer Loader](https://github.com/RedDude/ArcherLoader/releases/)
### By: [RedDude](https://github.com/RedDude)
Add Custom Archers for Towerfall easily by just Drag n Drop and create Archers with custom hair, wings, ghost, particles, layers and even taunts and skins for archers.
![archer-loader](./img/archer-loader.png)

## [TF.EX (TowerFall NetPlay WIP)](https://github.com/Fcornaire/TF.EX)
### By: [DShad](https://github.com/Fcornaire)
TF EX is a mod that attempt to bring netplay to TowerFall (EX as a Fighting game EX move, usually costing meter).
![netplay](https://github.com/Fcornaire/TF.EX/blob/main/images/demo.gif)

## [Eight-Player Mode (WIP)](https://github.com/Terria-K/EightPlayerMod)
### By: [Terria](https://github.com/Terria-K) and [RedDude](https://github.com/RedDude)
Ported 8P mode as a FortRise mod with wider level set and additional Co-Op wide quest.
![eight-player-versus](./img/eight-player-versus.png)
![eight-player-quest](./img/eight-player-quest.png)

## [Oops! All Arrows Mod](https://github.com/CoolModder/Towerfall-Redemption/releases/tag/v3.1.0-Arrows)
### By: [CoolModder](https://github.com/CoolModder)
Enjoy the chaos of ten new arrows, with more on the way!
![oops-all-arrows](https://github.com/CoolModder/Towerfall-Redemption/blob/main/giphy.gif)

## Roadmap
Check the [roadmap](./ROADMAP.md) for planned features.


## Additional Credits
DRSkipper - for Maddy text splash screen.
Challengin' Chuck
strongsand
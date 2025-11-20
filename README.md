# FortRise - TowerFall Mod Loader
It's a mod loader for [TowerFall Dark World](http://www.towerfall-game.com/) (created by Maddy Thorson). 


FortRise is a main successor to [Bartizan](https://github.com/Kha/Bartizan) a modding framework exists for modding TowerFall.
FortRise did more than just a modding framework, it is a mod loader and an API utility developed with a help of TowerFall community and uses of
[MonoMod](https://github.com/MonoMod/MonoMod) patcher.

The main purposes of FortRise:
1. Mods are loaded inside of a single folder named `Mods`, it can be either a folder or a zip file as long as `meta.json` is present on its root.
2. It does not write anything into the base TowerFall directory, it should only write into its own folder as it should.
3. Contains most of a the features of TowerFall that you can load directly with a mod such as Levels, Archers, Arrows, etc.. This makes it easy to add new features to the game. 
4. It has its own filesystem API for mods to interact with their own files. Mods should not write or read anything outside of its context, but this cannot be enforced directly.
5. File logging across all mods and game logs to help figure out any specific issues to the mods installed.
6. All mods have a separate save file to save its data instead of using the original saves.
7. Stable and powerful patching utility with [Harmony](https://harmony.pardeike.net/).
> While FortRise tried to not touch the main game files, it still does modify your save file. Currently, no mods are touching the vanilla save
> file yet.


## Installation
For installation refer to [Installation Guide](res/guide/Installation.md).

## Creating mods
For creating mods, check out [Creating Mods in the wiki](https://github.com/Terria-K/FortRise/wiki/Creating-Mods), for more information.

## Migrating your mod to v5.0
See the [migration](./MIGRATION.md) guide here.


## FAQ (Frequently Ask Question)
### Can I use FortRise the game without the Dark World DLC?
Yes, you can use FortRise without the Dark World DLC, but it is not recommended to do so. Most mods relies on Dark World for it to work and you might experience a constant crashes without the Dark World DLC.
### Does FortRise modify the executable from my TowerFall directory?
No, FortRise just create its own copy of a TowerFall executable from your TowerFall directory and patch that copy instead, the copy will be located relative to the FortRise executable itself. 
This ensures that the original stays unmodified, the only thing that FortRise touches is your save data.


### Join the official TowerFall Discord Server, we can help you here!
<a href="https://discord.gg/m25mWsSv8P">
 <img alt="TowerFall" src="https://discordapp.com/api/guilds/248961953656078337/embed.png?style=banner2" />
</a>

## Install Mods
You can directly install mods here in [Gamebanana](https://gamebanana.com/games/18654). Most mods are also on the Discord Server #mod-showcase channels.

## Compiling FortRise
+ If you haven't already, install .NET 10.0 SDK.
+ Clone the FortRise repo recursively (must include submodules) in a CLI or in your IDE.
+ Restore the nuget packages.
+ Run the `./setup.sh` and wait for it to build. (Some packages might fail, but that is expected).
+ + (Help Wanted): translate the script into batch file with proper testing.


## Additional Credits
+ [Shockah](https://github.com/Shockah) ([Nickel](https://github.com/Shockah/Nickel) mod loader) - For the inspiration with the registry pattern.
+ DRSkipper - For Maddy text splash screen.
+ Challengin' Chuck
+ strongsand

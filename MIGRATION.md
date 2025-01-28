# FortRise v5.0 Migration Guide
Since FortRise v5.0 changes and removes a lot of API, some of the code needed to be change in order for a mod made in v4 to work with v5.
FortRise is currently cleaning up its API for the release of v6.0.

FortRise v5.0 might not be a big release, but can improves the development of the mod loader faster than what it was before.

## Table of Contents
1. [[#Required Migration]]
    1. [[#Metadata Changes]]
	2. [[#OnTower API Changes]]
	3. [[#Custom RoundLogic API Changes]]
	4. [[#OnVariantRegister Changes]]
	5. [[#Arrow and Pickup Registry Changes]]
    6. [[#VanillaAtlas removal]]
2. [[#Optional Migration]]
	1. [[#Using textures with an Atlas]]
    2. [[#Creating an Atlas]]
3. [[#Renamed APIs]]
4. [[#Removed APIs]] 
5. [[#Additional Information]]

# Required Migration
These migrations are required to work in v5.0 and must be changed before it is going to work.

## Metadata Changes
Metadata is an important step when creating a mod, but something has to change with this design as well.
FortRise no longer accepts `meta.hjson` since it is not usually used in any way, and just adds another bloat and complexity on the loader.

Second of all, FortRise would no longer accepts names that has a special characters and spaces in it except `.` and `_`. Which means
you had to change the mod name into a name that could accepts: [Aa-Zz0-9._].


## OnTower API Changes
OnTower patcher were removed in favor of a new TowerPatch API. If you are using this API correctly, migrating to it will be a breeze.
Some mods that has a custom arrows with customized rate must need to move to TowerPatch API immediately.

### Old
```csharp
// OnTower API (deprecated)
public override void Initialize() 
{
    new OnTower(this)
        .Versus_Flight
        .IncreaseTreasureRates(FortRise.RiseCore.PickupRegistry["ModName/Registry"].ID);
}
```

### New
```csharp
// TowerPatch API
public class Flight : TowerPatch
{
    public override void VersusPatch(VersusPatchTowerContext context) 
    {
        context.IncreaseTreasureRates(FortRise.RiseCore.PickupRegistry["Modname/Registry"].ID);
    }
}
```


## Custom RoundLogic API Changes
Custom RoundLogic API has been changes to properly add session-based Round Logic. RoundLogic were not a single registry anymore
and is accompany by the new Gamemode API which we will cover later.
This gave modders a freedom to do whatever they want to a RoundLogic API (which were vanilla-based).

### Old
```csharp
// Old CustomRoundLogic API
[CustomRoundLogic("SomeCustomRoundLogic")]
public class SomeCustomRoundLogic : CustomVersusRoundLogic 
{
    // you need to create this magical static method
    public static RoundLogicInfo Create() 
    {
        return new RoundLogicInfo {
            Name = "Some Custom",
            Icon = YourModModule.YourLoadedAtlas["gamemodes/custommode"],
            RoundType = RoundLogicType.HeadHunters // ??? can I create my own??
        };
    }

    public SomeCustomRoundLogic(Session session) : base(session, false) 
    {

    }
}
```

### New
It's exactly the same without the magical static method and it uses vanilla HeadhuntersRoundLogic.
Use RoundLogic if you want to create your own.
```csharp 
public class SomeCustomRoundLogic : HeadhuntersRoundLogic 
{
    public SomeCustomRoundLogic(Session session) : base(session, false) 
    {

    }
}
```

You will then accompany it with your own CustomGameMode class:

```csharp
public class SomeCustom : CustomGameMode 
{
    public override void Initialize() 
    {
        // Name attributes are optional, it will use your class name when not specified.
        Icon = TFGame.Atlas["YourModName/gamemodes/custommode"];
        NameColor = Color.Yellow; // you can even assign your own color
    }

    public override void InitializeSounds() 
    {
        // because of the way how FortRise loads Audio, they're separated.
    }

    public override RoundLogic CreateRoundLogic(Session session) 
    {
        return new SomeCustomRoundLogic(session);
    }
}
```
Additionally, you can modify the constructor of your RoundLogic to allow it to pass the CustomGameMode treating it as a custom session.
Then, you will use the `StartGame` method to initialize your state.
You can also modify the coin sprite as well as the sound of the coin with this API.
If you want to use the Skull Sprite from the head hunter gamemode, use `UseSkullSprite` and return it inside of `CoinSprite` method.

## OnVariantRegister Changes
We just had to change how variant registry works and touching `MatchVariants` are a bad idea in its mind. So we change the function signature
of this method and add `VariantManager`. It is also confusing why we need to pass an Atlas instead of a texture for an icon so that's why
the change has made.

### Old
```csharp
public override void OnVariantsRegister(MatchVariants variants, bool noPerPlayer = false) 
{
    var info = new VariantInfo(MyModModule.Atlas);
    var customVariant = variants.AddVariant("Variant Name", info, VariantFlags.PerPlayer);
    customVariant.IncompatibleWith(variants.NoTimeLimit);
}
```

### New
```csharp
public override void OnVariantsRegister(VariantManager variants, bool noPerPlayer = false) 
{
    var info = new CustomVariantInfo("Variant Name", TFGame.Atlas["MyMod/variantIcon/variantname"], CustomVariantFlags.PerPlayer);
    var customVariant = variants.AddVariant(info, noPerPlayer);
    variants.CreateLinks(variants.NoTimeLimit, customVariant);
}
```

## Arrow and Pickup Registry Changes
RiseCore for some reason hold these variables before for no good reason. So the change has made to be more
consistent.

### Old
```csharp
FortRise.RiseCore.PickupLoader
FortRise.RiseCore.PickupRegistry

FortRise.RiseCore.ArrowsRegistry
FortRise.RiseCore.ArrowsID
FortRise.RiseCore.Arrows
FortRise.RiseCore.ArrowNameMap
```
There's also a method that can be used to get an arrow type id, but this is also has been removed.
```csharp
FortRise.RiseCore.GetArrowID(String);
```

### New
This is what you should be using now.
```csharp
FortRise.ModRegisters.ArrowData<T>()
FortRise.ModRegisters.ArrowType<T>()
FortRise.ModRegisters.PickupData<T>()
FortRise.ModRegisters.PickupType<T>()
```
For simplicity sake of this documentation, I did not include the other function signature which takes string instead of T. 

You can still access the specific Registers as well like `PickupRegistry` and `ArrowRegistry` which contains the under the hood
implementation of these `ModRegisters`.

## VanillaAtlas removal
To iterate what this is, this feature is for replacing TowerFall's texture directly from the Atlas. This uses a separate directory
from the Mod's Atlas and it also uses the Atlas format to replace.

For the texture to be replaced, the path of the texture from VanillaAtlas must match from the texture from TowerFall's main Atlas.

This feature has been replaced along with the new redesigned atlas system. First, read this section: [[##Creating an Atlas]], then
to replace the texture, you must surround the image file with parenthesis `(filename.png)` to replace the texture. Note that the path
must match to the texture from TowerFall's main Atlas in order to be replaced.

Not surrounding the image file will put the texture into the Mod's atlas and prefixed with the metadata name of a mod 
which is the intended behavior if you wanna use your own texture to other stuff of your mod.

# Optional Migration
These migrations are optional and is not needed to be migrated. Although it covers a good practices on what you should do to
modding with this mod loader.

## Using textures with an Atlas

### Old
Before, if you needed to have a custom textures in your mod, you need to load your atlas from your mod directory. In which case, you might
stumble across the usage of `Content.LoadAtlas`. You do this in your module's LoadContent. You load xml first then the image second.
Then store the output at the top level of your Module class

```csharp
public Atlas MySingleAtlas;

public override void LoadContent() 
{
    MySingleAtlas = Content.LoadAtlas("Atlas/atlas.xml", "Atlas/atlas.xml");
}
```

Then you use this like this:
```csharp
MyModModule.Instance.MySingleAtlas["mytextures"];
```

### New
Now, you don't need load your textures inside of your Atlas as long as you put your atlas inside of an Atlas and named both xml and image as atlas.
You can now use the Vanilla `TFGame.Atlas` for looking up followed by the metadata name of your mod.

```csharp
TFGame.Atlas["MyMod/mytextures"];
```

## Creating an Atlas
Before, you had to create your own Atlas which means, you had to merge all of the images into one single image, and map its location and
size in a single xml file. This is tedious and not so fun to do.

There are many tool to autogenerate your Atlas like [cluttered](https://github.com/Terria-K/cluttered) which helps generating an Atlas for
you.

Luckily, in FortRise v5.0, there is no need for that. FortRise will now generate an Atlas at load times for you. When generated, it will
put into one of the Vanilla Atlas such as Atlas, BGAtlas, MenuAtlas, and BossAtlas depending on where you want to put it.

To start creating one, you will need to create a folder called `atlas`, `menuAtlas`, `bgAtlas`, or `bossAtlas` in the Atlas folder
on your mod's content, choose what you only need.

You can then put an images there like so and you don't need to do much, and that's it. Use the 
Vanilla Atlas to look for your textures.


# Renamed APIs
These following methods and classes are renamed on v5.0. It could be a namespace changed, or causes by full reworked of the APIs.
+ TowerFall.UIModal -> FortRise.UIModal
+ TowerFall.ArrowObject -> FortRise.ArrowData
+ TowerFall.ArrowInfo -> FortRise.ArrowInfo
+ TowerFall.CustomArrowsAttribute -> FortRise.CustomArrowsAttribute
+ TowerFall.ContentAccess -> FortRise.ContentAccess
+ FortRise.PickupObject -> FortRise.PickupData
+ FortRise.RiseCore.ParseMetadata -> ModuleMetadata.ParseMetadata

# Removed APIs
These following methods and classes are removed on v5.0. If you needed these features, feel free to backport it inside your mods.
+ FortRise.SoundHelper
+ FortRise.Option\<T\>
+ FortRise.JsonUtils.GetJsonValueOrNone
+ FortRise.ITowerPatcher
+ FortRise.OnTower
+ FortRise.CustomRoundLogicAttribut
+ TowerFall.CustomVersusRoundLogic
+ TowerFall.RoundLogicInfo
+ TowerFall.RoundLogicType
+ TowerFall.UploadMapButton
+ TowerFall.Atlas.Create
+ TowerFall.Atlas.DataPath
+ TowerFall.Atlas.Digest
+ TowerFall.Atlas.MapAllAssets
+ TowerFall.SpriteData.Create
+ Towerfall.GotoAdventureButton
+ TowerFall.MatchVariants.AddVariant
+ TowerFall.MatchVariants.GetVariantIconFromName
+ TowerFall.MatchVariants.CreateCustomLinks
+ TowerFall.VariantInfo
+ TowerFall.VariantFlags
+ Monocle.Calc.IncompatibleWith
+ FortRise.RiseCore.ParseMetadataWithJson
+ FortRise.RiseCore.ParseMetadataWithHJson
+ FortRise.ThemeResource
+ FortRise.FortContent.GetContentPath
+ FortRise.ModuleSaveData.Load
+ FortRise.ModuleSaveData.Save
+ FortRise.ClosedFormat
+ FortRise.SaveDataFormat
+ FortRise.SaveDataFormatExt
+ FortRise.JsonSaveDataFormat
+ FortRise.JsonBinarySaveDataFormat

### With signatures
+ TowerFall.AtlasExt.CreateAtlas(FortContent, string, string, bool, ContentAccess)
+ TowerFall.TowerTheme.ctor(XmlElement, RiseCore.Resource  FortRise.ThemeResource)
+ TowerFall.TowerTheme.ctor(Hjson.Value, RiseCore.Resource  FortRise.ThemeResource)
+ FortRise.FortContent.ctor(ModuleMetadata, RiseCore.ModResource)
+ FortRise.FortContent.ctor(string, RiseCore.ModResource)
+ FortRise.RiseCore.Resource.ModResource.ctor(string)

# Additional Information
## DotNetZip has been replaced with System.IO.Compression
Due to the reason that DotNetZip has been discontinued, we replaced the zip library to System.IO.Compression to ensure
the quality of the dependencies that we are using and reduces the risk of having a vulnerable code in the future.
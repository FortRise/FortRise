# FortRise v5.0 Migration Guide
Since FortRise v5.0 changes and removes a lot of API, some of the code needed to be change in order for a mod made in v4 to work with v5.
FortRise is currently cleaning up its API for the release of v6.0.

FortRise v5.0 might not be a big release, but can improves the development of the mod loader faster than what it was before.

# Required Migration
These migrations are required to work in v5.0 and must be changed before it is going to work.

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
Now, you need load your textures inside of your Atlas as long as you put your atlas inside of an Atlas and named both xml and image as atlas.
You can now use the Vanilla `TFGame.Atlas` for looking up followed by the metadata name of your mod.

```csharp
TFGame.Atlas["MyMod/mytextures"];
```


# Renamed APIs
These following methods and classes are renamed on v5.0. It could be a namespace changed, or causes by full reworked of the APIs.
+ TowerFall.UIModal -> FortRise.UIModal
+ TowerFall.ArrowObject -> FortRise.ArrowObject
+ TowerFall.ArrowInfo -> FortRise.ArrowInfo
+ TowerFall.CustomArrowsAttribute -> FortRise.CustomArrowsAttribute
+ TowerFall.ContentAccess -> FortRise.ContentAccess

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
+ TowerFall.SpriteData.Create

### With signatures
+ TowerFall.AtlasExt.CreateAtlas(FortContent, string, string, bool, ContentAccess)
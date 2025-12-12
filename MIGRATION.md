# FortRise v5.0 Migration Guide
Since FortRise v5.0 changes and removes a lot of API, some of the code needed to be change in order for a mod made in v4 to work with v5.
FortRise v5.0 has been changed to be a huge major release.

FortRise v5.0 might not be a big release, but can improves the development of the mod loader faster than what it was before.

## Table of Contents
1. [Metadata Changes](#metadata-changes)
2. [Using textures with an Atlas](#using-textures-with-an-atlas)
3. [Registering and Using SpriteDatas](#using-spritedatas)
4. [OnTower API Changes](#ontower-api-changes)
5. [Custom RoundLogic API Changes](#custom-roundlogic-api-changes)
6. [OnVariantRegister Changes](#onvariantregister-changes)
7. [Variant Prefixed](#variant-prefixed)
8. [Arrow Info and Pickup changes](#arrow-info-and-pickup-changes)
9. [Arrow and Pickup Registry Changes](#arrow-and-pickup-registry-changes)
10. [Hjson and TeuJson Removal](#hjson-and-teujson-removal)
11. [TowerFall DLL](#towerfall-dll)

These migrations are required to work in v5.0 and must be changed before it is going to work.

## Metadata Changes
Metadata is an important step when creating a mod, but something has to change with this design as well.
FortRise no longer accepts `meta.hjson` since it is not usually used in any way, and just adds another bloat and complexity on the loader.

Second of all, FortRise would no longer accepts names that has a special characters and spaces in it except `.` and `_`. Which means
you had to change the mod name into a name that could accepts: [Aa-Zz0-9._].

Third, the dependencies need to explicitly depends on a certain version of FortRise to ensure backward compatibility for the forseeable future,
you will need to write this in your dependencies field:
```json
{
    // Other stuff above
    "dependencies": [
        {
            "name": "FortRise",
            "version": "5.0.0"
        }
    ]
}
```

For content mods that may load predefined xml file, you also need to put `FortRise.Content` as its dependencies, and `version: "5.0.0"` for its version.

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
Register the texture through a registry.

```csharp
MyTexture = context.Registry.Subtextures.RegisterTexture(
    content.Root.GetRelativePath("path/to/myTexture.png"),
    // SubtextureAtlasDestination.Atlas (this could be required in some situation such as sprite creation)
);
```

## Registering and Using Sprites
Register the sprites through a registry. You can use either `int` or `string` depends on how you would want to set up your frame.
```csharp
MySprite = context.Registry.Sprites.RegisterSprite<int>(new() {
    Texture = MyTexture,
    FrameWidth = 20,
    FrameHeight = 20,
    Animations = [
        new() { ID = 0, Frames = [0, 1, 2, 3], Delay = 0.1f, Loop = false }
    ]
});

// Sprite may depend on what it needs and some registry accepts a type-safe entry as well.

// using it
AMethodThatAcceptsSprite(
    MySprite.GetCastEntry<int>().Sprite 
);
```

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
        .IncreaseTreasureRates(FortRise.RiseCore.PickupRegistry["ModName/TestArrow"].ID);
}
```

### New
```csharp
// TowerPatch API
public class Flight : ITowerPatch
{
    // TargetTowers has to be explicitly defined.
    public HashSet<string> TargetTowers => new HashSet<string>() { "Flight" }; 
    // To affect all towers instead, use this. This will ignore the TargetTowers property.
    // public bool IsGlobal => true;

    public void VersusPatch(VersusPatchTowerContext context) 
    {
        // you might have an arrow entry called `TestArrow`.
        context.IncreaseTreasureRates(TestArrow.Pickups);
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
    public SomeCustomRoundLogic(Session session) : base(session, false) {}
}
```

You will then accompany it with your own CustomGameMode class:

```csharp
public class SomeCustom : IVersusGamemode
{
    public string Name => "Some Custom";
    public Color NameColor => Color.Yellow;
    public ISubtextureEntry Icon => SomeCustomIcon; // you need to register the icon via the texture registry.

    public RoundLogic OnCreateRoundLogic(Session session) 
    {
        return new SomeCustomRoundLogic(session);
    }
}
```
Additionally, you can modify the constructor of your RoundLogic to allow it to pass the CustomGameMode treating it as a custom session.
Then, you will use the `OnStartGame` method to initialize your state.
You can also modify the coin sprite as well as the sound of the coin with this API.
If you want to use the Skull Sprite from the head hunter gamemode, use `DeathSkull.GetSprite` and return it inside of `OverrideCoinSprite` method.

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
// this does not require any of method, just use the Registry
CustomVariant = context.Registry.Variants.RegisterVariant("VariantID", new() {
    Title = "VARIANT NAME",
    Icon = VariantIcon,
    Flags = CustomVariantFlags.PerPlayer
});


// It is recommended to store the variant entry somewhere else in your code as it will be used
// for checking if this variant is active.
```

## Arrow Info and Pickup changes
Arrow API has changes once again and its for the better to maintain FortRise's stability. This changes affects all mods that has custom
arrows registered.

This will now allows the mod developer to customize the pickup entity for arrows they wanted.

# Old
The arrows and pickups were in a same place.

```csharp
[CustomArrow("Test/Test", nameof(CreateGraphicPickup))]
public class TestArrow : Arrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }

    public static ArrowInfo CreateGraphicPickup() 
    {
        var graphic = new Sprite<int>(MyMod.ArrowAtlas["TestArrowPickup"], 12, 12, 0);
        graphic.Add(0, 0.3f, new int[2] { 0, 0 });
        graphic.Play(0, false);
        graphic.CenterOrigin();
        var arrowInfo = ArrowInfo.Create(graphic, MyMod.ArrowAtlas["TestArrowHud"]);
        arrowInfo.Name = "Test Arrows";
        return arrowInfo;
    }
}
```

# New
Use the two registry which is Pickup and Arrows.

```csharp
TestArrowHud = context.Registry.Subtextures.RegisterTexture(
    content.Root.GetRelativePath("path/to/testArrowHud.png")
);

TestArrowPickup = context.Registry.Subtextures.RegisterTexture(
    content.Root.GetRelativePath("path/to/testArrowPickup.png")
);

TestArrowPickup = context.Registry.Pickups.RegisterPickup(new() {
    Name = "Test",
    PickupType = typeof(TestArrowPickup)
    // assign Color and ColorB to change the text color
});

TestArrow = context.Registry.Arrows.RegisterArrow(new() {
    ArrowType = typeof(TestArrow),
    HUD = TestArrowHud
    // you can also make this a LowPriority arrow which on pickup will acts like 
    // Normal or Jester that will be overriden by a better arrow
});

public class TestArrowPickup : ArrowTypePickup
{
    public TestArrowPickup(Vector2 position, Vector2 targetPosition, ArrowTypes type) : base(position, targetPosition, type)
    {
        var graphic = new Sprite<int>(TestArrowPickup, 12, 12, 0);
        graphic.Add(0, 0.3f, new int[2] { 0, 0 });
        graphic.Play(0, false);
        graphic.CenterOrigin();
        AddGraphic(graphic);
    }
}
public class TestArrow : Arrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
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

### New
Use the new registry for the arrows instead.
```csharp
// First, register your arrows.
ArrowEntry = context.Registry.RegisterArrow(/* some code config here */);
// Then, use its ArrowTypes property to get its enum ID for checking purposes.
ArrowEntry.ArrowType;
```


## Hjson and TeuJson Removal
Hjson and TeuJson has been removed completely, which means you will not have an access to the Hjson format anymore, and TeuJson
will cause a compile error.

Use `System.Text.Json` instead and it add this to your dependencies if needed.

## TowerFall DLL
Due to the recent change that Installer being replaced with a Launcher instead, the `TowerFall.exe` is no longer reliable to be used
as a dependency for modding. Luckily, the Launcher still spits out a modified version of `TowerFall.exe` which is called 
`TowerFall.Patch.dll` after running the launcher for the first time.

So instead of referencing TowerFall with a HintPath of `TowerFall.exe`, you now change the HintPath to `TowerFall.Patch.dll`.

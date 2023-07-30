using System;
using FortRise;
using Monocle;

namespace TowerFall;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CustomArrowsAttribute : Attribute 
{
    public string Name;
    public string GraphicPickupInitializer;

    public CustomArrowsAttribute(string name, string graphicPickupFn = null) 
    {
        Name = name;
        GraphicPickupInitializer = graphicPickupFn;
    }
}

public class ArrowObject
{
    public ArrowTypes Types;
    public Pickups PickupType;
    public TreasureChest.Types SpawnType;
    public ArrowInfoLoader InfoLoader;
}

public struct ArrowInfo 
{
    public string Color = "F7EAC3";
    public string ColorB = "FFFFFF";
    public string Name = "";
    public TreasureChest.Types SpawnType;
    internal Image Simple;
    internal Subtexture HUD;
    internal Sprite<int> Animated;
    internal byte Type;

    [Obsolete("Use ArrowInfo.Create or ArrowInfo.CreateAnimated instead")]
    public ArrowInfo()
    {
        Simple = null;
        Animated = null;
        HUD = null;
        Type = 0;
        SpawnType = TreasureChest.Types.Normal;
    }
#pragma warning disable CS0618

    public static ArrowInfo Create(Image simple, TreasureChest.Types spawnType) 
    {
        return new ArrowInfo { Simple = simple, Type = 0, SpawnType = spawnType };
    }

    public static ArrowInfo Create(Image simple) 
    {
        return new ArrowInfo { Simple = simple, Type = 0 };
    }

    public static ArrowInfo CreateAnimated(Sprite<int> animated) 
    {
        return new ArrowInfo { Animated = animated, Type = 1 };
    }

    public static ArrowInfo CreateAnimated(Sprite<int> animated, TreasureChest.Types spawnType) 
    {
        return new ArrowInfo { Animated = animated, Type = 1, SpawnType = spawnType };
    }

    public static ArrowInfo Create(Image simple, Subtexture hud, TreasureChest.Types spawnType) 
    {
        return new ArrowInfo { Simple = simple, Type = 0, HUD = hud, SpawnType = spawnType };
    }

    public static ArrowInfo CreateAnimated(Sprite<int> animated, Subtexture hud, TreasureChest.Types spawnType) 
    {
        return new ArrowInfo { Animated = animated, Type = 1, HUD = hud, SpawnType = spawnType };
    }

    public static ArrowInfo Create(Image simple, Subtexture hud) 
    {
        return new ArrowInfo { Simple = simple, Type = 0, HUD = hud };
    }

    public static ArrowInfo CreateAnimated(Sprite<int> animated, Subtexture hud) 
    {
        return new ArrowInfo { Animated = animated, Type = 1, HUD = hud };
    }
}
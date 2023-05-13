using System;
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

public struct ArrowInfo 
{
    public string Color = "F7EAC3";
    public string ColorB = "FFFFFF";
    public string Name = "";
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
    }
#pragma warning disable CS0618

    public static ArrowInfo Create(Image simple) 
    {
        return new ArrowInfo { Simple = simple, Type = 0 };
    }

    public static ArrowInfo CreateAnimated(Sprite<int> animated) 
    {
        return new ArrowInfo { Animated = animated, Type = 1 };
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
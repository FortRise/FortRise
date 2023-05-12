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
    internal Image Simple;
    internal Sprite<int> Animated;
    internal byte Type;

    public static ArrowInfo Create(Image simple) 
    {
        return new ArrowInfo { Simple = simple, Type = 0 };
    }

    public static ArrowInfo CreateAnimated(Sprite<int> animated) 
    {
        return new ArrowInfo { Animated = animated, Type = 1 };
    }
}
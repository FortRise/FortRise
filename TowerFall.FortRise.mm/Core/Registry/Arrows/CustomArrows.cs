using System;
using Monocle;
using TowerFall;
using Microsoft.Xna.Framework;

namespace FortRise;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class CustomArrowsAttribute : Attribute 
{
    public string Name;
    public string CreateHud;
    public float Chance = 1f;

    public CustomArrowsAttribute(string name, string createHudFn = null) 
    {
        Name = name;
        CreateHud = createHudFn;
    }

    public CustomArrowsAttribute(string name, float chance, string createHudFn = null) 
    {
        Name = name;
        CreateHud = createHudFn;
        Chance = chance;
    }
}

public class ArrowData
{
    public ArrowLoader ArrowLoader;
    public string Name;
    public ArrowTypes Types;
    public ArrowHUDLoader HudLoader;
}
using System;
using TowerFall;

namespace FortRise;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CustomPickupAttribute : Attribute 
{
    public string Name;
    public string GraphicPickupInitializer;
    public float Chance = 1f;

    public CustomPickupAttribute() {}

    public CustomPickupAttribute(string name, string init = "Init") 
    {
        Name = name;
        GraphicPickupInitializer = init;
    }

    public CustomPickupAttribute(string name, float chance, string init = "Init") 
    {
        Name = name;
        GraphicPickupInitializer = init;
        Chance = chance;
    }
}

public class PickupData 
{
    public string Name;
    public Pickups ID;
    public float Chance;
    public PickupLoader PickupLoader;
}

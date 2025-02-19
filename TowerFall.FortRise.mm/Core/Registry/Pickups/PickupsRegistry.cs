using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace FortRise;

public static class PickupsRegistry 
{
    public static Dictionary<Pickups, PickupData> PickupDatas = new Dictionary<Pickups, PickupData>();
    public static Dictionary<string, Pickups> StringToTypes = new();
    public static Dictionary<Type, Pickups> Types = new();
    public static List<Pickups> ArrowPickups = new List<Pickups>();
    public static List<Type> ArrowPickupsType = new List<Type>();
    public static Dictionary<ArrowTypes, Pickups> ArrowToPickupMapping = new Dictionary<ArrowTypes, Pickups>();

    public static string TypesToString(Pickups pickups)
    {
        if ((int)pickups < 21)
        {
            return pickups.ToString();
        }

        if (PickupDatas.TryGetValue(pickups, out var data))
        {
            return data.Name;
        }

        Logger.Error("[PickupRegistry] Unknown Pickups type passed");
        return null;
    }


    public static void Register<T>(FortModule module) 
    {
        Register(typeof(T), module);
    }

    public static void Register(Type type, FortModule module) 
    {
        foreach (var pickup in type.GetCustomAttributes<CustomPickupAttribute>()) 
        {
            if (pickup is null)
                continue;

            var pickupName = pickup.Name ?? $"{type.Namespace}.{type.Name}";
            Register(type, module, pickupName, pickup.Chance);
        }

        foreach (var pickup in type.GetCustomAttributes<CustomArrowPickupAttribute>()) 
        {
            if (pickup is null)
                continue;

            var pickupName = pickup.Name ?? $"{type.Namespace}.{type.Name}";
            var arrowType = pickup.ArrowType;

            ConstructorInfo ctor = type.GetConstructor(new Type[3] { typeof(Vector2), typeof(Vector2), typeof(ArrowTypes) });
            PickupLoader loader = null;

            var t = ArrowsRegistry.Types[arrowType];

            if (ctor != null) 
            {
                loader = (pos, targetPos, idx) => (ArrowTypePickup)ctor.Invoke(new object[3] { pos, targetPos, t });
            }
            AddToArrowPickupTypeList(arrowType);
            Register(type, module, pickupName, pickup.Chance, loader);
        }
    }

    public static void Register(Type type, FortModule module, string name, float chance)
    {
        ConstructorInfo ctor = type.GetConstructor(new Type[2] { typeof(Vector2), typeof(Vector2) });
        PickupLoader loader = null;

        if (ctor != null) 
        {
            loader = (pos, targetPos, idx) => 
            {
                var custom = (Pickup)ctor.Invoke(new object[2] { pos, targetPos });
                if (custom is CustomOrbPickup customOrb) 
                {
                    var info = customOrb.CreateInfo();
                    customOrb.Sprite = info.Sprite;
                    customOrb.LightColor = info.Color.Invert();
                    customOrb.Collider = info.Hitbox;
                    customOrb.Border.Color = info.Color;
                    customOrb.Sprite.Play(0);
                    customOrb.Add(customOrb.Sprite);
                }

                return custom;
            };
        }
        Register(type, module, name, chance, loader);
    }

    public static void Register(Type type, FortModule module, string name, float chance, PickupLoader loader)
    {
        const int offset = 21;
        var stride = (Pickups)offset + PickupDatas.Count;
        var pickupObject = new PickupData() 
        {
            Name = name,
            ID = stride,
            Chance = chance,
            PickupLoader = loader
        };
        StringToTypes[name] = stride;
        PickupDatas[stride] = pickupObject;
        Types.Add(type, stride);

        if (type.IsCompatible(typeof(ArrowTypePickup)))
        {
            int index = ArrowPickups.Count;
            AddToArrowPickupList(stride);
            ArrowToPickupMapping.Add(ArrowsRegistry.Types[ArrowPickupsType[index]], ArrowPickups[index]);
        }
    }

    public static void AddToArrowPickupList(Pickups pickups)
    {
        ArrowPickups.Add(pickups);
    }

    public static void AddToArrowPickupTypeList(Type type)
    {
        ArrowPickupsType.Add(type);
    }
}
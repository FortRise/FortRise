using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public static class PickupsRegistry 
{
    public static Dictionary<Pickups, PickupData> PickupDatas = new Dictionary<Pickups, PickupData>();
    public static Dictionary<string, Pickups> StringToTypes = new();
    public static Dictionary<Type, Pickups> Types = new();
    public static List<Pickups> ArrowPickups = new List<Pickups>();

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

            string id = $"{module.Meta.Name}/{pickup.Name}";
            Register(id, new PickupConfiguration() 
            {
                Name = pickup.Name,
                Chance = pickup.Chance,
                PickupType = type
            });
        }

        foreach (var pickup in type.GetCustomAttributes<CustomArrowPickupAttribute>()) 
        {
            if (pickup is null)
                continue;

            string id = $"{module.Meta.Name}/{pickup.Name}";
            Register(id, new PickupConfiguration() 
            {
                Name = pickup.Name,
                Chance = pickup.Chance,
                PickupType = type,
                ArrowType = pickup.ArrowType,
            });
        }
    }

    public static void Register(string name, in PickupConfiguration configuration)
    {
        PickupLoader loader = null;

        if (configuration.ArrowType != null)
        {
            ConstructorInfo arrowCtor = configuration.PickupType.GetConstructor([typeof(Vector2), typeof(Vector2), typeof(ArrowTypes)]);

            if (arrowCtor != null)
            {
                Type arrowType = configuration.ArrowType;
                string n = configuration.Name;
                Option<Color> colA = configuration.Color;
                Option<Color> colB = configuration.ColorB;
                loader = (pos, targetPos, idx) => 
                {
                    var pickup = (patch_ArrowTypePickup)arrowCtor.Invoke([pos, targetPos, ArrowsRegistry.Types[arrowType]]);
                    if (string.IsNullOrEmpty(pickup.Name))
                    {
                        pickup.Name = n;
                    }
                    if (colA.TryGetValue(out Color col))
                    {
                        pickup.Color = col;
                    }

                    if (colB.TryGetValue(out Color cb))
                    {
                        pickup.ColorB = cb;
                    }
                    return pickup;
                };
            }
        }
        else
        {
            ConstructorInfo ctor = configuration.PickupType.GetConstructor([typeof(Vector2), typeof(Vector2)]);
            if (ctor != null)
            {
                loader = (pos, targetPos, idx) => 
                {
                    var custom = (Pickup)ctor.Invoke([pos, targetPos]);
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
        }

        const int offset = 21;
        var stride = (Pickups)offset + PickupDatas.Count;
        var pickupObject = new PickupData() 
        {
            Name = name,
            ID = stride,
            Chance = configuration.Chance,
            ArrowType = configuration.ArrowType,
            PickupLoader = loader
        };
        StringToTypes[name] = stride;
        PickupDatas[stride] = pickupObject;
        Types.Add(configuration.PickupType, stride);
    }
}
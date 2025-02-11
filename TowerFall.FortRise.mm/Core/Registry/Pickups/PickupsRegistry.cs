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
        const int offset = 21;
        foreach (var pickup in type.GetCustomAttributes<CustomPickupAttribute>()) 
        {
            if (pickup is null)
                continue;
            var pickupName = pickup.Name ?? $"{type.Namespace}.{type.Name}";
            var stride = (Pickups)offset + PickupDatas.Count;
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

            var pickupObject = new PickupData() 
            {
                Name = pickupName,
                ID = stride,
                Chance = pickup.Chance,
                PickupLoader = loader
            };

            StringToTypes[pickupName] = stride;
            PickupDatas[stride] = pickupObject;
            Types.Add(type, stride);
        }
    }
}
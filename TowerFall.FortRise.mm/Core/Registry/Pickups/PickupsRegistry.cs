using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

public static class PickupsRegistry 
{
    private static Dictionary<string, IPickupEntry> pickupEntries = [];
    private static Dictionary<Pickups, IPickupEntry> typeToEntries = [];
    public static Dictionary<string, Pickups> StringToTypes = new();

    public static void AddPickup(IPickupEntry entry)
    {
        pickupEntries[entry.Name] = entry;
        typeToEntries[entry.Pickups] = entry;
    }

#nullable enable
    public static IPickupEntry? GetPickup(string id)
    {
        pickupEntries.TryGetValue(id, out var entry);
        return entry;
    }

    public static IPickupEntry? GetPickup(Pickups pickupType)
    {
        typeToEntries.TryGetValue(pickupType, out var entry);
        return entry;
    }

    public static IReadOnlyDictionary<string, IPickupEntry> GetAllPickups()
    {
        return pickupEntries;
    }

    private static Dictionary<Type, ConstructorInfo> constructors = new Dictionary<Type, ConstructorInfo>();

    internal static patch_Pickup? CreatePickup(Pickups pickup, Vector2 pos, Vector2 targetPos, int playerIndex)
    {
        var entry = GetPickup(pickup);

        if (entry is null)
        {
            RiseCore.logger.LogError("This type does not exists or it is only for vanilla.");
            return null;
        }

        if (entry.Configuration.ArrowType.TryGetValue(out var type))
        {
            ref var arrowCtor = ref CollectionsMarshal.GetValueRefOrAddDefault(constructors, entry.Configuration.PickupType, out var exists);

            if (!exists)
            {
                arrowCtor = entry.Configuration.PickupType.GetConstructor([typeof(Vector2), typeof(Vector2), typeof(ArrowTypes)]);
            }

            if (arrowCtor is null)
            {
                RiseCore.logger.LogError("Invalid pickup type: '{name}'", entry.Configuration.PickupType.Name);
                return null;
            }

            var arrowType = (patch_ArrowTypePickup)arrowCtor.Invoke([pos, targetPos, type]);

            arrowType.Name = entry.Configuration.Name;

            if (entry.Configuration.Color.TryGetValue(out var colA))
            {
                arrowType.Color = colA;
            }

            if (entry.Configuration.ColorB.TryGetValue(out var colB))
            {
                arrowType.ColorB = colB;
            }

            return (patch_Pickup)(object)arrowType;
        }
        else
        {
            ref var ctor = ref CollectionsMarshal.GetValueRefOrAddDefault(constructors, entry.Configuration.PickupType, out var didExistsAgain);

            if (!didExistsAgain)
            {
                ctor = entry.Configuration.PickupType.GetConstructor([typeof(Vector2), typeof(Vector2), typeof(int)]);
            }

            if (ctor is not null)
            {
                // FIXME: remove this try-catch non sense 
                try
                {
                    return (patch_Pickup)ctor.Invoke([pos, targetPos, playerIndex]);
                }
                catch (TargetParameterCountException)
                {
                    return (patch_Pickup)ctor.Invoke([pos, targetPos]);
                }
            }
            else
            {
                ctor = entry.Configuration.PickupType.GetConstructor([typeof(Vector2), typeof(Vector2)]);
            }

            if (ctor is not null)
            {
                return (patch_Pickup)ctor.Invoke([pos, targetPos]);
            }

            RiseCore.logger.LogError("Invalid pickup type: '{name}'", entry.Configuration.PickupType.Name);
            return null;
        }
    }
#nullable disable

    public static string TypesToString(Pickups pickups)
    {
        if ((int)pickups < 21)
        {
            return pickups.ToString();
        }
        var pickup = GetPickup(pickups);

        if (pickup is not null)
        {
            return pickup.Name;
        }

        Logger.Error("[PickupRegistry] Unknown Pickups type passed");
        return null;
    }

    public static void Register(string name, Pickups pickups, in PickupConfiguration configuration)
    {
        var stride = pickups;

        StringToTypes[name] = stride;
    }
}
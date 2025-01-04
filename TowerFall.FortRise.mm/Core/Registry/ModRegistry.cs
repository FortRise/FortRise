using TowerFall;

namespace FortRise;

public static class ModRegisters
{
    public static TowerFall.Modes GameModeType<T>() 
    where T : CustomGameMode
    {
        if (GameModeRegistry.GameModeTypes.TryGetValue(typeof(T), out var idx))
            return GameModeRegistry.VersusGameModes[idx].GameModeInternal;
        return TowerFall.Modes.LastManStanding;
    }

    public static TowerFall.Modes GameModeType(string name) 
    {
        if (GameModeRegistry.GameModesMap.TryGetValue(name, out var idx))
            return GameModeRegistry.VersusGameModes[GameModeRegistry.GameModeTypes[idx]].GameModeInternal;
        return TowerFall.Modes.LastManStanding;
    }

    public static ArrowData ArrowData(ArrowTypes types) 
    {
        if (ArrowsRegistry.ArrowDatas.TryGetValue(types, out var val)) 
        {
            return val;
        }
        return null;
    }

    public static ArrowData ArrowData(string name) 
    {
        if (ArrowsRegistry.StringToTypes.TryGetValue(name, out var val)) 
        {
            return ArrowData(val);
        }
        Logger.Warning($"{name} cannot be found");
        return null;
    }

    public static ArrowData ArrowData<T>() 
    where T : Arrow
    {
        return ArrowData(ArrowType<T>());
    }

    public static ArrowTypes ArrowType(string name) 
    {
        if (ArrowsRegistry.StringToTypes.TryGetValue(name, out var val))
            return val;
        Logger.Warning($"{name} cannot be found, falling back to Default Arrows");
        return ArrowTypes.Normal;
    }

    public static ArrowTypes ArrowType<T>() 
    where T : Arrow
    {
        if (ArrowsRegistry.Types.TryGetValue(typeof(T), out var val))
            return val;
        return ArrowTypes.Normal;
    }

    public static PickupData PickupData(Pickups types) 
    {
        if (PickupsRegistry.PickupDatas.TryGetValue(types, out var val)) 
        {
            return val;
        }
        return null;
    }

    public static PickupData PickupData(string name) 
    {
        if (PickupsRegistry.StringToTypes.TryGetValue(name, out var val)) 
        {
            return PickupData(val);
        }
        Logger.Warning($"{name} cannot be found");
        return null;
    }

    public static PickupData PickupData<T>() 
    where T : Pickup
    {
        return PickupData(PickupType<T>());
    }

    public static Pickups PickupType(string name) 
    {
        if (PickupsRegistry.StringToTypes.TryGetValue(name, out var val))
            return val;
        
        Logger.Warning($"{name} cannot be found, falling back to Default Arrows");
        return Pickups.Arrows;
    }

    public static Pickups PickupType<T>() 
    where T : Pickup
    {
        if (PickupsRegistry.Types.TryGetValue(typeof(T), out var val))
            return val;
        return Pickups.Arrows;
    }
}
using System;
using TowerFall;

namespace FortRise;

public static class ModRegisters
{
    [Obsolete("Use VersusGameModeType")]
    public static TowerFall.Modes GameModeType<T>()
    where T : CustomGameMode
    {
        Logger.Error("This will not work anymore, use the new IVersusGamemode");
        return Modes.LastManStanding;
    }

    [Obsolete("Use VersusGameModeType")]
    public static TowerFall.Modes GameModeType(string name) 
    {
        Logger.Error("This will not work anymore, use the new IVersusGamemode");
        return Modes.LastManStanding;
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

    public static MainMenu.MenuState MenuState<T>()
    where T : CustomMenuState
    {
        if (CustomMenuStateRegistry.TypeToMenuStates.TryGetValue(typeof(T), out var val))
        {
            return val;
        }
        return MainMenu.MenuState.PressStart;
    }

    public static MainMenu.MenuState MenuState(string name)
    {
        if (CustomMenuStateRegistry.StringToMenuStates.TryGetValue(name, out var val))
        {
            return val;
        }
        return MainMenu.MenuState.PressStart;
    }
}
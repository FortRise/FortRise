using System;
using TowerFall;

namespace FortRise;

public static class ModRegisters
{
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
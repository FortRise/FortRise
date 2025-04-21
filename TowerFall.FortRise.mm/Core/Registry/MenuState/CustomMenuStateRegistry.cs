using System;
using System.Collections.Generic;
using System.Reflection;
using TowerFall;

namespace FortRise;

public static class CustomMenuStateRegistry 
{
    public static Dictionary<string, MainMenu.MenuState> StringToMenuStates = new Dictionary<string, MainMenu.MenuState>();
    public static Dictionary<Type, MainMenu.MenuState> TypeToMenuStates = new Dictionary<Type, MainMenu.MenuState>();
    public static Dictionary<MainMenu.MenuState, CustomMenuStateLoader> MenuLoaders = new Dictionary<MainMenu.MenuState, CustomMenuStateLoader>();
    public static HashSet<MainMenu.MenuState> MenuStates = new HashSet<MainMenu.MenuState>();

    internal static void LoadAllBuiltinMenuState()
    {
        Register("FortRise/Mods", EnumPool.Obtain<MainMenu.MenuState>(), new MenuStateConfiguration() { MenuStateType = typeof(UIModMenu )});
        Register("FortRise/UIModOptions", EnumPool.Obtain<MainMenu.MenuState>(), new MenuStateConfiguration() { MenuStateType = typeof(UIModOptions)});
        Register("FortRise/UIModToggler", EnumPool.Obtain<MainMenu.MenuState>(), new MenuStateConfiguration() { MenuStateType = typeof(UIModToggler)});
    }


    public static void Register(string id, MainMenu.MenuState state, MenuStateConfiguration configuration)
    {
        var type = configuration.MenuStateType;
        ConstructorInfo ctor = type.GetConstructor([typeof(MainMenu)]);
        CustomMenuStateLoader loader = null;

        if (ctor != null) 
        {
            loader = (menu) => 
            {
                var custom = (CustomMenuState)ctor.Invoke([menu]);
                return custom;
            };
        }

        string name = id;
        StringToMenuStates[id] = state;
        TypeToMenuStates[type] = state;
        MenuLoaders[state] = loader;
        MenuStates.Add(state);
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using TowerFall;

namespace FortRise;

public static class CustomMenuStateRegistry 
{
    private static Dictionary<string, IMenuStateEntry> menuStateEntries = [];
    private static readonly Dictionary<Type, CustomMenuState> typeCache = [];
    public static Dictionary<string, MainMenu.MenuState> StringToMenuStates = new Dictionary<string, MainMenu.MenuState>();
    public static Dictionary<Type, MainMenu.MenuState> TypeToMenuStates = new Dictionary<Type, MainMenu.MenuState>();
    public static Dictionary<MainMenu.MenuState, CustomMenuStateLoader> MenuLoaders = new Dictionary<MainMenu.MenuState, CustomMenuStateLoader>();
    public static HashSet<MainMenu.MenuState> MenuStates = new HashSet<MainMenu.MenuState>();

    internal static void LoadAllBuiltinMenuState()
    {
        Register("FortRise/UIMods", EnumPool.Obtain<MainMenu.MenuState>(), new MenuStateConfiguration() { MenuStateType = typeof(UIModMenu )});
        Register("FortRise/UIArcherBlacklist", EnumPool.Obtain<MainMenu.MenuState>(), new MenuStateConfiguration() { MenuStateType = typeof(UIArcherBlacklist)});
        Register("FortRise/UIMusicList", EnumPool.Obtain<MainMenu.MenuState>(), new MenuStateConfiguration() { MenuStateType = typeof(UIMusicList)});
        Register("FortRise/UIModOptions", EnumPool.Obtain<MainMenu.MenuState>(), new MenuStateConfiguration() { MenuStateType = typeof(UIModOptions)});
        Register("FortRise/UIModToggler", EnumPool.Obtain<MainMenu.MenuState>(), new MenuStateConfiguration() { MenuStateType = typeof(UIModToggler)});
    }

    public static void AddMenuState(IMenuStateEntry entry)
    {
        menuStateEntries[entry.Name] = entry;
    }

#nullable enable
    public static IMenuStateEntry? GetMenuState(string id)
    {
        menuStateEntries.TryGetValue(id, out var entry);
        return entry;
    }
#nullable disable


    public static void Register(string id, MainMenu.MenuState state, MenuStateConfiguration configuration)
    {
        var type = configuration.MenuStateType;
        ConstructorInfo ctor = type.GetConstructor([typeof(MainMenu)]);
        CustomMenuStateLoader loader = null;

        if (ctor != null)
        {
            loader = (menu) =>
            {
                ref var custom = ref CollectionsMarshal.GetValueRefOrAddDefault(typeCache, type, out bool exists);
                if (!exists)
                {
                    custom = (CustomMenuState)ctor.Invoke([menu]);
                }

                return custom;
            };
        }

        string name = id;
        StringToMenuStates[id] = state;
        TypeToMenuStates[type] = state;
        MenuLoaders[state] = loader;
        MenuStates.Add(state);
    }

    public static void DestroyTypeCache(Type type) 
    {
        typeCache.Remove(type);
    }
}

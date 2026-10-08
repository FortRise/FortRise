using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using TowerFall;

namespace FortRise;

public static class CustomMenuStateRegistry 
{
    public static Dictionary<string, IMenuStateEntry> MenuStateEntries = [];
    private static readonly Dictionary<Type, CustomMenuState> typeCache = [];
    public static Dictionary<MainMenu.MenuState, CustomMenuStateLoader> MenuLoaders = new Dictionary<MainMenu.MenuState, CustomMenuStateLoader>();

    public static void AddMenuState(IMenuStateEntry entry)
    {
        MenuStateEntries[entry.Name] = entry;
    }

#nullable enable
    public static IMenuStateEntry? GetMenuState(string id)
    {
        MenuStateEntries.TryGetValue(id, out var entry);
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
        MenuLoaders[state] = loader;
    }

    public static void Unregister(IMenuStateEntry entry)
    {
        MenuStateEntries.Remove(entry.Name);
        MenuLoaders.Remove(entry.MenuState);
        DestroyTypeCache(entry.Configuration.MenuStateType);
    }

    public static void DestroyTypeCache(Type type) 
    {
        typeCache.Remove(type);
    }
}

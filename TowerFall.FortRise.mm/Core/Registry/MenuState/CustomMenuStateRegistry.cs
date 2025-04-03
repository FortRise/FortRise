using System;
using System.Collections.Generic;
using System.Reflection;
using TowerFall;

namespace FortRise;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CustomMenuStateAttribute : Attribute
{
    public string Name { get; set; }

    public CustomMenuStateAttribute(string name)
    {
        Name = name;
    }
}

public static class CustomMenuStateRegistry 
{
    public static Dictionary<string, MainMenu.MenuState> StringToMenuStates = new Dictionary<string, MainMenu.MenuState>();
    public static Dictionary<Type, MainMenu.MenuState> TypeToMenuStates = new Dictionary<Type, MainMenu.MenuState>();
    public static Dictionary<MainMenu.MenuState, CustomMenuStateLoader> MenuLoaders = new Dictionary<MainMenu.MenuState, CustomMenuStateLoader>();
    public static HashSet<MainMenu.MenuState> MenuStates = new HashSet<MainMenu.MenuState>();
    private const ulong offset = 16;

    internal static void LoadAllBuiltinMenuState()
    {
        Register(typeof(UIModMenu), null);
        Register(typeof(UIModOptions), null);
    }


    public static void Register(Type type, FortModule module)
    {
        foreach (var menuState in type.GetCustomAttributes<CustomMenuStateAttribute>()) 
        {
            if (menuState is null)
            {
                continue;
            }

            string name = menuState.Name;

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

            var id = (MainMenu.MenuState)offset + MenuStates.Count;

            StringToMenuStates[name] = id;
            TypeToMenuStates[type] = id;
            MenuLoaders[id] = loader;
            MenuStates.Add(id);
        }
    }
}

public abstract class CustomMenuState 
{
    public MainMenu Main { get; set; }


    public CustomMenuState(MainMenu main)
    {
        Main = main;
    }

    public abstract void Create();
    public abstract void Destroy();
}
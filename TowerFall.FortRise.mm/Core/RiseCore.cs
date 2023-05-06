using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Monocle;
using TeuJson;
using TowerFall;

namespace FortRise;

public static partial class RiseCore 
{
    public static Dictionary<string, EnemyLoader> Loader = new();
    public static List<RiseModule> Modules = new();
    private static List<ModuleHandler> ModAssemblies = new List<ModuleHandler>();

    private static Type[] Types;

    public readonly static Type[] EmptyTypeArray = new Type[0];
    public readonly static object[] EmptyObjectArray = new object[0];

    internal static void Register(this RiseModule module) 
    {
        Modules.Add(module);
        foreach (var type in module.GetType().Assembly.GetTypes()) 
        {
            if (type is null)
                continue;

            foreach (EnemyAttribute attrib in type.GetCustomAttributes<EnemyAttribute>()) 
            {
                if (attrib is null)
                    continue;
                var name = attrib.Name;
                var arg = attrib.FuncArg;

                ConstructorInfo ctor;
                EnemyLoader loader = null;

                ctor = type.GetConstructor(
                    new Type[] { typeof(Vector2), typeof(Facing) }
                );
                if (ctor != null) 
                {
                    loader = (position, facing) => {
                        var invoked = (patch_Enemy)ctor.Invoke(new object[] {
                            position,
                            facing
                        });
                        invoked.Load();

                        return invoked;

                    };
                    goto Loaded;
                }
                Loaded:
                Loader.Add(name, loader);
            }
        }
    }

    internal static void ModuleStart() 
    {
        var directory = Directory.EnumerateDirectories("Mods").ToList();
        if (directory.Count <= 0) 
        {
            Types = Array.Empty<Type>();
            return;
        }

        int i = 0;
        Types = new Type[directory.Count];
        foreach (var dir in directory) 
        {
            var metaPath = Path.Combine(dir, "meta.json");
            if (!File.Exists(metaPath))
                continue;
            
            var json = JsonTextReader.FromFile(metaPath);
            var pathToAssembly = Path.GetFullPath(Path.Combine(dir, json["dll"]));
            if (!File.Exists(pathToAssembly))
                continue;
            var assembly = Assembly.LoadFrom(pathToAssembly);
            var module = new ModuleHandler(
                json["name"], new Version(json["version"]), 
                json["description"], json["author"], assembly);
            ModAssemblies.Add(module);
            GetModuleTypes(assembly, i++);
        }
    }

    private static void GetModuleTypes(Assembly asm, int index) 
    {
        foreach (var t in asm.GetTypes()) 
        {
            var customAttribute = t.GetCustomAttribute<RiseAttribute>();
            if (customAttribute != null) 
            {
                Types[index] = t;
                RiseModule obj = Activator.CreateInstance(t) as RiseModule;
                obj.Name = customAttribute.Name;
                obj.ID = customAttribute.GUID;
                obj.Register();
                var method = t.GetMethod("InternalLoad");
                method.Invoke(obj, null);
            }
        }
    }


    internal static void LogAllTypes() 
    {
        Commands commands = Engine.Instance.Commands;
        int i = 0;
        foreach (var t in Types) 
        {
            if (t is null)
                continue;
            commands.Log(t.Assembly.FullName);
            i++;
        }
        commands.Log($"{i} total of mods loaded");
    }

    internal static void LoadContent() 
    {
        foreach (var t in Types) 
        {
            if (t is null)
                continue;
            var obj = Activator.CreateInstance(t);
            var method = t.GetMethod("LoadContent");
            method.Invoke(obj, null);
        }
    }

    internal static void Initialize() 
    {
        foreach (var t in Types) 
        {
            if (t is null)
                continue;
            var obj = Activator.CreateInstance(t);
            var method = t.GetMethod("Initialize");
            method.Invoke(obj, null);
        }
    }

    internal static void ModuleEnd() 
    {
        foreach (var t in Types) 
        {
            if (t is null)
                continue;
            var obj = Activator.CreateInstance(t);
            var method = t.GetMethod("Unload");
            method.Invoke(obj, null);
        }
    }
}

// Work In Progress
public class ModuleHandler
{
    public string Name;
    public System.Version Version;
    public string Description;
    public string Author;
    public Assembly Module;

    public ModuleHandler(string name, System.Version version, string description, string author, Assembly assembly) 
    {
        Name = name;
        Version = version;
        Description = description;
        Author = author;
        Module = assembly;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TeuJson;
using TowerFall;

namespace FortRise;

public delegate Enemy EnemyLoader(Vector2 position, Facing facing);
public delegate LevelEntity LevelEntityLoader(XmlElement x);
public delegate RoundLogic RoundLogicLoader(patch_Session session, bool canHaveMiasma = false);


public static partial class RiseCore 
{
    public static Dictionary<string, EnemyLoader> EnemyLoader = new();
    public static Dictionary<string, LevelEntityLoader> LevelEntityLoader = new();
    public static Dictionary<string, RoundLogicLoader> RoundLogicLoader = new();
    public static Dictionary<string, RoundLogicIdentifier> RoundLogicIdentifiers = new();
    public static List<FortModule> Modules = new();

    private static Type[] Types;

    public readonly static Type[] EmptyTypeArray = new Type[0];
    public readonly static object[] EmptyObjectArray = new object[0];

    internal static void Register(this FortModule module) 
    {
        foreach (var type in module.GetType().Assembly.GetTypes()) 
        {
            if (type is null)
                continue;
            
            foreach (var attrib in type.GetCustomAttributes<CustomRoundLogicAttribute>()) 
            {
                if (attrib is null)
                    continue;

                string name = attrib.Name;
                RoundLogicLoader loader = null;
                ConstructorInfo ctor;
                MethodInfo info;
                string overriden = attrib.OverrideFunction ?? "Create";
                info = type.GetMethod(overriden);
                if (info == null || !info.IsStatic)
                {
                    Logger.Log($"No `static RoundLogicIdentifier Create()` method found on this RoundLogic {name}, ignored.");
                    continue;
                }
                var identifier = (RoundLogicIdentifier)info.Invoke(null, Array.Empty<object>());
                RoundLogicIdentifiers.Add(name, identifier);
                ctor = type.GetConstructor(new Type[] { typeof(Session), typeof(bool) });
                if (ctor != null) 
                {
                    loader = (x, y) => {
                        var roundLogic = (CustomVersusRoundLogic)ctor.Invoke(new object[] { x, y });
                        return roundLogic;
                    };
                    goto Loaded;
                }
                ctor = type.GetConstructor(new Type[] { typeof(Session) });
                if (ctor != null) 
                {
                    loader = (x, y) => {
                        var roundLogic = (CustomVersusRoundLogic)ctor.Invoke(new object[] { x });
                        return roundLogic;
                    };
                    goto Loaded;
                }
                Loaded:
                if (identifier.RoundType == RoundLogicType.FFA)
                    CustomVersusRoundLogic.LookUpModes.Add(name, Modes.LastManStanding);
                else if (identifier.RoundType == RoundLogicType.TeamDeatchmatch)
                    CustomVersusRoundLogic.LookUpModes.Add(name, Modes.TeamDeathmatch);
                else
                    CustomVersusRoundLogic.LookUpModes.Add(name, Modes.HeadHunters);

                CustomVersusRoundLogic.VersusModes.Add(name);
                RoundLogicLoader.Add(name, loader);
                
            }

            foreach (CustomEnemyAttribute attrib in type.GetCustomAttributes<CustomEnemyAttribute>()) 
            {
                if (attrib is null)
                    continue;
                foreach (var name in attrib.Names) 
                {
                    string id;
                    string methodName = null;
                    string[] split = name.Split('=');
                    if (split.Length == 1) 
                    {
                        id = split[0];
                    }
                    else if (split.Length == 2) 
                    {
                        id = split[0];
                        methodName = split[1];
                    }
                    else 
                    {
                        Logger.Log($"Invalid syntax of custom entity ID: {name}, {type.FullName}", Logger.LogLevel.Warning);
                        continue;
                    }
                    id = id.Trim();
                    methodName = methodName?.Trim();
                    ConstructorInfo ctor;
                    MethodInfo info;
                    EnemyLoader loader = null;

                    info = type.GetMethod(methodName, new Type[] { typeof(Vector2), typeof(Facing) });
                    if (methodName != null && info.IsStatic && info.ReturnType.IsCompatible(typeof(Enemy))) 
                    {
                        loader = (position, facing) => {
                            var invoked = (patch_Enemy)info.Invoke(null, new object[] {
                                position, facing
                            });
                            invoked.Load();
                            return invoked;
                        };
                        goto Loaded;
                    }

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
                    EnemyLoader.Add(id, loader);
                }
            }

            foreach (var clea in type.GetCustomAttributes<CustomLevelEntityAttribute>()) 
            {
                if (clea is null)
                    return;
                var name = clea.Name;

                ConstructorInfo ctor;
                LevelEntityLoader loader = null;
                ctor = type.GetConstructor(new Type[] { typeof(XmlElement) });
                if (ctor != null) 
                {
                    loader = x => 
                    {
                        var invoked = (LevelEntity)ctor.Invoke(new object[] { x });
                        return invoked;
                    };
                    goto Loaded;
                }
                Loaded:
                LevelEntityLoader.Add(name, loader);
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
            ResolveEventHandler resolver = (object o, ResolveEventArgs args) => {
                string asmPath = Path.Combine(dir, new AssemblyName(args.Name).Name + ".dll");
                if (!File.Exists(asmPath))
                    return null;
                return Assembly.LoadFrom(asmPath);
            };
            AppDomain.CurrentDomain.AssemblyResolve += resolver;
            var assembly = Assembly.LoadFrom(pathToAssembly);
            GetModuleTypes(json, assembly, i++);
            AppDomain.CurrentDomain.AssemblyResolve -= resolver;
        }
    }

    private static void GetModuleTypes(JsonValue json, Assembly asm, int index) 
    {
        foreach (var t in asm.GetTypes()) 
        {
            var customAttribute = t.GetCustomAttribute<FortAttribute>();
            if (customAttribute != null) 
            {
                Types[index] = t;
                FortModule obj = Activator.CreateInstance(t) as FortModule;
                obj.Name = customAttribute.Name;
                obj.MetaName = json["name"];
                obj.MetaVersion = new Version(json["version"]);
                obj.MetaDescription = json["description"];
                obj.MetaAuthor = json["author"];
                obj.ID = customAttribute.GUID;
                Modules.Add(obj);
                obj.InternalLoad();
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
        foreach (var t in Modules) 
        {
            t.LoadContent();
            t.Register();
        }
    }

    internal static void Initialize() 
    {
        foreach (var t in Modules) 
        {
            t.Initialize();
        }
    }

    internal static void ModuleEnd() 
    {
        foreach (var t in Modules) 
        {
            t.Unload();
        }
    }
}
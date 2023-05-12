using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TeuJson;
using TowerFall;

namespace FortRise;

public delegate Enemy EnemyLoader(Vector2 position, Facing facing);
public delegate patch_Arrow ArrowLoader();
public delegate Pickup PickupLoader(Vector2 position, Vector2 targetPosition, int playerIndex);
public delegate LevelEntity LevelEntityLoader(XmlElement x);
public delegate RoundLogic RoundLogicLoader(patch_Session session, bool canHaveMiasma = false);


public static partial class RiseCore 
{
    internal static long PickupLoaderCount = 21;
    public static Dictionary<string, EnemyLoader> EnemyLoader = new();
    public static Dictionary<string, LevelEntityLoader> LevelEntityLoader = new();
    public static Dictionary<string, RoundLogicLoader> RoundLogicLoader = new();
    public static Dictionary<string, RoundLogicInfo> RoundLogicIdentifiers = new();
    public static Dictionary<Pickups, PickupLoader> PickupLoader = new();

    // This is the way we could use to manipulate arrows from enums
    public static Dictionary<string, ArrowTypes> ArrowsID = new();
    public static Dictionary<ArrowTypes, ArrowLoader> Arrows = new();
    public static Dictionary<ArrowTypes, ArrowInfo> PickupGraphicArrows = new();

    public static ReadOnlyCollection<FortModule> Modules => InternalModules.AsReadOnly();
    internal static List<FortModule> InternalModules = new();

    public static List<string> DetourLogs = new List<string>();

    private static Type[] Types;

    public static bool DebugMode;

    internal static void ModuleStart() 
    {
        if (!Directory.Exists("Mods"))
            Directory.CreateDirectory("Mods");
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
                var name = json.Contains("name") ? json["name"].AsString : obj.Name;
                var version = json.Contains("version") ? json["version"].AsString : "1.0.0";
                var description = json.GetJsonValueOrNull("description") ?? "";
                var author = json.GetJsonValueOrNull("author") ?? "";
                
                obj.Meta = new ModuleMetadata 
                {
                    Name = name,
                    Version = new Version(version),
                    Description = description,
                    Author = author
                };
                obj.ID = customAttribute.GUID;
                obj.Register();
            }
        }
    }


    internal static void LogAllTypes() 
    {
        int i = 0;
        foreach (var t in Types) 
        {
            if (t is null)
                continue;
            Logger.Info(t.Assembly.FullName);
            i++;
        }
        Logger.Info(i + " total of mods loaded");
    }

    internal static void Initialize() 
    {
        foreach (var t in InternalModules) 
        {
            t.Initialize();
        }
    }

    internal static void ModuleEnd() 
    {
        foreach (var t in InternalModules) 
        {
            t.Unload();
        }
    }

    internal static void Register(this FortModule module) 
    {
        module.InternalLoad();
        module.LoadContent();
        module.Enabled = true;
        patch_MatchVariants.DeclareVariants += module.OnVariantsRegister;
        InternalModules.Add(module);
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
                    Logger.Error($"No `static RoundLogicIdentifier Create()` method found on this RoundLogic {name}, ignored.");
                    continue;
                }
                var identifier = (RoundLogicInfo)info.Invoke(null, Array.Empty<object>());
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
                    CustomVersusRoundLogic.LookUpModes.Add(name, patch_Modes.LastManStanding);
                else if (identifier.RoundType == RoundLogicType.TeamDeatchmatch)
                    CustomVersusRoundLogic.LookUpModes.Add(name, patch_Modes.TeamDeathmatch);
                else
                    CustomVersusRoundLogic.LookUpModes.Add(name, patch_Modes.HeadHunters);

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
                        Logger.Error($"Invalid syntax of custom entity ID: {name}, {type.FullName}");
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

            foreach (var arrow in type.GetCustomAttributes<CustomArrowsAttribute>()) 
            {
                const int offset = 11;
                if (arrow is null)
                    return;
                var name = arrow.Name;
                var graphicFn = arrow.GraphicPickupInitializer ?? "CreateGraphicPickup";
                var stride = (ArrowTypes)offset + ArrowsID.Count;
                MethodInfo graphic = type.GetMethod(graphicFn);

                ConstructorInfo ctor = type.GetConstructor(Array.Empty<Type>());
                ArrowLoader loader = null;
                if (ctor != null) 
                {
                    loader = () => 
                    {
                        var invoked = (patch_Arrow)ctor.Invoke(Array.Empty<object>());
                        invoked.ArrowType = stride;
                        return invoked;
                    };
                }
                if (graphic == null || !graphic.IsStatic)
                {
                    Logger.Log($"No `static ArrowInfo CreateGraphicPickup()` method found on this Arrow {name}, falling back to normal arrow graphics.");
                }
                else 
                {
                    var identifier = (ArrowInfo)graphic.Invoke(null, Array.Empty<object>());
                    PickupGraphicArrows.Add(stride, identifier);
                }

                ArrowsID.Add(name, stride);
                Arrows.Add(stride, loader);
                PickupLoader.Add((Pickups)PickupLoaderCount, (pos, targetPos, _) => new ArrowTypePickup(
                    pos, targetPos, stride));
                PickupLoaderCount++;
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

    internal static void Unregister(this FortModule module) 
    {
        module.Unload();
        InternalModules.Remove(module);
    }


    internal static IConsole ConsoleAttachment() 
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
        {
            return new WindowConsole();
        }
        return null;
    }

    public static void LogDetours(Logger.LogLevel level = Logger.LogLevel.Debug) 
    {
        List<string> detours = DetourLogs;
        if (detours.Count == 0)
            return;

        DetourLogs = new List<string>();

        foreach (string line in detours) 
        {
            Logger.Log(line, level);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml;
using FortRise.Adventure;
using Microsoft.Xna.Framework;
using MonoMod;
using MonoMod.Utils;
using TeuJson;
using TowerFall;

namespace FortRise;

public delegate Enemy EnemyLoader(Vector2 position, Facing facing);
public delegate DarkWorldBoss DarkWorldBossLoader(int difficulty);
public delegate patch_Arrow ArrowLoader();
public delegate ArrowInfo ArrowInfoLoader();
public delegate Pickup PickupLoader(Vector2 position, Vector2 targetPosition, int playerIndex);
public delegate LevelEntity LevelEntityLoader(XmlElement x, Vector2 position);
public delegate RoundLogic RoundLogicLoader(patch_Session session, bool canHaveMiasma = false);


public static partial class RiseCore 
{
    internal static long PickupLoaderCount = 21;
    public static Dictionary<string, EnemyLoader> EnemyLoader = new();
    public static Dictionary<string, DarkWorldBossLoader> DarkWorldBossLoader = new();
    public static Dictionary<string, LevelEntityLoader> LevelEntityLoader = new();
    public static Dictionary<string, RoundLogicLoader> RoundLogicLoader = new();
    public static Dictionary<string, RoundLogicInfo> RoundLogicIdentifiers = new();
    public static Dictionary<Pickups, PickupLoader> PickupLoader = new();

    // This is the way we could use to manipulate arrows from enums
    public static Dictionary<string, ArrowTypes> ArrowsID = new();
    public static Dictionary<string, Pickups> PickupID = new();
    public static Dictionary<ArrowTypes, ArrowLoader> Arrows = new();
    public static Dictionary<ArrowTypes, ArrowInfoLoader> PickupGraphicArrows = new();

    public static ReadOnlyCollection<FortModule> Modules => InternalFortModules.AsReadOnly();
    public static ReadOnlyCollection<ModuleMetadata> Mods => InternalMods.AsReadOnly();
    internal static List<FortModule> InternalFortModules = new();
    internal static HashSet<string> ModuleGuids = new();
    internal static List<ModuleMetadata> InternalMods = new();

    public static List<string> DetourLogs = new List<string>();
    public static Version FortRiseVersion;
    public static bool IsWindows { get; internal set; }


    public static bool DebugMode;

    internal static void ModuleStart() 
    {
        RiseCore.Flags();
        GameChecksum = GetChecksum(typeof(TFGame).Assembly.Location).ToHexadecimalString();
        GameRootPath = Path.GetDirectoryName(typeof(TFGame).Assembly.Location);
        if (!Directory.Exists("Mods"))
            Directory.CreateDirectory("Mods");

        var directory = Directory.EnumerateDirectories("Mods").ToList();
        new AdventureModule().Register();

        new NoModule(new ModuleMetadata() {
            Name = "FortRise",
            Version = FortRiseVersion
        }).Register();

        AppDomain.CurrentDomain.AssemblyResolve += (asmSender, asmArgs) => {
            AssemblyName asmName = new AssemblyName(asmArgs.Name);
            foreach (Assembly asm in Relinker.RelinkedAssemblies) {
                if (asm.GetName().Name == asmName.Name)
                    return asm;
            }

            return null;
        };

        // List<string> resolverLoadedPath = new List<string>();

        AppDomain.CurrentDomain.AssemblyResolve += (asmSender, asmArgs) => 
        {
            var name = asmArgs?.Name == null ? null : new AssemblyName(asmArgs.Name);
            if (string.IsNullOrEmpty(name?.Name))
                return null;

            foreach (var mod in InternalFortModules) 
            {
                var meta = mod.Meta;

                if (meta == null)
                    continue;

                if (string.IsNullOrEmpty(meta.PathDirectory))
                    continue;
                
                var path = name.Name + ".dll";
                if (!string.IsNullOrEmpty(meta.DLL)) 
                {
                    var pathDirectory = Path.GetDirectoryName(meta.DLL);
                    path = Path.Combine(pathDirectory, path).Replace('\\', '/');
                    if (!string.IsNullOrEmpty(pathDirectory))
                        path = path.Substring(pathDirectory.Length + 1);
                }

                var depPath = Path.Combine(meta.PathDirectory, path);
                if (!File.Exists(depPath))
                    continue;
                using var fs = File.OpenRead(depPath);
                if (fs != null)
                    return Relinker.GetRelinkedAssembly(meta, Path.GetFullPath(Path.Combine(meta.PathDirectory, path)), fs);
                
            }
            return null;
        };

        if (directory.Count <= 0) 
            return;

        int i = 0;

        foreach (var dir in directory) 
        {
            var metaPath = Path.Combine(dir, "meta.json");
            if (!File.Exists(metaPath))
                continue;

            var json = JsonTextReader.FromFile(metaPath);
            var dll = json.GetJsonValueOrNull("dll");
            var name = json.GetJsonValueOrNull("name");
            if (name == null)
            {
                Logger.Error($"{dir} does not have a name metadata.");
                continue;
            }
            var version = json.Contains("version") ? json["version"].AsString : "1.0.0";
            var requiredVersion = new Version(json.Contains("required") ? json["required"].AsString : "2.3.1");
            var description = json.GetJsonValueOrNull("description") ?? "";
            var author = json.GetJsonValueOrNull("author") ?? "";
            var jsonDependencies = json.GetJsonValueOrNull("dependencies");
            var nativePath = json.GetJsonValueOrNull("nativePath") ?? "";
            var nativePathX86 = json.GetJsonValueOrNull("nativePathX86") ?? "";
            string[] dependencies = null;
            if (jsonDependencies != null) 
            {
                dependencies = jsonDependencies.ConvertToArrayString();
            }
            if (FortRiseVersion < requiredVersion) 
            {
                Logger.Error($"Mod Name: {name} has a higher version of FortRise required {requiredVersion}. Your FortRise version: {FortRiseVersion}");
                continue;
            }
            var moduleMetadata = new ModuleMetadata() 
            {
                Name = name,
                Version = new Version(version),
                Description = description,
                Author = author,
                FortRiseVersion = requiredVersion,
                DLL = dll != null ? Path.GetFullPath(Path.Combine(dir, dll)) : string.Empty,
                PathDirectory = dir,
                Dependencies = dependencies,
                NativePath = nativePath,
                NativePathX86 = nativePathX86
            };

            InternalMods.Add(moduleMetadata);

            // Assembly Mod Loading
            if (dll == null) 
                continue;
            
            var pathToAssembly = Path.GetFullPath(Path.Combine(dir, dll));
            if (!File.Exists(pathToAssembly))
                continue;

            using var fs = File.OpenRead(pathToAssembly);

            var asm = Relinker.GetRelinkedAssembly(moduleMetadata, pathToAssembly, fs);
            RegisterAssembly(moduleMetadata, asm, i++);
        }
    }

    private static void RegisterAssembly(ModuleMetadata metadata, Assembly asm, int index) 
    {
        foreach (var t in asm.GetTypes()) 
        {
            var customAttribute = t.GetCustomAttribute<FortAttribute>();
            if (customAttribute != null) 
            {
                FortModule obj = Activator.CreateInstance(t) as FortModule;
                if (metadata.Name == string.Empty) 
                {
                    metadata.Name = customAttribute.Name;
                }
                obj.Name = customAttribute.Name;
                obj.ID = customAttribute.GUID;
                obj.Meta = metadata;

                ModuleGuids.Add(obj.ID);
                obj.Register();
                Logger.Info($"{obj.ID}: {obj.Name} Registered.");
            }
        }
    }

    [PatchFlags]
    internal static void Flags() 
    {

    }

    public static readonly HashAlgorithm ChecksumHasher = MD5.Create();

    public static byte[] GetChecksum(string path) 
    {
        using var fs = File.OpenRead(path);
        return ChecksumHasher.ComputeHash(fs);
    }

    public static byte[] GetChecksum(ModuleMetadata meta) 
    {
        return GetChecksum(meta.DLL);
    }

    // https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Everest/Everest.cs
    public static byte[] GetChecksum(ref Stream stream) 
    {
        if (!stream.CanSeek) 
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            stream.Dispose();
            stream = ms;
            stream.Seek(0, SeekOrigin.Begin);
        }

        long pos = stream.Position;
        stream.Seek(0, SeekOrigin.Begin);
        byte[] hash = ChecksumHasher.ComputeHash(stream);
        stream.Seek(pos, SeekOrigin.Begin);
        return hash;
    }


    internal static void LogAllTypes() 
    {
        Logger.Info(InternalFortModules.Count + " total of mods loaded");
    }

    internal static void Initialize() 
    {
        foreach (var t in InternalFortModules) 
        {
            t.Initialize();
        }

        Relinker.Modder.Dispose();
        Relinker.Modder = null;
    }

    internal static void ModuleEnd() 
    {
        foreach (var t in InternalFortModules) 
        {
            t.Unload();
        }
    }

    internal static void Register(this FortModule module) 
    {
        InternalFortModules.Add(module);

        if (module is NoModule)
            return;
        module.InternalLoad();
        module.LoadContent();
        module.Enabled = true;
        try 
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
                    PickupGraphicArrows.Add(stride, () => {
                        var identifier = (ArrowInfo)graphic.Invoke(null, Array.Empty<object>());
                        if (string.IsNullOrEmpty(identifier.Name))
                            identifier.Name = name;
                        return identifier;
                    });
                }

                ArrowsID.Add(name, stride);
                Arrows.Add(stride, loader);
                PickupID.Add(name, (Pickups)PickupLoaderCount);
                PickupLoader.Add((Pickups)PickupLoaderCount, (pos, targetPos, _) => new ArrowTypePickup(
                    pos, targetPos, stride));
                PickupLoaderCount++;
            }
            foreach (var dwBoss in type.GetCustomAttributes<CustomDarkWorldBossAttribute>()) 
            {
                if (dwBoss is null)
                    return;
                var bossName = dwBoss.BossName;

                ConstructorInfo ctor;
                DarkWorldBossLoader loader = null;
                ctor = type.GetConstructor(new Type[] { typeof(int) });
                if (ctor != null) 
                {
                    loader = diff => 
                    {
                        var invoked = (DarkWorldBoss)ctor.Invoke(new object[] { diff });
                        return invoked;
                    };
                    goto Loaded;
                }
                Loaded:
                DarkWorldBossLoader.Add(bossName, loader);
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
                    loader = (x, _) => 
                    {
                        var invoked = (LevelEntity)ctor.Invoke(new object[] { x });
                        return invoked;
                    };
                    goto Loaded;
                }
                ctor = type.GetConstructor(new Type[] { typeof(XmlElement), typeof(Vector2) });
                if (ctor != null) 
                {
                    loader = (x, pos) => 
                    {
                        var invoked = (LevelEntity)ctor.Invoke(new object[] { x, pos});
                        return invoked;
                    };
                    goto Loaded;
                }
                Loaded:
                LevelEntityLoader.Add(name, loader);
            }
        }
        }
        catch (Exception e) 
        {
            Logger.Log(e.ToString());
        }
    }

    internal static void Unregister(this FortModule module) 
    {
        module.Unload();
        InternalFortModules.Remove(module);
        InternalMods.Remove(module.Meta);
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
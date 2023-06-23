using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;
using FortRise.Adventure;
using Microsoft.Xna.Framework;
using Monocle;
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
    public static ReadOnlyCollection<ModResource> Mods => InternalMods.AsReadOnly();
    internal static List<FortModule> InternalFortModules = new();
    internal static HashSet<string> ModuleGuids = new();
    internal static List<ModResource> InternalMods = new();

    public static List<string> DetourLogs = new List<string>();
    public static Version FortRiseVersion;
    public static bool IsWindows { get; internal set; }


    public static bool DebugMode;

    private static ReadOnlySpan<char> SpanTest(ReadOnlySpan<char> variant) 
    {
        int offset = 0;
        Span<char> dest = default;
        variant.ToUpperInvariant(dest);
        for (int i = 0; i < dest.Length + offset; i++) 
        {
            if (char.IsUpper(dest[i + 1]) && char.IsLower(dest[i])) 
            {
                dest[i] += ' ';
                offset++;
            }
        }

        return dest; 
    }

    private static void Process() 
    {
        string variant = "BIG HEADS";

        ReadOnlySpan<char> parsed = SpanTest(variant.AsSpan());

        if (parsed.SequenceEqual("BigHeads".AsSpan())) 
        {

        }
    }

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

        // Get all mods metadata before loading the mod.
        foreach (var dir in directory) 
        {
            var metaPath = Path.Combine(dir, "meta.json");
            ModuleMetadata moduleMetadata = null;
            if (!File.Exists(metaPath)) 
                metaPath = Path.Combine(dir, "meta.xml");
            if (!File.Exists(metaPath))
                continue;
            
            if (metaPath.EndsWith("json"))
                moduleMetadata = ParseMetadataWithJson(dir, metaPath);
            else
                moduleMetadata = ParseMetadataWithXML(dir, metaPath);


            // Assembly Mod Loading
            var fortContent = new FortContent(moduleMetadata.PathDirectory);
            var modResource = new ModResource(fortContent, moduleMetadata);
            InternalMods.Add(modResource);
        
            // var pathToAssembly = Path.GetFullPath(Path.Combine(dir, moduleMetadata.DLL));
            // if (!File.Exists(pathToAssembly))
            //     continue;

            // using var fs = File.OpenRead(pathToAssembly);

            // var asm = Relinker.GetRelinkedAssembly(moduleMetadata, pathToAssembly, fs);
            // RegisterAssembly_OLD(moduleMetadata, asm);
        }

        foreach (var mod in InternalMods) 
        {
            var metadata = mod.Metadata;
            if (metadata.DLL == string.Empty) 
                return;
            
            var pathToAssembly = Path.GetFullPath(metadata.DLL);
            if (!File.Exists(pathToAssembly))
                return;

            using var fs = File.OpenRead(pathToAssembly);

            var asm = Relinker.GetRelinkedAssembly(metadata, pathToAssembly, fs);
            if (asm == null) 
            {
                Logger.Error("Failed to load assembly: " + asm.FullName);
                return;
            }
            RegisterAssembly(metadata, mod, asm);
        }
    }

    private static void RegisterAssembly(ModuleMetadata metadata, ModResource resource, Assembly asm) 
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
                obj.Meta = metadata;
                obj.Name = customAttribute.Name;
                obj.ID = customAttribute.GUID;
                var content = resource.Content;
                obj.Content = content;

                ModuleGuids.Add(obj.ID);
                obj.Register();

                Logger.Info($"{obj.ID}: {obj.Name} Registered.");
            }
        }
    }

    private static ModuleMetadata ParseMetadataWithXML(string dir, string path) 
    {
        var xml = Calc.LoadXML(path)["meta"];
        var dll = xml.ChildText("dll", null);
        var name = xml.ChildText("name", null);
        if (name == null)
        {
            Logger.Error($"{dir} does not have a name metadata.");
            return null;
        }
        var version = xml.ChildText("version", "1.0.0");
        var requiredVersion = new Version(xml.ChildText("required", "4.0.0"));
        var description = xml.ChildText("description", string.Empty);
        var author = xml.ChildText("author", string.Empty);
        var xmlDependencies = xml.ChildStringArray("dependencies"); 
        var nativePath = xml.ChildText("nativePath", string.Empty);
        var nativePathX86 = xml.ChildText("nativePathX86", string.Empty);
        string[] dependencies = xmlDependencies;
        
        if (FortRiseVersion < requiredVersion) 
        {
            Logger.Error($"Mod Name: {name} has a higher version of FortRise required {requiredVersion}. Your FortRise version: {FortRiseVersion}");
            return null;
        }
        return new ModuleMetadata() 
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
    }

    private static ModuleMetadata ParseMetadataWithJson(string dir, string path) 
    {
        var json = JsonTextReader.FromFile(path);
        var dll = json.GetJsonValueOrNull("dll");
        var name = json.GetJsonValueOrNull("name");
        if (name == null)
        {
            Logger.Error($"{dir} does not have a name metadata.");
            return null;
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
            return null;
        }
        return new ModuleMetadata() 
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
            foreach (var pickup in type.GetCustomAttributes<CustomPickupAttribute>()) 
            {
                if (pickup is null)
                    return;
                var pickupName = pickup.Name;
                var stride = PickupLoaderCount;
                ConstructorInfo ctor = type.GetConstructor(new Type[2] { typeof(Vector2), typeof(Vector2) });
                PickupLoader loader = null;

                if (ctor != null) 
                {
                    loader = (pos, targetPos, idx) => 
                    {
                        var custom = (Pickup)ctor.Invoke(new object[2] { pos, targetPos });
                        if (custom is CustomOrbPickup customOrb) 
                        {
                            var info = customOrb.CreateInfo();
                            customOrb.Sprite = info.Sprite;
                            customOrb.LightColor = info.Color.Invert();
                            customOrb.Collider = info.Hitbox;
                            customOrb.Border.Color = info.Color;
                            customOrb.Sprite.Play(0);
                            customOrb.Add(customOrb.Sprite);
                        }

                        return custom;
                    };
                }

                PickupID.Add(pickupName, (Pickups)stride);
                PickupLoader.Add((Pickups)PickupLoaderCount, loader);
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
        // InternalMods.Remove(module.Meta);
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
using FortRise.Adventure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using TowerFall;
using YYProject.XXHash;

namespace FortRise;

public delegate Enemy EnemyLoader(Vector2 position, Facing facing, Vector2[] nodes);
public delegate DarkWorldBoss DarkWorldBossLoader(int difficulty);
public delegate patch_Arrow ArrowLoader();
public delegate Pickup PickupLoader(Vector2 position, Vector2 targetPosition, int playerIndex);
public delegate LevelEntity LevelEntityLoader(XmlElement x, Vector2 position, Vector2[] nodes);
public delegate CustomMenuState CustomMenuStateLoader(MainMenu menu);


/// <summary>
/// Core API of FortRise
/// </summary>
public static partial class RiseCore
{
    /// <summary>
    /// A TowerFall root directory.
    /// </summary>
    public static string GameRootPath { get; internal set; }
    internal static Dictionary<string, EnemyLoader> EnemyLoader => EntityRegistry.EnemyLoader;
    internal static Dictionary<string, DarkWorldBossLoader> DarkWorldBossLoader = new();
    internal static Dictionary<string, LevelEntityLoader> LevelEntityLoader = new();


    internal static FortModule AdventureModule;
    internal static ModuleManager ModuleManager = new();
    internal static readonly HashAlgorithm ChecksumHasher = XXHash64.Create();

    internal static List<string> DetourLogs = new List<string>();

    /// <summary>
    /// A current version of FortRise.
    /// <note>This should be not used to check for Latest Version and Current Version,
    /// this is an information for logging purposes.</note>
    /// </summary>
    public static SemanticVersion FortRiseVersion { get; internal set; }
    /// <summary>
    /// Checks if the OS that is currently running is Windows.
    /// </summary>
    /// <value>true if the OS is running on Windows, otherwise false.</value>
    public static bool IsWindows { get; internal set; }

    public static bool NoIntro { get; private set; }
    public static bool NoAutoPause { get; private set; }
    public static bool NoErrorScene { get; private set; }
    public static bool NoRichPresence { get; private set; }
    public static bool DumpResources { get; private set; }
    public static bool DisableFortMods { get; private set; }
    internal static string[] ApplicationArgs;

    internal static bool CantRestart = true;

    /// <summary>
    /// Check if the game is about to restart.
    /// </summary>
    public static bool WillRestart { get; set; }

    internal static HashSet<string> ReadBlacklistedMods(string blackListPath)
    {
        try
        {
            var json = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(blackListPath));
            var blacklisted = new HashSet<string>();
            foreach (var j in json)
            {
                blacklisted.Add(j);
            }
            return blacklisted;
        }
        catch
        {
            return new HashSet<string>(0);
        }
    }

    internal static bool Start()
    {
        GameRootPath = Path.GetDirectoryName(typeof(TFGame).Assembly.Location);

        var modDirectory = Path.Combine(GameRootPath, "Mods");
        var modUpdater = Path.Combine(GameRootPath, "ModUpdater");

        // Check for the important directories
        if (!Directory.Exists(modDirectory))
        {
            Directory.CreateDirectory(modDirectory);
        }

        if (!Directory.Exists(modUpdater))
        {
            Directory.CreateDirectory(modUpdater);
        }

        // Check for updates
        string updaterPath = Path.Combine(modUpdater, "updater.json");
        if (File.Exists(updaterPath))
        {
            var json = File.ReadAllText(updaterPath);
            var updaterInfos = JsonSerializer.Deserialize<UpdateChecks.UpdaterInfo[]>(json, UpdateChecks.UpdaterInfoOptions);

            foreach (var info in updaterInfos)
            {
                Logger.Info($"Updating {info.ModName} {info.Version} --> {info.ModName} {info.UpdateVersion}");
                string oldMod = Path.Combine(modDirectory, info.ModPath);
                if (info.IsZipped)
                {
                    if (File.Exists(oldMod))
                    {
                        File.Delete(oldMod);
                    }
                }
                else 
                {
                    if (Directory.Exists(oldMod))
                    {
                        Directory.Delete(oldMod, true);
                    }
                }

                File.Move(info.UpdateModPath, Path.Combine(modDirectory, Path.GetFileName(info.UpdateModPath)));
            }
            File.Delete(updaterPath);
        }


        // Load env mods here
        ModuleManager.BlacklistedMods = ReadBlacklistedMods(Path.Combine(modDirectory, "blacklist.txt"));
        var directory = Directory.GetDirectories(modDirectory);
        foreach (var dir in directory)
        {
            if (dir.Contains("_RelinkerCache"))
                continue;
            var dirInfo = new DirectoryInfo(dir);
            if (ModuleManager.BlacklistedMods != null && ModuleManager.BlacklistedMods.Contains(dirInfo.Name))
                continue;

            SetEnvDir(dir);
        }
        var files = Directory.GetFiles(modDirectory);
        foreach (var file in files)
        {
            if (!file.EndsWith("zip"))
                continue;
            var fileName = Path.GetFileName(file);
            if (ModuleManager.BlacklistedMods != null && ModuleManager.BlacklistedMods.Contains(Path.GetFileName(fileName)))
                continue;

            SetEnvZip(file);
        }
        static void SetEnvDir(string dir)
        {
            var metaPath = Path.Combine(dir, "env.json");
            if (!File.Exists(metaPath))
                return;
            var value = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(metaPath));
            foreach (var val in value)
            {
                Environment.SetEnvironmentVariable(val.Key, val.Value);
            }
        }

        static void SetEnvZip(string file)
        {
            using var zipFile = ZipFile.OpenRead(file);

            var envZip = zipFile.GetEntry("env.json");

            if (envZip == null)
                return;

            using var memStream = envZip.ExtractStream();
            var value = JsonSerializer.Deserialize<Dictionary<string, string>>(memStream);
            foreach (var val in value)
            {
                Environment.SetEnvironmentVariable(val.Key, val.Value);
            }
        }

        return true;
    }

    internal static void ModuleStart()
    {
        RiseCore.Flags();
        GameChecksum = GetChecksum(typeof(TFGame).Assembly.Location).ToHexadecimalString();

        var fortRiseMetadata = new ModuleMetadata() {
            Name = "FortRise",
            Version = FortRiseVersion
        };
        var fortRiseModule = new NoModule(fortRiseMetadata);
        ModuleManager.InternalFortModules.Add(fortRiseModule);
        ModuleManager.InternalModuleMetadatas.Add(fortRiseMetadata);

        AdventureModule = new AdventureModule();
        AdventureModule.InternalLoad();
        ModuleManager.InternalFortModules.Add(AdventureModule);

        QuestEventRegistry.LoadAllBuiltinEvents();
        CustomMenuStateRegistry.LoadAllBuiltinMenuState();

        DetourManager.DetourApplied += info => {
            if (GetHookOwner(out bool isMMHOOK) is not Assembly owner)
            {
                return;
            }

            if (!isMMHOOK)
            {
                DetourLogs.Add($"new Detour by {owner.GetName().Name}: {info.Method.Method.GetID()}");
            }
            else 
            {
                DetourLogs.Add($"new On.+= by {owner.GetName().Name}: {info.Method.Method.GetID()}");
            }
        };

        DetourManager.ILHookApplied += info => {
            if (GetHookOwner(out bool isMMHOOK) is not Assembly owner)
            {
                return;
            }

            if (!isMMHOOK)
            {
                DetourLogs.Add($"new ILHook by {owner.GetName().Name}: {info.Method.Method.GetID()}");
            }
            else 
            {
                DetourLogs.Add($"new IL.+= by {owner.GetName().Name}: {info.Method.Method.GetID()}");
            }
        };

        AtlasReader.Initialize();
        // load the internals first
        ModuleManager.LoadModsFromDirectory(Path.Combine(GameRootPath, "Internals"));
        ModuleManager.LoadModsFromDirectory(Path.Combine(GameRootPath, "Mods"));
        if (!NoRichPresence)
            DiscordComponent.Create();
    }

    internal static void ParseArgs(string[] args)
    {
        string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "launch.txt");
        var compiledArgs = new List<string>(args);
        if (File.Exists(file))
        {
            using var fs = File.OpenText(file);
            string argPass;
            while ((argPass = fs.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(argPass) || argPass.Trim().StartsWith(';'))
                {
                    continue;
                }
                var splittedArgs = argPass.Trim().Split(' ');
                foreach (var splittedArg in splittedArgs)
                {
                    if (!compiledArgs.Contains(splittedArg))
                    {
                        compiledArgs.Add(argPass.Trim());
                    }
                }
            }
        }
        else
        {
            using var fs = File.CreateText(file);
            fs.WriteLine("; Add any of available launch arguments here.");
            fs.WriteLine("; Lines starting with ; are ignored, so all of the arguments are disabled.");
            fs.WriteLine("");
            fs.WriteLine(";--debug");
            fs.WriteLine(";--nointro");
            fs.WriteLine("; You can also change your graphics driver, it can be either OpenGL, DirectX, or Vulkan");
            fs.WriteLine("; The defualt may be depends on your platform and some platform doesn't support one of the other graphics driver.");
            fs.WriteLine(";--graphics OpenGL");
        }

        Logger.Verbosity = Logger.LogLevel.Error;
        int cursor = 0;
        while (cursor < compiledArgs.Count)
        {
            var arg = compiledArgs[cursor];
            switch (arg)
            {
            case "--verbose":
                Logger.Verbosity = Logger.LogLevel.Assert;
                break;
            case "--no-rich-presence":
                NoRichPresence = true;
                break;
            case "--no-auto-pause":
                NoAutoPause = true;
                break;
            case "--dump-resources":
                DumpResources = true;
                break;
            case "--use-scancodes":
                Environment.SetEnvironmentVariable("FNA_KEYBOARD_USE_SCANCODES", "1");
                break;
            case "--no-quit":
                MainMenu.NoQuit = true;
                break;
            case "--no-gamepads":
                MainMenu.NoGamepads = true;
                break;
            case "--no-gamepadsupdates":
                MainMenu.NoGamepadUpdates = true;
                break;
            case "--nointro":
                NoIntro = true;
                break;
            case "--no-error-scene":
                NoErrorScene = true;
                break;
            case "--loadlog":
                TFGame.StartLoadLog();
                break;
            case "--graphics":
                cursor++;
                arg = compiledArgs[cursor];
                Environment.SetEnvironmentVariable("FNA3D_FORCE_DRIVER", arg);
                break;
            case "--disable-fort-mods":
                DisableFortMods = true;
                break;
            case "--level-quick-start":
                cursor++;
                arg = compiledArgs[cursor];
                try
                {
                    var argSpan = arg.AsSpan();
                    var slashSplit = argSpan.SplitLines('/');
                    byte phase = 0;

                    int num = 0;
                    string towerSet = null;
                    string levelID = null;
                    foreach (var (slashSet, _) in slashSplit)
                    {
                        switch (phase)
                        {
                        case 0:
                            towerSet = slashSet.ToString();
                            break;
                        case 1:
                            levelID = slashSet.ToString();
                            break;
                        case 2:
                            var span = slashSet.SplitLines('.');
                            foreach (var (s, x) in span)
                            {
                                if (s[0] == '0')
                                {
                                    num = int.Parse(s[1].ToString());
                                    break;
                                }
                                num = int.Parse(s.ToString());
                                break;
                            }
                            break;
                        }

                        phase++;
                    }
                    if (towerSet == null)
                    {
                        Logger.Error("[Quick Start] Couldn't quick start as TowerSet is missing");
                        continue;
                    }
                    if (levelID == null)
                    {
                        Logger.Error("[Quick Start] Couldn't quick start as LevelID is missing");
                        continue;
                    }

                    RiseCore.Events.OnPostInitialize += () => {
                        TowerRegistry.PlayDarkWorld(towerSet, towerSet + "/" + levelID, DarkWorldDifficulties.Legendary, num);
                    };
                }
                catch (Exception ex)
                {
                    Logger.Error("[Quick Start] Couldn't quick start as the passed arguments is invalid");
                    Logger.Error(ex);
                }

                break;
            }
            cursor++;
        }

        ApplicationArgs = compiledArgs.ToArray();
    }

    /// <summary>
    /// Ask the mod loader to restart the game. If it isn't possible to restart, the call will be ignored.
    /// </summary>
    public static void AskForRestart(FortModule module)
    {
        string name = module.Meta.Name;
        if (CantRestart)
        {
            Logger.Warning($"[RESTART] {name} asked for restart. But it was rejected as the game is not ready yet.");
            return;
        }

        Logger.Info($"[RESTART] {name} asked for restart. Restarting the game...");
        WillRestart = true;
        Engine.Instance.Exit();
    }

    internal static void InternalRestart()
    {
        WillRestart = true;
        Engine.Instance.Exit();
    }

    internal static void RunTowerFallProcess(string towerFallPath, string[] args)
    {
        bool noIntroFound = false;
        var sb = new StringBuilder();
        foreach (var arg in args)
        {
            sb.Append(arg + " ");
            if (arg is "-nointro" or "nointro")
            {
                noIntroFound = true;
            }
        }
        if (!noIntroFound)
            sb.Append("-nointro");
        var process = new Process();
        process.StartInfo.FileName = towerFallPath;
        process.StartInfo.Arguments = sb.ToString();

        process.Start();
    }

    // Generated at patch-time
    [PatchFlags]
    internal static void Flags() {}


    internal static byte[] GetChecksum(string path)
    {
        using var fs = File.OpenRead(path);
        return ChecksumHasher.ComputeHash(fs);
    }

    internal static byte[] GetChecksum(ModuleMetadata meta)
    {
        return GetChecksum(meta.DLL);
    }

    // https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Everest/Everest.cs
    internal static byte[] GetChecksum(ref Stream stream)
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
        Logger.Info(ModuleManager.InternalMods.Count + " total of mods loaded");
    }

    internal static void Initialize()
    {
        ModuleManager.Initialize();    
    }

    internal static void ModuleEnd()
    {
        foreach (var t in ModuleManager.InternalFortModules)
        {
            t.InternalUnload();
        }
    }

    internal static void ModsAfterLoad()
    {
        foreach (var mod in ModuleManager.InternalFortModules)
        {
            mod.AfterLoad();
        }
    }

    internal static void RegisterMods()
    {
        foreach (var mod in ModuleManager.InternalFortModules)
        {
            TowerFall.Loader.Message = ("Registering " + mod.Meta.Name + " Features").ToUpperInvariant();
            mod.Register();
        }
        FortRise.RiseCore.ModsAfterLoad();
    }

    internal static void WriteBlacklist(List<string> ctx, string path)
    {
        var json = JsonSerializer.Serialize(ctx);
        File.WriteAllText(path, json);
    }

    internal static void Register(this FortModule module)
    {
        if (module is NoModule or Adventure.AdventureModule)
            return;

        try
        {
            module.LoadContent();
        }
        catch (Exception ex)
        {
            Logger.Error($"[Register] [{module.Meta.Name}] There was an error trying to load a content.");
            Logger.Error(ex.ToString());
            return;
        }

        module.Enabled = true;
        List<Action> laziedRegisters = new List<Action>();

        try
        {
        foreach (var type in module.GetType().Assembly.GetTypes())
        {
            if (type is null)
                continue;

            GameModeRegistry.Register(type, module);
            foreach (CustomEnemyAttribute attrib in type.GetCustomAttributes<CustomEnemyAttribute>())
            {
                if (attrib is null)
                    continue;
                EntityRegistry.AddEnemy(module, type, attrib.Names);
            }
            foreach (var clea in type.GetCustomAttributes<CustomLevelEntityAttribute>())
            {
                if (clea is null)
                    continue;
                foreach (var name in clea.Names)
                {
                    string id;
                    string methodName = string.Empty;
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
                        Logger.Error($"[Loader] [{module.Meta.Name}] Invalid syntax of custom entity ID: {name}, {type.FullName}");
                        continue;
                    }
                    id = id.Trim();
                    methodName = methodName?.Trim();

                    ConstructorInfo ctor;
                    MethodInfo info;
                    LevelEntityLoader loader = null;
                    info = type.GetMethod(methodName, new Type[] { typeof(XmlElement) });
                    if (info != null && info.IsStatic && info.ReturnType.IsCompatible(typeof(LevelEntity)))
                    {
                        loader = (xml, _, _) => {
                            var invoked = (LevelEntity)info.Invoke(null, new object[] {
                               xml
                            });
                            return invoked;
                        };
                        goto Loaded;
                    }

                    info = type.GetMethod(methodName, new Type[] { typeof(XmlElement), typeof(Vector2) });
                    if (info != null && info.IsStatic && info.ReturnType.IsCompatible(typeof(LevelEntity)))
                    {
                        loader = (xml, pos, _) => {
                            var invoked = (LevelEntity)info.Invoke(null, new object[] {
                                xml, pos
                            });
                            return invoked;
                        };
                        goto Loaded;
                    }

                    info = type.GetMethod(methodName, new Type[] { typeof(XmlElement), typeof(Vector2), typeof(Vector2[]) });
                    if (info != null && info.IsStatic && info.ReturnType.IsCompatible(typeof(LevelEntity)))
                    {
                        loader = (xml, pos, nodes) => {
                            var invoked = (LevelEntity)info.Invoke(null, new object[] {
                                xml, pos, nodes
                            });
                            return invoked;
                        };
                        goto Loaded;
                    }

                    ctor = type.GetConstructor(new Type[] { typeof(XmlElement) });
                    if (ctor != null)
                    {
                        loader = (x, _, _) =>
                        {
                            var invoked = (LevelEntity)ctor.Invoke(new object[] { x });
                            return invoked;
                        };
                        goto Loaded;
                    }
                    ctor = type.GetConstructor(new Type[] { typeof(XmlElement), typeof(Vector2) });
                    if (ctor != null)
                    {
                        loader = (x, pos, _) =>
                        {
                            var invoked = (LevelEntity)ctor.Invoke(new object[] { x, pos});
                            return invoked;
                        };
                        goto Loaded;
                    }
                    ctor = type.GetConstructor(new Type[] { typeof(XmlElement), typeof(Vector2), typeof(Vector2[]) });
                    if (ctor != null)
                    {
                        loader = (x, pos, nodes) =>
                        {
                            var invoked = (LevelEntity)ctor.Invoke(new object[] { x, pos, nodes });
                            return invoked;
                        };
                        goto Loaded;
                    }
                    Loaded:
                    LevelEntityLoader[name] = loader;
                }
            }

            FortRise.ArrowsRegistry.Register(type, module);
            // laziedRegisters exists for PickupRegistry since it sometimes depends on Arrows
            laziedRegisters.Add(() => FortRise.PickupsRegistry.Register(type, module));
            foreach (var dwBoss in type.GetCustomAttributes<CustomDarkWorldBossAttribute>())
            {
                if (dwBoss is null)
                    continue;
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
                DarkWorldBossLoader[bossName] = loader;
            }
        }

        foreach (var lazy in laziedRegisters)
        {
            lazy();
        }
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
        }

        module.AfterLoad();
    }

    internal static void Unregister(this FortModule module)
    {
        module.InternalUnload();
    }

    internal static void LogDetours(Logger.LogLevel level = Logger.LogLevel.Debug)
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

    internal static Assembly GetHookOwner(out bool isMMHOOK, StackTrace trace = null)
    {
        isMMHOOK = false;

        if (trace == null)
        {
            trace = new StackTrace();
        }

        int frameCount = trace.FrameCount;
        for (int i = 0; i < frameCount; i++)
        {
            StackFrame frame = trace.GetFrame(i);
            MethodBase caller = frame.GetMethod();

            if (caller == null)
            {
                continue;
            }

            var declaringType = caller.DeclaringType;

            if (declaringType == null)
            {
                continue;
            }

            Assembly assembly = declaringType.Assembly;

            if (assembly == null || 
                assembly == Assembly.GetExecutingAssembly() ||
                assembly == typeof(Hook).Assembly)
            {
                continue;
            }

            if (assembly.GetName().Name.StartsWith("MMHOOK_"))
            {
                isMMHOOK = true;
                continue;
            }
            return assembly;
        }

        return null;
    }

    /// <summary>
    /// Checks if a <see cref="FortRise.FortModule"/> exists or been loaded. This checks for a Fort name, not a metadata name.
    /// </summary>
    /// <param name="modName">A Fort name, not a metadata name</param>
    /// <returns>true if found, else false</returns>
    [Obsolete("FortModule could use the 'Interop' instead")]
    public static bool IsModExists(string modName)
    {
        foreach (var module in ModuleManager.InternalFortModules)
        {
            if (module.Meta.Name == modName)
            {
                return true;
            }
        }
        return false;
    }
}
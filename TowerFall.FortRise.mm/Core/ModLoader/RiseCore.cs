using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using TowerFall;
using YYProject.XXHash;
using Microsoft.Extensions.Logging;

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
    internal static Dictionary<string, LevelEntityLoader> LevelEntityLoader => EntityRegistry.LevelEntityLoader;


    internal static Mod FortRiseModule;
    internal static ModuleManager ModuleManager;
    internal static readonly HashAlgorithm ChecksumHasher = XXHash64.Create();

    /// <summary>
    /// A current version of FortRise.
    /// <note>This should be not used to check for Latest Version and Current Version,
    /// this is an information for logging purposes.</note>
    /// </summary>
    public static SemanticVersion FortRiseVersion { get; internal set; }
    /// <summary>
    /// Checks if the OS that is currently running is Windows.
    /// </summary>
    /// <value>true if the OS is running on Windows.</value>
    public static bool IsWindows { get; internal set; }
    /// <summary>
    /// Checks if the game is launched from Steam.
    /// </summary>
    /// <value>true if the game is launched from Steam.</value>
    public static bool IsSteam { get; internal set; }

    public static bool NoIntro { get; private set; }
    public static bool NoAutoPause { get; private set; }
    public static bool NoErrorScene { get; private set; }
    public static bool NoRichPresence { get; private set; }
    public static bool DumpResources { get; private set; }
    internal static string[] ApplicationArgs;

    internal static bool CantRestart = true;

    /// <summary>
    /// Check if the game is about to restart.
    /// </summary>
    public static bool WillRestart { get; set; }

    internal static ILogger logger;
    private static ILoggerFactory loggerFactory;

    internal static void LauncherPipe(ILogger logger, ILoggerFactory loggerFactory)
    {
        RiseCore.logger = logger;
        RiseCore.loggerFactory = loggerFactory;
        RiseCore.logger.LogInformation("Piped logger successfully!");
    }

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
            return [];
        }
    }

    internal static bool Start()
    {
        RiseCore.Flags();   

        ModuleManager = new ModuleManager(logger, loggerFactory);
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

        ModuleStart();
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
        // warm up
        IDPool.WarmIndex("boss", 3);
        IDPool.WarmIndex("archers", 8);
        IDPool.WarmIndex("characterSounds", 13);
        GameChecksum = GetChecksum(typeof(TFGame).Assembly.Location).ToHexadecimalString();

        FortRiseModule = ModuleManager.CreateFortRiseModule();

        CustomMenuStateRegistry.LoadAllBuiltinMenuState();

        AtlasReader.Initialize();
        // load the internals first
        ModuleManager.LoadModsFromDirectory(Path.Combine(GameRootPath, "Internals"));
        ModuleManager.LoadModsFromDirectory(Path.Combine(GameRootPath, "Mods"));
        ModuleManager.EventsManager.OnModLoadStateFinished.Raise(null, LoadState.Load);
        // if (!NoRichPresence)
        // {
        //     DiscordComponent.Create();
        // }
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

                        // RiseCore.Events.OnPostInitialize += () =>
                        // {
                        //     TowerRegistry.PlayDarkWorld(towerSet, towerSet + "/" + levelID, DarkWorldDifficulties.Legendary, num);
                        // };
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
    internal static void Flags() { }


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


    internal static void LogTotalModsLoaded()
    {
        Logger.Info(ModuleManager.InternalMods.Count + " total of mods loaded");
    }

    internal static void Initialize()
    {
        ModuleManager.Initialize();
    }

    internal static void ModuleEnd()
    {
        foreach (var mod in ModuleManager.InternalFortModules)
        {
            UnloadMod(mod);
        }
    }

    internal static void UnloadMod(Mod mod)
    {
        // first, call the unload first
        mod.OnUnload?.Invoke(mod.Context);

        // then unload all FortRise managed objects
        // TODO: unload all mod's features, useful for hot reload
        mod.Context.Harmony.UnpatchAll();
        ModEventsManager.Instance.RemoveByMod(mod);
    }

    internal static void WriteBlacklist(List<string> ctx, string path)
    {
        var json = JsonSerializer.Serialize(ctx);
        File.WriteAllText(path, json);
    }

    internal static void Unregister()
    {
    }
}

using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Steamworks;

namespace TowerFall;

internal static class NativeMethods 
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetDefaultDllDirectories(int directoryFlags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern void AddDllDirectory(string lpPathName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetDllDirectory(string lpPathName);
}

public partial class patch_TFGame : TFGame
{
    public static bool SoundLoaded;
    private const int LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;

    public static patch_Atlas FortRiseMenuAtlas;
    public static bool Loaded
    {
        [MonoModIgnore]
        get => throw null;
        [MonoModIgnore]
        set => throw null;
    }



    private bool noIntro;
    public patch_TFGame(bool noIntro) : base(noIntro)
    {
    }

    public extern void orig_ctor(bool noIntro);

    [MonoModConstructor]
    public void ctor(bool noIntro) 
    {
        orig_ctor(noIntro);
        this.noIntro = RiseCore.DebugMode;
    }

    public extern static void orig_Main(string[] args);
    public static void Main(string[] args) 
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) 
        {
            try 
            {
                NativeMethods.SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
                NativeMethods.AddDllDirectory(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    Environment.Is64BitProcess ? "x64" : "x86"
                ));
            }
            catch 
            {
                NativeMethods.SetDllDirectory(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    Environment.Is64BitProcess ? "x64" : "x86"
                ));
            }
        }
        var patchFile = "PatchVersion.txt";
        if (File.Exists(patchFile)) 
        {
            try 
            {
                using var fs = File.OpenRead(patchFile);
                using TextReader reader = new StreamReader(fs);
                string readed;
                while ((readed = reader.ReadLine()) != null) 
                {
                    if (readed.Contains("Debug")) 
                    {
                        var debugMode = readed.Split(':')[1].Trim();
                        RiseCore.DebugMode = bool.Parse(debugMode);
                        if (RiseCore.DebugMode && Logger.Verbosity < Logger.LogLevel.Error)
                            Logger.Verbosity = Logger.LogLevel.Error;
                    }
                    if (readed.Contains("Verbose")) 
                    {
                        var verbose = readed.Split(':')[1].Trim();
                        if (bool.Parse(verbose))
                            Logger.Verbosity = Logger.LogLevel.Assert;
                    }
                    if (readed.Contains("Installer")) 
                    {
                        var version = readed.Split(':')[1].Trim();
                        RiseCore.FortRiseVersion = new System.Version(version);
                    }
                }
            }
            catch 
            {
                Logger.Log("Unable to load PatchVersion.txt, Debug Mode Proceed", Logger.LogLevel.Warning);
                Logger.Log("Please report this bug", Logger.LogLevel.Warning);
                RiseCore.DebugMode = true;
            }
        }
        if (RiseCore.DebugMode) 
        {
            // var detourModManager = new DetourModManager();
            // detourModManager.OnILHook += (assembly, source, dest) => 
            // {
            //     object obj = dest.Target;
            //     RiseCore.DetourLogs.Add($"ILHook from {assembly.GetName().Name}: {source.GetID()} :: {dest.Method?.GetID() ?? "??"}{(obj == null ? "" : $"(object: {obj})")}");
            // };
            // detourModManager.OnHook += (assembly, source, dest, obj) => 
            // {
            //     RiseCore.DetourLogs.Add($"Hook from {assembly.GetName().Name}: {source.GetID()} :: {dest.GetID()}{(obj == null ? "" : $"(object: {obj})")}");
            // };
            try 
            {
                Logger.AttachConsole(RiseCore.ConsoleAttachment());
            }
            catch (Exception e) 
            {
                Logger.Error("Failed to attach console.");
                Logger.Error(e.ToString());
            }
        }

        orig_Main(args);
        Logger.DetachConsole();
        Logger.WriteToFile("fortRiseLog.txt");
    }

    protected extern void orig_LoadContent();

    protected override void LoadContent()
    {
        orig_LoadContent();
        FortRiseMenuAtlas = AtlasExt.CreateAtlasFromEmbedded("Content\\Atlas\\menuatlas.xml", "Content\\Atlas\\menuatlas.png");
    }

    protected extern void orig_Initialize();

    protected override void Initialize() 
    {
        FortRise.RiseCore.ModuleStart();
        FortRise.RiseCore.Events.Invoke_OnPreInitialize();
        FortRise.RiseCore.Initialize();
        orig_Initialize();
        FortRise.RiseCore.Events.Invoke_OnPostInitialize();
        patch_Arrow.ExtendArrows();
        FortRise.RiseCore.LogAllTypes();
    }

    protected extern void orig_UnloadContent();

    protected override void UnloadContent()
    {
        FortRise.RiseCore.ModuleEnd();
        orig_UnloadContent();
    }

    protected extern void orig_Update(GameTime gameTime);

    protected override void Update(GameTime gameTime)
    {
        RiseCore.Events.Invoke_BeforeUpdate(gameTime);
        RiseCore.Events.Invoke_Update(gameTime);
        orig_Update(gameTime);
        RiseCore.Events.Invoke_AfterUpdate(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        RiseCore.Events.Invoke_BeforeRender(Monocle.Draw.SpriteBatch);
        RiseCore.Events.Invoke_Render(Monocle.Draw.SpriteBatch);
        base.Draw(gameTime);
        RiseCore.Events.Invoke_AfterRender(Monocle.Draw.SpriteBatch);
    }

    [MonoModReplace]
    public static void Load()
    {
        Task.Factory.StartNew(delegate
        {
            try
            {
                Loader.Message = "LOADING";
                Calc.Log(new object[] { "=== LOADING DATA ===" });
                Calc.Log(new object[] { "...Input" });
                TFGame.WriteLineToLoadLog("Initializing Input...");
                NewGamepadInput.Init();
                PlayerInput.AssignInputs();
                for (int i = 0; i < 4; i++)
                {
                    TFGame.Characters[i] = i;
                }
                Calc.Log(new object[] { "...Archer Data" });
                ArcherData.Initialize();
                Calc.Log(new object[] { "...Level Data" });
                GameData.Load();
                TFGame.WriteLineToLoadLog("Initialize Default Sessions...");
                MainMenu.VersusMatchSettings = MatchSettings.GetDefaultVersus();
                MainMenu.TrialsMatchSettings = MatchSettings.GetDefaultTrials();
                MainMenu.QuestMatchSettings = MatchSettings.GetDefaultQuest();
                MainMenu.DarkWorldMatchSettings = MatchSettings.GetDefaultDarkWorld();
                Calc.Log(new object[] { "...Awards and Tips" });
                TFGame.WriteLineToLoadLog("Loading Versus Awards...");
                VersusAwards.Initialize();
                TFGame.WriteLineToLoadLog("Loading Versus Tips...");
                GameTips.Initialize();
                Calc.Log(new object[] { "...Save Data" });
                TFGame.WriteLineToLoadLog("Verifying Save Data...");
                SaveData.Instance.Verify();
                SessionStats.Initialize();
                Calc.Log(new object[] { "...Entity Caching" });
                TFGame.WriteLineToLoadLog("Initializing Entity Caching...");
                Arrow.Initialize();
                Cache.Init<CatchShine>();
                Cache.Init<LightFade>();
                Cache.Init<Explosion>();
                Cache.Init<PlayerBreath>();
                Cache.Init<ShockCircle>();
                Cache.Init<SmallShock>();
                Cache.Init<QuestScorePopup>();
                Cache.Init<EnemyAttack>();
                Cache.Init<Prism>();
                Cache.Init<PrismVanish>();
                Cache.Init<PrismParticle>();
                Cache.Init<CrumbleWallChunk>();
                Cache.Init<AmaranthShot>();
                Cache.Init<BrambleSpore>();
                Cache.Init<CyclopsShot>();
                Cache.Init<CataclysmBullet>();
                Cache.Init<WorkshopPortal>();
                Calc.Log(new object[] { "...Particles and Lighting" });
                TFGame.WriteLineToLoadLog("Initializing Particle Systems...");
                Particles.Initialize();
                TFGame.WriteLineToLoadLog("Initializing Lighting Systems...");
                LightingLayer.Initialize();

                Calc.Log(new object[] { "...Shaders (1/2)" });
                TFGame.WriteLineToLoadLog("Initializing Shaders (1 of 2)...");
                ScreenEffects.Initialize();
                Calc.Log(new object[] { "...Shaders (2/2)" });
                TFGame.WriteLineToLoadLog("Initializing Shaders (2 of 2)...");
                TFGame.LoadLightingEffect();
                Calc.Log(new object[] { "=== LOADING COMPLETE ===" });
                TFGame.WriteLineToLoadLog("Loading Complete!");
                Loader.Message = "";
                TFGame.GameLoaded = true;
            }
            catch (Exception ex)
            {
                TFGame.Log(ex, true);
                TFGame.OpenLog();
                Engine.Instance.Exit();
            }
        });

        Task.Factory.StartNew(() => 
        {
            Calc.Log(new object[] { "...Music" });
            TFGame.WriteLineToLoadLog("Loading Music...");
            patch_Music.Initialize();
            if (!Sounds.Loaded)
            {
                Calc.Log("...SFX" );
                TFGame.WriteLineToLoadLog("Loading Sounds...");
                Sounds.Load();
            }
            SoundLoaded = true;
            Calc.Log("=== SOUND LOADING COMPLETE ===");
        });
    }

    [MonoModReplace]
    public static IEnumerator MainMenuLoadWait()
    {
        while (!TFGame.GameLoaded || !patch_TFGame.SoundLoaded)
        {
            yield return 0;
        }
        if (SaveData.NewDataCreated && MainMenu.LoadError == null)
        {
            Saver saver = new Saver(true, null);
            Engine.Instance.Scene.Add<Saver>(saver);
            saver.CanHandleError = true;
            while (!saver.Finished)
            {
                yield return 0;
            }
            saver = null;
        }
        MainMenu.PlayMenuMusic(false, false);
        Engine.ConsoleEnabled = SaveData.Instance.Options.DevConsole;
        if (SaveData.Instance.Unlocks.Ascension)
        {
            (Engine.Instance.Scene as MainMenu).Background.AscensionTransition();
        }
        Calc.Log(new object[] { "Steam? " + SteamAPI.IsSteamRunning().ToString() });
        patch_TFGame.Loaded = true;
        yield break;
    }
}
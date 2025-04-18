using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;
using SDL3;

namespace TowerFall
{
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

        [DllImport("libc", SetLastError = true)]
        public static extern int mprotect(IntPtr ptr, nuint len, int prot);
    }

    public partial class patch_TFGame : TFGame
    {
        private static string FILENAME;
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
        private ErrorPanel panel;
        public patch_TFGame(bool noIntro) : base(noIntro)
        {
        }

        public extern void orig_ctor(bool noIntro);

        [MonoModConstructor]
        public void ctor(bool noIntro) 
        {
            panel = new ErrorPanel();
            orig_ctor(noIntro || RiseCore.NoIntro);

            FortRise.RiseCore.ModuleStart();
            if (!RiseCore.NoIntro && !noIntro)
                this.noIntro = RiseCore.DebugMode;
        }

        [MonoModReplace]
        [STAThread]
        public static void Main(string[] args) 
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            var towerFallPath = Directory.GetCurrentDirectory();
            bool vanillaLaunch = false;
            bool parseVersion = false;
            foreach (var arg in args) 
            {
                if (parseVersion)
                {
                    if (!SemanticVersion.TryParse(arg, out SemanticVersion version))
                    {
                        throw new Exception("Invalid FortRise Version");
                    }

                    RiseCore.FortRiseVersion = version;
                    parseVersion = false;
                }
                if (arg == "--vanilla")
                {
                    vanillaLaunch = true;
                    break;
                }

                if (arg == "--version")
                {
                    parseVersion = true;
                }
            }

            if (vanillaLaunch) 
            {
                ThreadStart start = () => {
                    try 
                    {
                        AppDomain.CurrentDomain.ExecuteAssembly("fortOrig/TowerFall.exe");
                    }
                    catch (Exception e) 
                    {
                        Console.WriteLine(e.ToString());
                        Console.WriteLine(e.StackTrace);
                    }
                };
                Thread thread = new Thread(start);
                thread.Start();
                thread.Join();
                goto Exit;
            }
            
            if (!FortRise.RiseCore.Start()) 
            {
                SDL.SDL_ShowSimpleMessageBox(
                    SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, 
                    "CONTENT NOT FOUND",
                    "TowerFall Content cannot be found elsewhere.", IntPtr.Zero);
                return;
            }
            RiseCore.ParseArgs(args);
            var patchFile = "PatchVersion.txt";
            if (!File.Exists(patchFile)) 
            {
                patchFile = Path.Combine(RiseCore.GameRootPath, "PacthVersion.txt");
            }
            
            TFGame.WriteLineToLoadLog("Initializing Steam...");
            if (!TryInit())
                goto Exit;

            // execheap
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) 
            {
                const int PROT_READ = 1, PROT_WRITE = 2, PROT_EXEC = 4;

                //Allocate a bit of memory on the heap
                IntPtr heapAlloc = Marshal.AllocHGlobal(123);
                IntPtr heapPage = new IntPtr(heapAlloc.ToInt64() & ~0xfff);

                //Try to make it executable
                if (NativeMethods.mprotect(heapPage, 0x1000, PROT_READ | PROT_WRITE | PROT_EXEC) < 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "SELinux execheap probe failed! Please ensure FortRise has this permission, then try again");

                //Cleanup
                if (NativeMethods.mprotect(heapPage, 0x1000, PROT_READ | PROT_WRITE) < 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to revert memory permissions after SELinux execheap probe");

                Marshal.FreeHGlobal(heapAlloc);
            }

            try 
            {
                InnerMain(args);

                if (RiseCore.WillRestart) 
                {
                    RiseCore.RunTowerFallProcess(towerFallPath, args);
                }
            }
            catch (Exception e) 
            {
                Logger.Error(e.ToString());
                Logger.Error(e.StackTrace);
            }

            Exit:
            Logger.WriteToFile("fortRiseLog.txt");
            Environment.Exit(0);
        }

        [MonoModIfFlag("Steamworks")]
        [MonoModPatch("TryInit")]
        [MonoModOriginalName("TryInit")]
        public static bool TryInit_Steam() 
        {
            return SteamClient.Init();
        }

        [MonoModIfFlag("NoLauncher")]
        [MonoModPatch("TryInit")]
        [MonoModOriginalName("TryInit")]
        public static bool TryInit_NoLauncher() 
        {
            return true;
        }

        [MonoModIgnore]
        public static extern bool TryInit();

        public static void InnerMain(string[] args) 
        {
            bool noIntro = false;
            bool loadLog = false;
            foreach (string text in args)
            {
                if (text.ToLower(CultureInfo.InvariantCulture) == "nointro" || text.ToLower(CultureInfo.InvariantCulture) == "-nointro")
                {
                    noIntro = true;
                }
                else if (text.ToLower(CultureInfo.InvariantCulture) == "loadlog" || text.ToLower(CultureInfo.InvariantCulture) == "-loadlog")
                {
                    loadLog = true;
                }
                else if (text.ToLower(CultureInfo.InvariantCulture) == "noquit" || text.ToLower(CultureInfo.InvariantCulture) == "-noquit")
                {
                    MainMenu.NoQuit = true;
                }
                else if (text.ToLower(CultureInfo.InvariantCulture) == "nogamepads" || text.ToLower(CultureInfo.InvariantCulture) == "-nogamepads")
                {
                    MainMenu.NoGamepads = true;
                }
                else if (text.ToLower(CultureInfo.InvariantCulture) == "nogamepadupdates" || text.ToLower(CultureInfo.InvariantCulture) == "-nogamepadupdates")
                {
                    MainMenu.NoGamepadUpdates = true;
                }
            }
            if (loadLog)
            {
                TFGame.StartLoadLog();
            }

            TFGame.WriteLineToLoadLog("Initializing game window...");

            try
            {
                using (patch_TFGame tfgame = new patch_TFGame(noIntro))
                {
                    TFGame.WriteLineToLoadLog("Starting game...");
                    tfgame.Run();
                }
            }
            catch (NoSuitableGraphicsDeviceException)
            {
                SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "SAVE THIS MESSAGE!", "No suitable graphics card found!", IntPtr.Zero);
            }
            catch (Exception ex)
            {
                TFGame.Log(ex, false);
                TFGame.OpenLog();
                Engine.Instance.Exit();
                throw;
            }
        }

        // [MonoModIgnore]
        // [MonoModLinkTo("Monocle.Engine", "InternalRun")]
        // private extern void InternalRun();

        [PatchSDL2ToSDL3]
        protected extern void orig_LoadContent();

        protected override void LoadContent()
        {
            FortRiseMenuAtlas = AtlasExt.CreateAtlasFromEmbedded("Content.Atlas.menuatlas.xml", "Content.Atlas.menuatlas.png");
        }

        protected extern void orig_Initialize();

        protected override void Initialize() 
        {
            Content = new ModContentManager(Services, Directory.GetCurrentDirectory());
            orig_LoadContent();
            FortRise.RiseCore.Events.Invoke_OnPreInitialize();
            orig_Initialize();
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
            if (ErrorPanel.Show)
            {
                panel.Update();
            }
            RiseCore.ResourceReloader.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            RiseCore.Events.Invoke_BeforeRender(Monocle.Draw.SpriteBatch);
            RiseCore.Events.Invoke_Render(Monocle.Draw.SpriteBatch);
            base.Draw(gameTime);
            RiseCore.Events.Invoke_AfterRender(Monocle.Draw.SpriteBatch);
            if (ErrorPanel.Show)
            {
                panel.Render();
            }
        }

        [MonoModReplace]
        public static void Load()
        {
            TaskHelper.Run("loading data", () =>
            {
                try
                {
                    Loader.Message = "LOADING";
                    Logger.Log("[LOAD] --- LOADING DATA ---");

                    foreach (var mods in RiseCore.ModuleManager.InternalMods) 
                    {
                        mods.Content.LoadResources();
                    }
                    RiseCore.ResourceTree.AfterModdedLoadContent();
                    FortRise.RiseCore.RegisterMods();
                    Loader.Message = "INITIALIZING INPUT";
                    Logger.Log("[LOAD] ...Input");
                    TFGame.WriteLineToLoadLog("Initializing Input...");
                    patch_XGamepadInput.Init();
                    PlayerInput.AssignInputs();
                    for (int i = 0; i < 4; i++)
                    {
                        TFGame.Characters[i] = i;
                    }
                    Loader.Message = "INITIALIZING ARCHER DATA";
                    Logger.Log("[LOAD] ...Archer Data");
                    ArcherData.Initialize();

                    Loader.Message = "INITIALIZING MODS";
                    FortRise.RiseCore.Initialize();

                    Arrow.Initialize();
                    patch_Arrow.ExtendArrows();
                    patch_TreasureSpawner.ExtendTreasures();

                    Loader.Message = "INITIALIZING LEVEL DATA (1/2)";
                    Logger.Log("[LOAD] ...Level Data");
                    GameData.Load();
                    Loader.Message = "INITIALIZING LEVEL DATA (2/2)";

                    TowerPatchRegistry.Initialize();

                    Loader.Message = "INITIALIZING DEFAULT SESSION";
                    TFGame.WriteLineToLoadLog("Initialize Default Sessions...");
                    MainMenu.VersusMatchSettings = MatchSettings.GetDefaultVersus();
                    MainMenu.TrialsMatchSettings = MatchSettings.GetDefaultTrials();
                    MainMenu.QuestMatchSettings = MatchSettings.GetDefaultQuest();
                    MainMenu.DarkWorldMatchSettings = MatchSettings.GetDefaultDarkWorld();
                    Loader.Message = "LOADING VERSUS AWARDS AND TIPS";
                    Logger.Log("[LOAD] ...Awards and Tips");
                    TFGame.WriteLineToLoadLog("Loading Versus Awards...");
                    VersusAwards.Initialize();
                    TFGame.WriteLineToLoadLog("Loading Versus Tips...");
                    GameTips.Initialize();
                    Loader.Message = "VERIFYING SAVE DATA";
                    Logger.Log("[LOAD] ...Save Data");
                    TFGame.WriteLineToLoadLog("Verifying Save Data...");
                    SaveData.Instance.Verify();
                    SessionStats.Initialize();
                    Loader.Message = "CACHING ENTITIES";
                    Logger.Log("[LOAD] ...Entity Caching");
                    TFGame.WriteLineToLoadLog("Initializing Entity Caching...");
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
                    Loader.Message = "LOADING PARTICLES AND LIGHTING";
                    Logger.Log("[LOAD] ...Particles and Lighting");
                    TFGame.WriteLineToLoadLog("Initializing Particle Systems...");
                    Particles.Initialize();
                    TFGame.WriteLineToLoadLog("Initializing Lighting Systems...");
                    LightingLayer.Initialize();

                    Loader.Message = "LOADING SHADERS";
                    Logger.Log("[LOAD] ...Shaders (1/2)");
                    TFGame.WriteLineToLoadLog("Initializing Shaders (1 of 2)...");
                    ScreenEffects.Initialize();
                    Logger.Log("[LOAD] ...Shaders (2/2)");
                    TFGame.WriteLineToLoadLog("Initializing Shaders (2 of 2)...");
                    TFGame.LoadLightingEffect();
                    Logger.Log("[LOAD] --- LOADING COMPLETE ---");
                    TFGame.WriteLineToLoadLog("Loading Complete!");


                    TFGame.GameLoaded = true;
                    if (TaskHelper.WaitForAll()) 
                    {
                        Loader.Message = "WAITING FOR OTHER TASK TO COMPLETE";
                    }
                }
                catch (Exception ex)
                {
                    TFGame.Log(ex, true);
                    TFGame.OpenLog();
                    Engine.Instance.Exit();
                }
            });

            if (RiseCore.DumpResources)
                TaskHelper.RunAsync("dumping assets", RiseCore.ResourceTree.DumpAll);

            TaskHelper.Run("loading sfx", () => 
            {
                try 
                {
                    Stopwatch watch = Stopwatch.StartNew();
                    Logger.Log("[LOAD] ...Music");
                    TFGame.WriteLineToLoadLog("Loading Music...");
                    patch_Music.Initialize();
                    patch_Audio.InitMusicSystems();

                    foreach (var mods in RiseCore.ModuleManager.InternalMods) 
                    {
                        mods.Content.LoadAudio();
                    }

                    Logger.Info($"[LOAD] -- MUSIC LOADING: {watch.ElapsedMilliseconds} ms --");

                    watch = Stopwatch.StartNew();
                    if (!Sounds.Loaded)
                    {
                        Logger.Log("[LOAD] ...SFX" );
                        TFGame.WriteLineToLoadLog("Loading Sounds...");
                        Sounds.Load();
                    }

                    SoundLoaded = true;
                    Logger.Info($"[LOAD] -- SOUND LOADING: {watch.ElapsedMilliseconds} ms --");
                    watch.Stop();
                }
                catch (Exception ex)
                {
                    TFGame.Log(ex, true);
                    TFGame.OpenLog();
                    Engine.Instance.Exit();
                }
            });

            Task.Run(CheckUpdate);
            Task.Run(CheckModUpdate);
        }

        private static async Task CheckModUpdate()
        {
            var tasks = new List<Task<Result<bool, string>>>();
            foreach (var metadata in RiseCore.ModuleManager.InternalModuleMetadatas)
            {
                if (metadata.Name is "FortRise" or "Adventure")
                {
                    continue;
                }
                tasks.Add(RiseCore.UpdateChecks.CheckModUpdate(metadata));
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                if (!task.Result.Check(out var res, out string err))
                {
                    if (err != "NRF")
                    {
                        Logger.Error(err);
                    }
                    continue;
                }
            }
        }

        private static async Task CheckUpdate()
        {
            var result = await RiseCore.UpdateChecks.CheckFortRiseUpdate();
            if (!result.Check(out var res, out string err))
            {
                RiseCore.UpdateChecks.UpdateMessage = err.ToUpperInvariant();
                Logger.Error(err);
                return;
            }

            string url = res.Ref;
            int index = url.LastIndexOf('/');
            if (!SemanticVersion.TryParse(url.Substring(index + 1), out SemanticVersion version))
            {
                RiseCore.UpdateChecks.UpdateMessage = "ERROR PARSING THE VERSION NUMBER.";
                return;
            }

            RiseCore.UpdateChecks.UpdateFortRiseConfirm(version);

            if (RiseCore.UpdateChecks.FortRiseUpdateAvailable)
            {
                RiseCore.UpdateChecks.UpdateMessage = "FORTRISE UPDATE IS AVAILABLE!";
                return;
            }
            RiseCore.UpdateChecks.UpdateMessage = string.Empty;
        }

        [MonoModReplace]
        public static IEnumerator MainMenuLoadWait()
        {
            while (TaskHelper.WaitForAll())
            {
                yield return 0;
            }
            XNAFileDialog.GraphicsDevice = Engine.Instance.GraphicsDevice;
            // can be remove
            foreach (var gameMode in GameModeRegistry.LegacyGameModes) 
            {
                gameMode.InitializeSoundsInternal();
            }

            FortRise.RiseCore.Events.Invoke_OnPostInitialize();
            Loader.Message = "";
            if (SaveData.NewDataCreated && MainMenu.LoadError == null)
            {
                Saver saver = new Saver(true);
                Engine.Instance.Scene.Add(saver);
                saver.CanHandleError = true;
                while (!saver.Finished)
                {
                    yield return 0;
                }
                saver = null;
            }

            if (ErrorPanel.Errors.Count > 0)
            {
                ErrorPanel.Show = true;
            }
            MainMenu.PlayMenuMusic();
            Engine.ConsoleEnabled = SaveData.Instance.Options.DevConsole;
            if (SaveData.Instance.Unlocks.Ascension)
            {
                (Engine.Instance.Scene as MainMenu).Background.AscensionTransition();
            }
            patch_TFGame.Loaded = true;
            yield break;
        }

        [MonoModReplace]
        public static void OpenLog()
        {
            if (File.Exists(FILENAME))
            {
                var process = new Process();
                string os = SDL.SDL_GetPlatform();
                
                // is this safe?
                process.StartInfo.UseShellExecute = true;
                if (os.Equals("Linux"))
                {
                    process.StartInfo.FileName = "xdg-open";
                    process.StartInfo.Arguments = "\"" + FILENAME + "\"";
                }
                else if (os.Equals("Mac OS X"))
                {
                    process.StartInfo.FileName = "open";
                    process.StartInfo.Arguments = $"-a TextEdit \"{FILENAME}\"";
                }
                else 
                {
                    process.StartInfo.FileName = "\"" + FILENAME + "\"";
                }

                process.Start();
            }
        }

        [MonoModIfFlag("OS:NotWindows")]
        [PatchSDL2ToSDL3]
        [MonoModIgnore]
        private static extern string GetSavePath();

        [MonoModIfFlag("OS:Windows")]
        [MonoModPatch("GetSavePath")]
        [MonoModReplace]
        private static string Windows_GetSavePath()
        {
            return Directory.GetCurrentDirectory();
        }

        [PatchSDL2ToSDL3]
        [MonoModIgnore]
        private static extern string GetCloudSavePath();


        [MonoModIfFlag("OS:Windows")]
        [MonoModPatch("<>c")]
        public class TFGame_c
        {
            [MonoModIfFlag("OS:Windows")]
            [MonoModRemove]
            [MonoModPatch("<Load>b__120_0")]
            internal extern void Loadb__120_0();

            [MonoModIfFlag("OS:Windows")]
            [MonoModRemove]
            [MonoModPatch("<Load>b__118_0")]
            internal extern void Loadb__118_0();
        }
    }
}

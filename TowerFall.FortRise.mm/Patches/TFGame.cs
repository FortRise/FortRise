using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FortRise;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using SDL3;
[assembly: InternalsVisibleTo("FortRise.Content")]
[assembly: InternalsVisibleTo("FortRise.ImGui")]

namespace TowerFall
{
    internal static class NativeMethods 
    {
        [DllImport("libc", SetLastError = true)]
        public static extern int mprotect(IntPtr ptr, nuint len, int prot);

        [DllImport("libc", SetLastError = true)]
        public static extern nuint getpagesize();
    }

    public partial class patch_TFGame : TFGame
    {
        private static string FILENAME;
        public static bool SoundLoaded;

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
        }

        [MonoModReplace]
        [STAThread]
        public static void Main(string[] args) 
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
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

                if (arg == "--version")
                {
                    parseVersion = true;
                }
            }
            
            RiseCore.Start();
            RiseCore.ParseArgs(args);
            
            WriteLineToLoadLog("Initializing Steam...");
            if (!TryInit())
            {
                return;
            }

            // execheap
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) 
            {
                const int PROT_READ = 1, PROT_WRITE = 2, PROT_EXEC = 4;

                nuint pageSize = NativeMethods.getpagesize();

                //Allocate a bit of memory on the heap
                var heapAlloc = Marshal.AllocHGlobal(123);
                var heapPage = heapAlloc & ~(nint)(pageSize - 1);

                //Try to make it executable
                if (NativeMethods.mprotect(heapPage, pageSize, PROT_READ | PROT_WRITE | PROT_EXEC) < 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "SELinux execheap probe failed! Please ensure FortRise has this permission, then try again");

                //Cleanup
                if (NativeMethods.mprotect(heapPage, pageSize, PROT_READ | PROT_WRITE) < 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to revert memory permissions after SELinux execheap probe");

                Marshal.FreeHGlobal(heapAlloc);
            }

            try 
            {
                InnerMain(args);

                if (RiseCore.WillRestart) 
                {
                    string fortRisePath = Path.Combine(AppContext.BaseDirectory, "FortRise");
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        fortRisePath += ".exe";
                    }
                    RiseCore.RunProcess(fortRisePath, args);
                }
            }
            catch (Exception e) 
            {
                Logger.Error(e.ToString());
                Logger.Error(e.StackTrace);
            }
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
                if (text.Equals("nointro", StringComparison.CurrentCultureIgnoreCase) || text.Equals("-nointro", StringComparison.CurrentCultureIgnoreCase))
                {
                    noIntro = true;
                }
                else if (text.Equals("loadlog", StringComparison.CurrentCultureIgnoreCase) || text.Equals("-loadlog", StringComparison.CurrentCultureIgnoreCase))
                {
                    loadLog = true;
                }
                else if (text.Equals("noquit", StringComparison.CurrentCultureIgnoreCase) || text.Equals("-noquit", StringComparison.CurrentCultureIgnoreCase))
                {
                    MainMenu.NoQuit = true;
                }
                else if (text.Equals("nogamepads", StringComparison.CurrentCultureIgnoreCase) || text.Equals("-nogamepads", StringComparison.CurrentCultureIgnoreCase))
                {
                    MainMenu.NoGamepads = true;
                }
                else if (text.Equals("nogamepadupdates", StringComparison.CurrentCultureIgnoreCase) || text.Equals("-nogamepadupdates", StringComparison.CurrentCultureIgnoreCase))
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
            orig_LoadContent();
            FortRiseMenuAtlas = AtlasExt.CreateAtlasFromEmbedded("Content.Atlas.menuatlas.xml", "Content.Atlas.menuatlas.png");

            var assembly = Assembly.GetExecutingAssembly();
            using Stream fortRisePng = assembly.GetManifestResourceStream("Content.Atlas.fortrise.png");

            FortRiseModule.FortRiseIcon = new Subtexture(
                new Monocle.Texture(Texture2D.FromStream(GraphicsDevice, fortRisePng))
            );

            RiseCore.ModuleManager.NameToIcon["FortRise"] = FortRiseModule.FortRiseIcon;

            foreach (var batch in ModuleManager.Instance.RegistryBatches[RegistryBatchType.PreloadedContent])
            {
                batch.Invoke();
            }
        }

        protected extern void orig_Initialize();

        protected override void Initialize() 
        {
            Content = new ModContentManager(Services, Directory.GetCurrentDirectory());
            FortRise.RiseCore.Events.Invoke_OnPreInitialize();
            orig_Initialize();
            FortRise.RiseCore.LogTotalModsLoaded();
            ModEventsManager.Instance.OnGameInitialized.Raise(this, this);
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
                    Loader.Message = "LOADING SFX";

                    Stopwatch watch = Stopwatch.StartNew();
                    Logger.Log("[LOAD] ...Music");
                    TFGame.WriteLineToLoadLog("Loading Music...");
                    lock (patch_Sounds.SoundLoadLock) 
                    {
                        patch_Music.Initialize();
                        patch_Audio.InitMusicSystems();
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

                    RiseCore.ResourceTree.AfterModdedLoadContent();
                    Loader.Message = "INITIALIZING INPUT";
                    Logger.Log("[LOAD] ...Input");
                    TFGame.WriteLineToLoadLog("Initializing Input...");
                    TowerFall.Patching.XGamepadInput.Init();
                    PlayerInput.AssignInputs();
                    for (int i = 0; i < 4; i++)
                    {
                        TFGame.Characters[i] = i;
                    }
                    Loader.Message = "INITIALIZING ARCHER DATA";
                    Logger.Log("[LOAD] ...Archer Data");
                    ArcherData.Initialize();

                    Loader.Message = "INITIALIZING LEVEL DATA (1/2)";
                    Logger.Log("[LOAD] ...Level Data");
                    GameData.Load();
                    Loader.Message = "INITIALIZING LEVEL DATA (2/2)";

                    Loader.Message = "INITIALIZING MODS";
                    FortRise.RiseCore.Initialize();

                    Loader.Message = "INITIALIZING ARROWS";
                    Arrow.Initialize();
                    patch_Arrow.ExtendArrows();

                    Loader.Message = "INITIALIZING TREASURE";
                    patch_TreasureSpawner.ExtendTreasures();

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
            {
                TaskHelper.RunAsync("dumping assets", RiseCore.ResourceTree.DumpAll);
            }

            Task.Run(CheckUpdate);
            Task.Run(CheckModUpdate);
        }

        private static async Task CheckModUpdate()
        {
            try 
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
                    if (!task.Result.Check(out var _, out string err))
                    {
                        if (err != "NRF")
                        {
                            RiseCore.logger.LogError("Mod Update Error: {error}", err);
                        }
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                RiseCore.logger.LogError("Exception: {error}", e);
            }
        }

        private static async Task CheckUpdate()
        {
            try 
            {
                var result = await RiseCore.UpdateChecks.CheckFortRiseUpdate();
                if (!result.Check(out var res, out string err))
                {
                    RiseCore.UpdateChecks.UpdateMessage = err.ToUpperInvariant();
                    RiseCore.logger.LogError("Update Error: {error}", err);
                    return;
                }

                string url = res.Ref;
                int index = url.LastIndexOf('/');
                if (!SemanticVersion.TryParse(url.AsSpan(index + 1), out SemanticVersion version))
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
            catch (Exception e)
            {
                RiseCore.logger.LogError("Exception: {error}", e);
                RiseCore.UpdateChecks.UpdateMessage = "FAILED TO CHECK UPDATE.";
            }
        }

        [MonoModReplace]
        public static IEnumerator MainMenuLoadWait()
        {
            while (TaskHelper.WaitForAll())
            {
                yield return 0;
            }

            (Atlas as patch_Atlas).ConvertToFastLookup();
            (BGAtlas as patch_Atlas).ConvertToFastLookup();
            (MenuAtlas as patch_Atlas).ConvertToFastLookup();
            (BossAtlas as patch_Atlas).ConvertToFastLookup();

            patch_Sounds.LoadModdedCharacterSounds();
            XNAFileDialog.GraphicsDevice = Instance.GraphicsDevice;

            Loader.Message = "";
            if (SaveData.NewDataCreated && MainMenu.LoadError == null)
            {
                Saver saver = new Saver(true);
                Instance.Scene.Add(saver);
                saver.CanHandleError = true;
                while (!saver.Finished)
                {
                    yield return 0;
                }
            }

            if (ErrorPanel.Errors.Count > 0)
            {
                ErrorPanel.Show = true;
            }
            MainMenu.PlayMenuMusic();
            ConsoleEnabled = SaveData.Instance.Options.DevConsole;
            if (SaveData.Instance.Unlocks.Ascension)
            {
                (Instance.Scene as MainMenu).Background.AscensionTransition();
            }
            Loaded = true;
            ModEventsManager.Instance.OnMenuLoaded.Raise(null, new(Instance.Scene as MainMenu, SaveData.NewDataCreated));
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
                else if (os.Equals("macOS"))
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

        [PatchTFGameOnSceneTransition]
        [MonoModIgnore]
        protected extern override void OnSceneTransition();


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

namespace MonoMod
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchTFGameOnSceneTransition))]
    internal class PatchTFGameOnSceneTransition : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchTFGameOnSceneTransition(ILContext ctx, CustomAttribute attrib) 
        {
            var cursor = new ILCursor(ctx);
            // this useless method call is causing issues for us, we need to remove it

            cursor.GotoNext(instr => instr.MatchCallOrCallvirt("Monocle.Audio", "Stop"));
            cursor.Remove();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FortRise;
using FortRise.Adventure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public partial class patch_MainMenu : MainMenu
    {
        private FortRise.FortModule currentModule;
        private float scrolling;
        private int totalScroll;
        private int count;
        private int scrollAmount = 12;
        private UILoader modLoader;
        private patch_MenuState state;
        private patch_MenuState switchTo;
        public patch_MenuState BackState;



        [MonoModIgnore]
        public patch_MenuState State
        {
            get
            {
                return state;
            }
            set
            {
                switchTo = value;
            }
        }

        [MonoModIgnore]
        public patch_MenuState OldState { get; set;  }

        [MonoModIgnore]
        public MenuItem ToStartSelected { get; private set; }


        public static patch_MatchSettings VersusMatchSettings;

        public patch_MainMenu(MenuState state) : base(state)
        {
        }

        public void CreateModOptions() 
        {
            var list = new List<OptionsButton>();
            var enabledButton = new OptionsButton("ENABLED");
            enabledButton.SetCallbacks(() => {
                enabledButton.State = currentModule.Enabled ? "YES" : "NO";
            }, null, null, () => {
                enabledButton.Selected = false;
                if (!currentModule.SupportModDisabling) 
                {
                    var uiModal = new UIModal();
                    uiModal.SetTitle("Error");
                    uiModal.AddFiller("Does not support disabling mod");
                    uiModal.AutoClose = true;
                    uiModal.AddItem("Ok", () => enabledButton.Selected = true);
                    Add(uiModal);
                    return currentModule.Enabled;
                }
                currentModule.Enabled = !currentModule.Enabled;
                modLoader = new UILoader();
                modLoader.WaitWith(() => {
                    if (!currentModule.Enabled && currentModule.RequiredRestart) 
                    {
                        var uiModal = new UIModal();
                        uiModal.SetTitle("Required Restart");
                        uiModal.AddFiller("This mod required restart to fully unload.");
                        uiModal.AutoClose = true;
                        uiModal.AddItem("Ok", () => enabledButton.Selected = true);
                        Add(uiModal);
                    }
                    else
                        enabledButton.Selected = true;
                });
                Add(modLoader);
                if (currentModule.Enabled) 
                {
                    RiseCore.BlacklistMods(currentModule, false);
                    TaskHelper.Run("mod_register", () => { 
                        currentModule.Register();
                        modLoader.Finished = true;
                    });
                }
                else
                {
                    RiseCore.BlacklistMods(currentModule, true);
                    TaskHelper.Run("mod_unregister", () => { 
                        currentModule.Unregister();
                        modLoader.Finished = true;
                    });
                }

                return currentModule.Enabled;
            });
            list.Add(enabledButton);
            currentModule.CreateSettings(list);
            if (list.Count > 0) 
            {
                InitOptions(list);
                ToStartSelected = list[0];
            }
            BackState = patch_MenuState.Mods;
            TweenUICameraToY(2);
        }

        public void DestroyModOptions() 
        {
            // scrollAmount = 24;
            currentModule = null;
        }

        public void CreateMods() 
        {
            var textContainer = new TextContainer(160);
            foreach (var mod in FortRise.RiseCore.InternalFortModules) 
            {
                var version = mod.Meta.Version.ToString();
                var setupName = mod.Meta.Name + " v" + version;
                string author = mod.Meta.Author ?? "";
                // var modButton = new ModButton(setupName.ToUpperInvariant() + " - " + author.ToUpperInvariant());
                // if (mod is AdventureModule)
                // {
                //     modButton.SetCallbacks(() => { /* Empty */ });
                // }
                // else
                // {
                //     modButton.SetCallbacks(() => {
                //         State = patch_MenuState.ModOptions;
                //         currentModule = mod;
                //     });
                // }
                var modButton = new TextContainer.ButtonText(setupName.ToUpperInvariant() + " - " + author.ToUpperInvariant());
                if (mod is not AdventureModule) 
                {
                    modButton.Pressed(() => {
                        State = patch_MenuState.ModOptions;
                        currentModule = mod;
                    });
                }

                textContainer.Add(modButton);
                // list.Add(modButton);
            }
            textContainer.Selected = true;
            Add(textContainer);

            // var list = new List<OptionsButton>();
            // foreach (var mod in FortRise.RiseCore.InternalFortModules) 
            // {
            //     var version = mod.Meta.Version.ToString();
            //     var setupName = mod.Meta.Name + " v" + version;
            //     string author = mod.Meta.Author ?? "";
            //     var modButton = new ModButton(setupName.ToUpperInvariant() + " - " + author.ToUpperInvariant());
            //     if (mod is AdventureModule)
            //     {
            //         modButton.SetCallbacks(() => { /* Empty */ });
            //     }
            //     else
            //     {
            //         modButton.SetCallbacks(() => {
            //             State = patch_MenuState.ModOptions;
            //             currentModule = mod;
            //         });
            //     }

            //     list.Add(modButton);
            // }
            // if (list.Count > 0) 
            // {
            //     InitMods(list);
            //     ToStartSelected = list[0];
            // }
            BackState = patch_MenuState.Main;
            TweenUICameraToY(1);
        }

        public void DestroyMods() 
        {
            if (switchTo != patch_MenuState.ModOptions)
            {
                SaveOnTransition = true;
                foreach (var mod in FortRise.RiseCore.InternalFortModules) 
                {
                    if (mod.InternalSettings == null)
                        continue;
                    mod.SaveSettings();
                }
            }
        }

        [PatchInitOptions]
        [MonoModIgnore]
        private extern void InitOptions(List<OptionsButton> buttons);


        private void InitMods(List<OptionsButton> buttons)
        {
            this.scrollAmount = 12;
            this.count = buttons.Count;
            for (int i = 0; i < buttons.Count; i++)
            {
                OptionsButton optionsButton = buttons[i];
                optionsButton.TweenTo = new Vector2(320f/2, (float)(45 + i * 12));
                optionsButton.Position = (optionsButton.TweenFrom = new Vector2((float)((i % 2 == 0) ? (-160) : 480), (float)(45 + i * 12)));
                if (i > 0)
                {
                    optionsButton.UpItem = buttons[i - 1];
                }
                if (i < buttons.Count - 1)
                {
                    optionsButton.DownItem = buttons[i + 1];
                }
            }
            base.Add<OptionsButton>(buttons);
        }

        public extern void orig_Update();

        [MonoModReplace]
        [MonoModLinkTo("Monocle.Scene", "System.Void Update()")]
        public void base_Update() 
        {
            base.Update();
        }

        public override void Update()
        {
            if (modLoader != null && !modLoader.Finished)
            {
                base_Update();
                return;
            }
            if (state is patch_MenuState.Mods or patch_MenuState.ModOptions) 
            {
                if (MenuInput.Up && totalScroll > 0) 
                {
                    scrolling += scrollAmount;
                    totalScroll--;
                }
                if (MenuInput.Down && totalScroll < count) 
                {
                    scrolling -= scrollAmount; 
                    totalScroll++;
                }
                if (totalScroll > 9 && totalScroll < count - 5) 
                {
                    foreach (var menuItem in Layers[-1].GetList<MenuItem>()) 
                    {
                        menuItem.Position.Y += scrolling;
                    }
                }
                scrolling = 0;
            }
            else 
            {
                scrolling = 0;
                totalScroll = 0;
            }
            orig_Update();
        }

        [MonoModIgnore]
        private extern void MainOptions();

        [MonoModIgnore]
        private extern void MainQuit();

        [MonoModIgnore]
        private extern void MainCredits();
        [MonoModIgnore]
        private extern void TweenBGCameraToY(int y);

        [MonoModReplace]
        public void CreateMain()
        {
            BladeButton quitButton = null;
            var list = new List<MenuItem>();

            FightButton fightButton = new FightButton(new Vector2(100f, 140f), new Vector2(-160f, 120f));
            list.Add(fightButton);

            CoOpButton coOpButton = new CoOpButton(new Vector2(220f, 140f), new Vector2(480f, 120f));
            list.Add(coOpButton);

            WorkshopButton workshopButton = new WorkshopButton(new Vector2(270f, 210f), new Vector2(270f, 300f));
            list.Add(workshopButton);

            ArchivesButton archivesButton = new ArchivesButton(new Vector2(200f, 210f), new Vector2(200f, 300f));
            list.Add(archivesButton);

            TrialsButton trialsButton = new TrialsButton(new Vector2(130f, 210f), new Vector2(130f, 300f));
            list.Add(trialsButton);

            BladeButton optionsButton;
            patch_BladeButton modsButtons;
            BladeButton creditsButton;
            if (MainMenu.NoQuit)
            {
                modsButtons = new patch_BladeButton(206 - 18f, "MODS", () => State = patch_MenuState.Mods);
                modsButtons.SetX(-50f);

                list.Add(modsButtons);
                optionsButton = new BladeButton(206f, "OPTIONS", this.MainOptions);
                list.Add(optionsButton);
                creditsButton = new BladeButton(224f, "CREDITS", this.MainCredits);
                list.Add(creditsButton);
            }
            else
            {
                modsButtons = new patch_BladeButton(192f - 18f, "MODS", () => State = patch_MenuState.Mods);
                modsButtons.SetX(-50f);
                list.Add(modsButtons);
                optionsButton = new BladeButton(192f, "OPTIONS", this.MainOptions);
                list.Add(optionsButton);
                creditsButton = new BladeButton(210f, "CREDITS", this.MainCredits);
                list.Add(creditsButton);
                quitButton = new BladeButton(228f, "QUIT", this.MainQuit);
                list.Add(quitButton);
            }
            base.Add<MenuItem>(list);
            fightButton.DownItem = modsButtons;
            fightButton.RightItem = coOpButton;
            coOpButton.DownItem = trialsButton;
            coOpButton.LeftItem = fightButton;
            modsButtons.UpItem = fightButton;
            modsButtons.DownItem = optionsButton;
            modsButtons.RightItem = trialsButton;
            optionsButton.UpItem = modsButtons;
            optionsButton.DownItem = creditsButton;
            optionsButton.RightItem = trialsButton;
            creditsButton.UpItem = optionsButton;
            creditsButton.RightItem = trialsButton;
            if (!MainMenu.NoQuit)
            {
                creditsButton.DownItem = quitButton;
                quitButton.UpItem = creditsButton;
                quitButton.RightItem = trialsButton;
            }
            trialsButton.RightItem = archivesButton;
            trialsButton.LeftItem = optionsButton;
            trialsButton.UpItem = coOpButton;
            archivesButton.LeftItem = trialsButton;
            archivesButton.UpItem = coOpButton;
            if (workshopButton != null)
            {
                archivesButton.RightItem = workshopButton;
                coOpButton.DownItem = archivesButton;
                trialsButton.UpItem = fightButton;
                workshopButton.RightItem = null;
                workshopButton.LeftItem = archivesButton;
                workshopButton.UpItem = coOpButton;
            }
            ToStartSelected = OldState switch 
            {
                patch_MenuState.Options => optionsButton,
                patch_MenuState.Archives => archivesButton,
                patch_MenuState.Workshop => workshopButton,
                patch_MenuState.Credits => creditsButton,
                patch_MenuState.Mods => modsButtons,
                patch_MenuState.CoOp when RollcallMode is RollcallModes.Quest or RollcallModes.DarkWorld => coOpButton,
                _ when RollcallMode is RollcallModes.Trials => trialsButton,
                _ => fightButton

            };
            BackState = patch_MenuState.PressStart;
            TweenBGCameraToY(0);
            MainMenu.CurrentMatchSettings = null;
        }

        public extern void orig_Render();
        public override void Render()
        {
            orig_Render();
            if (Loader.Message == "")
                return;
            var tasks = TaskHelper.Tasks;
            float y = 0;

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            foreach (var task in tasks) 
            {
                Draw.OutlineTextJustify(TFGame.Font, task.ToUpperInvariant(), new Vector2(4, 225 - y), Color.Gray, Color.Black, 
                    new Vector2(0, 0.5f));
                y += 10;
            }
            Draw.SpriteBatch.End();
        }

        // TODO better way to do this
        internal static int buttonCount(List<OptionsButton> buttons) 
        {
            return buttons.Count;
        }

        public enum patch_MenuState 
        {
            None,
            Loading,
            PressStart,
            Main,
            Fade,
            Rollcall,
            CoOp,
            VersusOptions,
            Variants,
            TeamSelect,
            Archives,
            Workshop,
            Options,
            Credits,
            ControlOptions,
            KeyboardConfig,
            Mods,
            ModOptions
        }
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchInitOptions))]
    internal class PatchInitOptions: Attribute {}


    internal static partial class MonoModRules 
    {
        public static void PatchInitOptions(ILContext ctx, CustomAttribute attrib) 
        {
            var MainMenu = ctx.Module.GetType("TowerFall.MainMenu");
            var scrollAmount = MainMenu.FindField("scrollAmount");
            var count = MainMenu.FindField("count");
            var buttonCount = MainMenu.FindMethod("System.Int32 buttonCount(System.Collections.Generic.List`1<TowerFall.OptionsButton>)");

            var cursor = new ILCursor(ctx);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldc_I4_S, (sbyte)12);
            cursor.Emit(OpCodes.Stfld, scrollAmount);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Call, buttonCount);
            cursor.Emit(OpCodes.Stfld, count);
        }
    }
}
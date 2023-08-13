using System.Collections.Generic;
using System.IO;
using FortRise;
using FortRise.Adventure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;

namespace TowerFall
{
    public partial class patch_MainMenu : MainMenu
    {
        private FortRise.FortModule currentModule;

        private UILoader modLoader;
        private FortRiseUI currentUI;
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

        private void CreateModToggle() 
        {
            currentUI = new UIModToggler(this);
            currentUI.OnEnter();
        }

        public void CreateModOptions() 
        {
            if (currentModule == null) 
            {
                CreateModToggle();
                BackState = patch_MenuState.Mods;
                TweenUICameraToY(2);
                return;
            }
            var textContainer = new TextContainer(180);
            var enabledButton = new TextContainer.Toggleable("ENABLED", currentModule.Enabled);
            enabledButton.Change(x => {
                enabledButton.Selected = false;
                if (!currentModule.SupportModDisabling) 
                {
                    var uiModal = new UIModal();
                    uiModal.SetTitle("Error");
                    uiModal.AddFiller("Does not support disabling mod");
                    uiModal.AutoClose = true;
                    uiModal.AddItem("Ok", () => enabledButton.Selected = true);
                    Add(uiModal);
                    enabledButton.Value = true;
                    return;
                }
                currentModule.Enabled = x;
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
            });
            textContainer.Add(enabledButton);

            textContainer.Selected = true;
            currentModule.CreateSettings(textContainer);
            Add(textContainer);

            BackState = patch_MenuState.Mods;
            TweenUICameraToY(2);
        }

        public void DestroyModOptions() 
        {
            currentModule = null;
            currentUI?.OnLeave();
            currentUI = null;
        }

        public void CreateMods() 
        {
            var textContainer = new TextContainer(160);
            var toggleButton = new TextContainer.ButtonText("Toggle Mods");
            toggleButton.Pressed(() => {
                State = patch_MenuState.ModOptions;
            });
            textContainer.Add(toggleButton);
            foreach (var mod in FortRise.RiseCore.InternalFortModules) 
            {
                var version = mod.Meta.Version.ToString();
                var setupName = mod.Meta.Name + " v" + version;
                string author = mod.Meta.Author ?? "";
                var modButton = new TextContainer.ButtonText(setupName.ToUpperInvariant() + " - " + author.ToUpperInvariant());
                if (mod is not AdventureModule) 
                {
                    modButton.Pressed(() => {
                        State = patch_MenuState.ModOptions;
                        currentModule = mod;
                    });
                }

                textContainer.Add(modButton);
            }
            textContainer.Selected = true;
            Add(textContainer);

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

        [MonoModIgnore]
        private extern void InitOptions(List<OptionsButton> buttons);


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
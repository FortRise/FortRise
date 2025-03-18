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

        [MonoModIgnore]
        [MonoModConstructor]
        [PatchMainMenuCtor]
        public extern void ctor(MenuState state);

        [MonoModIgnore]
        [PatchMainMenuBegin]
        public extern override void Begin();


        private void CreateModToggle() 
        {
            UIModToggler ui = new UIModToggler(this);
            currentUI = ui;
            ui.OnEnter();
            ToStartSelected = ui.Container;
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

            textContainer.Selected = true;
            if (RiseCore.UpdateChecks.HasUpdates.Contains(currentModule.Meta))
            {
                var updateButton = new TextContainer.ButtonText("UPDATE");
                updateButton.Pressed(() => {
                    UILoader loader = new UILoader();
                    Add(loader);
                    Task.Run(async () => {
                        textContainer.Selected = false;
                        var res = await RiseCore.UpdateChecks.DownloadUpdate(currentModule.Meta);
                        loader.Finish();

                        UIModal modal = new UIModal();
                        modal.AutoClose = true;
                        modal.SetTitle("Update Status");
                        if (!res.Check(out _, out string err))
                        {
                            modal.AddFiller(err);
                            modal.AddItem("Ok", () => textContainer.Selected = true);
                            Add(modal);
                            Logger.Error(err);
                            return;
                        }
                        modal.AddFiller("Restart Required!");
                        modal.AddItem("Ok", () => textContainer.Selected = true);
                        Add(modal);
                    });
                });
                textContainer.Add(updateButton);
            }

            if (currentModule.Meta.Update is not null and { GH: { Repository: not null } })
            {
                var visitGithubButton = new TextContainer.ButtonText("VISIT GITHUB");
                visitGithubButton.Pressed(() => {
                    var repo = currentModule.Meta.Update.GH.Repository;
                    RiseCore.UpdateChecks.OpenGithubURL(repo);
                });

                textContainer.Add(visitGithubButton);
            }


            currentModule.CreateSettings(textContainer);
            Add(textContainer);

            ToStartSelected = textContainer;
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

            if (!string.IsNullOrEmpty(RiseCore.UpdateChecks.UpdateMessage))
            {
                var modButton = new TextContainer.ButtonText("UPDATE FORTRISE");
                modButton.Pressed(RiseCore.UpdateChecks.OpenFortRiseUpdateURL);
                textContainer.Add(modButton);
            }

            foreach (var mod in FortRise.RiseCore.InternalFortModules) 
            {
                var version = mod.Meta.Version.ToString();
                var setupName = mod.Meta.Name + " v" + version.ToUpperInvariant();
                string author = mod.Meta.Author ?? "";

                string title;
                if (string.IsNullOrEmpty(author))
                {
                    title = setupName.ToUpperInvariant();
                }
                else 
                {
                    title = setupName.ToUpperInvariant() + " - " + author.ToUpperInvariant();
                }

                bool hasUpdate = RiseCore.UpdateChecks.HasUpdates.Contains(mod.Meta);

                var modButton = new UIModButtonText(title, hasUpdate);
                if (mod is not AdventureModule or NoModule) 
                {
                    modButton.Pressed(() => {
                        State = patch_MenuState.ModOptions;
                        currentModule = mod;
                    });
                }

                textContainer.Add(modButton);
            }
            Add(textContainer);
            ToStartSelected = textContainer;

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

        [MonoModIgnore]
        [PatchMainMenuCreateOptions]
        private extern void CreateOptions();

        public static void Internal_CreateOptions(List<OptionsButton> buttons) 
        {
            // Future use
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

        public void CreateCoOp()
        {
            CoOpModeButton coopModeButton = new CoOpModeButton(new Vector2(160f, 90), new Vector2(-100f, 90));
            Add(new List<MenuItem>() {coopModeButton});
            ToStartSelected = coopModeButton;

            BackState = patch_MenuState.Main;
            TweenBGCameraToY(1);

            QuestButton questButton = new QuestButton(new Vector2(100f, 90f), new Vector2(-160f, 120f));
			DarkWorldButton darkWorldButton = new DarkWorldButton(new Vector2(220f, 90f), new Vector2(480f, 120f));
            Add(new patch_CoOpDataDisplay(questButton, darkWorldButton, coopModeButton));
        }

        public extern void orig_Render();
        public override void Render()
        {
            orig_Render();
            if (State == patch_MenuState.PressStart)
            {
                if (!string.IsNullOrEmpty(RiseCore.UpdateChecks.UpdateMessage))
                {
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                    Draw.OutlineTextJustify(TFGame.Font, RiseCore.UpdateChecks.UpdateMessage, new Vector2(4, 225), Color.Gray, Color.Black, 
                        new Vector2(0, 0.5f));
                    Draw.SpriteBatch.End();
                }
            }
            else if (State == patch_MenuState.Main && (RiseCore.UpdateChecks.HasUpdates.Count > 0 || RiseCore.UpdateChecks.FortRiseUpdateAvailable))
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                Draw.Texture(TFGame.MenuAtlas["variants/newVariantsTag"], new Vector2(20, (MainMenu.NoQuit ? 208 : 192) - 28), Color.White);
                Draw.SpriteBatch.End();
            }

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
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchMainMenuCtor))]
    public class PatchMainMenuCtor : Attribute {}

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchMainMenuBegin))]
    public class PatchMainMenuBegin : Attribute {}

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchMainMenuCreateOptions))]
    public class PatchMainMenuCreateOptions : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchMainMenuCreateOptions(ILContext ctx, CustomAttribute attrib) 
        {
            var Internal_CreateOptions = ctx.Module.GetType("TowerFall.MainMenu").FindMethod("System.Void Internal_CreateOptions(System.Collections.Generic.List`1<TowerFall.OptionsButton>)");
            var cursor = new ILCursor(ctx);

            cursor.GotoNext(MoveType.After, 
                instr => instr.MatchCallOrCallvirt("System.Collections.Generic.List`1<TowerFall.OptionsButton>", "Add"));

            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, Internal_CreateOptions);
        }

        public static void PatchMainMenuCtor(ILContext ctx, CustomAttribute attrib) 
        {
            var cursor = new ILCursor(ctx);

            // unnecessary bloat of mono
            var numToRemove = IsWindows ? 4 : 11;

            cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("TowerFall.TFGame", "GameLoaded"));
            cursor.RemoveRange(numToRemove);
        }

        public static void PatchMainMenuBegin(ILContext ctx, CustomAttribute attrib) 
        {
            var OnMainBegin = ctx.Module.GetType("FortRise.RiseCore/Events").FindMethod("System.Void Invoke_OnMainBegin(TowerFall.MainMenu)");
            var cursor = new ILCursor(ctx);

            cursor.GotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt("Monocle.Scene", "Begin"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, OnMainBegin);
        }
    }
}
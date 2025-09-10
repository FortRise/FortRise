using System;
using System.Collections.Generic;
using FortRise;
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
        private FortRise.Mod currentModule;

        public FortRise.Mod CurrentModule 
        {
            get => currentModule;
            set => currentModule = value;
        }

        private MenuState state;
        [MonoModPublic]
        public MenuState switchTo;
        public MenuState BackState;



        [MonoModIgnore]
        public MenuState State
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
        public MenuState OldState { get; set; }

        public MenuItem ToStartSelected { get; [MonoModPublic] set; }


        public static patch_MatchSettings VersusMatchSettings;

        public patch_MainMenu(MenuState state) : base(state)
        {
        }

        [MonoModIgnore]
        [MonoModConstructor]
        [PatchMainMenuCtor]
        public extern void ctor(MenuState state);

        [MonoModReplace]
        private void CallStateFunc(string name, MainMenu.MenuState state)
        {
            if (CustomMenuStateRegistry.MenuLoaders.TryGetValue(state, out var loader))
            {
                var menuState = loader(this);
                if (name == "Create")
                {
                    menuState.Create();
                }
                else 
                {
                    menuState.Destroy();
                    CustomMenuStateRegistry.DestroyTypeCache(menuState.GetType());
                }

                return;
            }

            var method = typeof(MainMenu).GetMethod(name + state.ToString());
            if (method != null)
            {
                method.Invoke(this, []);
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
        [MonoModPublic]
        private extern void TweenBGCameraToY(int y);

        [MonoModReplace]
        public void CreateMain()
        {
            BladeButton quitButton = null;
            var list = new List<MenuItem>();

            FightButton fightButton = new FightButton(new Vector2(100f, 140f), new Vector2(-160f, 120f));
            CoOpButton coOpButton = new CoOpButton(new Vector2(220f, 140f), new Vector2(480f, 120f));
            WorkshopButton workshopButton = new WorkshopButton(new Vector2(270f, 210f), new Vector2(270f, 300f));
            ArchivesButton archivesButton = new ArchivesButton(new Vector2(200f, 210f), new Vector2(200f, 300f));
            TrialsButton trialsButton = new TrialsButton(new Vector2(130f, 210f), new Vector2(130f, 300f));

            list.Add(fightButton);
            list.Add(coOpButton);
            list.Add(workshopButton);
            list.Add(archivesButton);
            list.Add(trialsButton);

            BladeButton optionsButton;
            patch_BladeButton modsButtons;
            BladeButton creditsButton;
            if (MainMenu.NoQuit)
            {
                modsButtons = new patch_BladeButton(206 - 18f, "MODS", () => State = ModRegisters.MenuState<UIModMenu>());
                modsButtons.SetX(-50f);

                list.Add(modsButtons);
                optionsButton = new BladeButton(206f, "OPTIONS", this.MainOptions);
                list.Add(optionsButton);
                creditsButton = new BladeButton(224f, "CREDITS", this.MainCredits);
                list.Add(creditsButton);
            }
            else
            {
                modsButtons = new patch_BladeButton(192f - 18f, "MODS", () => State = ModRegisters.MenuState<UIModMenu>());
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
                MenuState.Options => optionsButton,
                MenuState.Archives => archivesButton,
                MenuState.Workshop => workshopButton,
                MenuState.Credits => creditsButton,
                MenuState.CoOp when RollcallMode is RollcallModes.Quest or RollcallModes.DarkWorld => coOpButton,
                _ when RollcallMode is RollcallModes.Trials => trialsButton,
                _ when OldState == ModRegisters.MenuState<UIModMenu>() => modsButtons,
                _ => fightButton

            };
            BackState = MenuState.PressStart;
            TweenBGCameraToY(0);
            MainMenu.CurrentMatchSettings = null;
        }

        public void CreateCoOp()
        {
            CoOpModeButton coopModeButton = new CoOpModeButton(new Vector2(160f, 90), new Vector2(-100f, 90));
            Add(new List<MenuItem>() {coopModeButton});
            ToStartSelected = coopModeButton;

            BackState = MenuState.Main;
            TweenBGCameraToY(1);

            QuestButton questButton = new QuestButton(new Vector2(100f, 90f), new Vector2(-160f, 120f));
			DarkWorldButton darkWorldButton = new DarkWorldButton(new Vector2(220f, 90f), new Vector2(480f, 120f));
            Add(new patch_CoOpDataDisplay(questButton, darkWorldButton, coopModeButton));
        }

        public extern void orig_Render();
        public override void Render()
        {
            orig_Render();
            if (State == MenuState.PressStart)
            {
                if (!string.IsNullOrEmpty(RiseCore.UpdateChecks.UpdateMessage))
                {
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                    Draw.OutlineTextJustify(TFGame.Font, RiseCore.UpdateChecks.UpdateMessage, new Vector2(4, 225), Color.Gray, Color.Black, 
                        new Vector2(0, 0.5f));
                    Draw.SpriteBatch.End();
                }
            }
            else if (State == MenuState.Main && (RiseCore.UpdateChecks.HasUpdates.Count > 0 || RiseCore.UpdateChecks.FortRiseUpdateAvailable))
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
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchMainMenuCtor))]
    public class PatchMainMenuCtor : Attribute {}

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
    }
}

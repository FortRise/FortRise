using System;
using System.Collections.Generic;
using System.Linq;
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

        public string FilterModOptions { get; set; } = string.Empty;



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
        [PatchMainMenuCreateOptions]
        private extern void orig_CreateOptions();

        public void CreateOptions()
        {
            if (string.IsNullOrEmpty(FilterModOptions))
            {
                orig_CreateOptions();
                return;
            }
            var list = new List<OptionsButton>();

            InitOptions(list);
            ToStartSelected = list[1];
            BackState = ModRegisters.MenuState<UIModMenu>();
            TweenBGCameraToY(1);
        }

        public static void Internal_CreateOptions(List<OptionsButton> buttons)
        {
            // Future use
        }

        private void InitModOptions(List<OptionsButton> buttons)
        {
            IEnumerable<Mod> fortModules;
            if (!string.IsNullOrEmpty(FilterModOptions))
            {
                fortModules = RiseCore.ModuleManager.InternalFortModules.Where(x => x.Meta.Name == FilterModOptions);
            }
            else
            {
                fortModules = RiseCore.ModuleManager.InternalFortModules;
            }

            foreach (var mod in fortModules)
            {
                var settings = mod.GetSettings();
                if (settings is null)
                {
                    continue;
                }

                string name;
                if (mod.Meta.DisplayName == null)
                {
                    name = mod.Meta.DisplayName.ToUpperInvariant();
                }
                else
                {
                    name = mod.Meta.Name.ToUpperInvariant();
                }

                OptionsButtonHeader buttonHeader = new OptionsButtonHeader(name);
                buttons.Add(buttonHeader);

                settings.Create(
                    new OptionsCreate(this, buttons)
                );
            }
        }

        [MonoModReplace]
        private void InitOptions(List<OptionsButton> buttons)
        {
            if (State != MenuState.ControlOptions)
            {
                InitModOptions(buttons);
            }

            int num = 0;
            int extraSpacing = 0;
            for (int i = 0; i < buttons.Count; i++)
            {
                OptionsButton optionsButton = buttons[i];
                optionsButton.TweenTo = new Vector2(200f, 45 + extraSpacing + i * 12);
                optionsButton.Position = optionsButton.TweenFrom = new Vector2((i % 2 == 0) ? (-160) : 480, 45 + extraSpacing + i * 12);

                if (optionsButton is not OptionsButtonHeader)
                {
                    int i2 = 1;
                    if (i > 0)
                    {
                        var button = buttons[i - 1];
                        while (button is OptionsButtonHeader)
                        {
                            i2 += 1;
                            if (i - i2 < 0)
                            {
                                break;
                            }
                            button = buttons[i - i2];
                        }

                        optionsButton.UpItem = button;
                    }
                    i2 = 1;

                    if (i < buttons.Count - 1)
                    {
                        var button = buttons[i + i2];
                        while (button is OptionsButtonHeader)
                        {
                            i2 += 1;
                            if (i - i2 > buttons.Count)
                            {
                                break;
                            }
                            button = buttons[i + i2];
                            extraSpacing += 6;
                        }

                        optionsButton.DownItem = button;
                    }
                }

                num += 9 + extraSpacing;
            }
            Add(buttons);
            MaxUICameraY = num;
        }

        [MonoModIgnore]
        private extern void MainOptions();

        [MonoModIgnore]
        private extern void MainQuit();

        [MonoModIgnore]
        private extern void MainCredits();
        [MonoModIgnore]
        [MonoModPublic]
        public extern void TweenBGCameraToY(int y);

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
            Add(new List<MenuItem>() { coopModeButton });
            ToStartSelected = coopModeButton;

            BackState = MenuState.Main;
            TweenBGCameraToY(1);

            QuestButton questButton = new QuestButton(new Vector2(100f, 90f), new Vector2(-160f, 120f));
            DarkWorldButton darkWorldButton = new DarkWorldButton(new Vector2(220f, 90f), new Vector2(480f, 120f));
            Add(new patch_CoOpDataDisplay(questButton, darkWorldButton, coopModeButton));
        }

        [MonoModReplace]
        public void DestroyOptions()
        {
            if (switchTo == MenuState.ControlOptions)
            {
                return;
            }

            SaveOnTransition = true;
            foreach (var mod in RiseCore.ModuleManager.InternalFortModules)
            {
                mod.SaveSettings();
            }
        }

        public extern void orig_Render();
        public override void Render()
        {
            orig_Render();
            if (State == MenuState.PressStart)
            {
                if (!string.IsNullOrEmpty(RiseCore.UpdateChecks.UpdateMessage))
                {
                    Draw.SpriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        DepthStencilState.None,
                        RasterizerState.CullNone
                    );

                    Draw.OutlineTextJustify(TFGame.Font, RiseCore.UpdateChecks.UpdateMessage, new Vector2(4, 225), Color.Gray, Color.Black, 
                        new Vector2(0, 0.5f));

                    Draw.SpriteBatch.End();
                }
            }
            else if (State == MenuState.Main && (RiseCore.UpdateChecks.HasUpdates.Count > 0 || RiseCore.UpdateChecks.FortRiseUpdateAvailable))
            {
                Draw.SpriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone
                );

                Draw.Texture(TFGame.MenuAtlas["variants/newVariantsTag"], new Vector2(20, (MainMenu.NoQuit ? 208 : 192) - 28), Color.White);

                Draw.SpriteBatch.End();
            }
        }

        [MonoModReplace]
        public void CreateArchives()
        {
            var archivesController = new ArchivesController();
            var archivesStatsPage = new ArchivesStatsPage();
            var archivesSessionPage = new ArchivesSessionPage();
            var archivesDeathsPage = new ArchivesDeathsPage();
            var archivesAwardsPage = new ArchivesAwardsPage();
            var archivesQuestPage = new ArchivesQuestPage();
            var archivesDarkWorldPage = new ArchivesDarkWorldPage();
            var archivesTrialsPage = new ArchivesTrialsPage();
            var archivesTipsPage = new ArchivesTipsPage();
            var archivesAscensionPage = new ArchivesAscensionPage();
            var archivesGlobalStatsPage = new ArchivesGlobalStatsPage();

            archivesController.SetPages([
                archivesStatsPage, archivesSessionPage, archivesDeathsPage, archivesAwardsPage, 
                archivesQuestPage, archivesDarkWorldPage, archivesTrialsPage, archivesGlobalStatsPage, 
                archivesTipsPage, archivesAscensionPage]);

            MenuItem[] items = [
                archivesController, archivesStatsPage, archivesSessionPage, archivesDeathsPage, 
                archivesAwardsPage, archivesGlobalStatsPage, archivesQuestPage, archivesDarkWorldPage, 
                archivesTrialsPage, archivesTipsPage,
                archivesAscensionPage
            ];

            Add(items);

            if (!string.IsNullOrEmpty(FortRiseModule.Settings.MusicEnableArchives))
            {
                try
                {
                    Music.Play(FortRiseModule.Settings.MusicEnableArchives);
                }
                catch {}
            }
            else
            {
                Music.Play("TheArchives");
            }

            ToStartSelected = null;
            BackState = MenuState.Main;
            TweenBGCameraToY(1);
        }

        [MonoModReplace]
        public static void PlayMenuMusic(bool fromArchives = false, bool forceNormal = false)
        {
            if (!string.IsNullOrEmpty(FortRiseModule.Settings.MusicEnableMainMenu))
            {
                try
                {
                    Music.Play(FortRiseModule.Settings.MusicEnableMainMenu);
                    return;
                }
                catch 
                {
                    // No-op
                }
            }

            if (Music.CurrentSong == "AltTitle" || Music.CurrentSong == "ChipTitle" || Music.CurrentSong == "Title")
            {
                return;
            }

            if (SaveData.Instance.Unlocks.Ascension)
            {
                if (forceNormal)
                {
                    Music.Play("AltTitle");
                    wasChipTheme = false;
                }
                else if (fromArchives)
                {
                    if (wasChipTheme)
                    {
                        Music.Play("ChipTitle");
                    }
                    else
                    {
                        Music.Play("AltTitle");
                    }
                }
                else if (Calc.Random.Chance(0.01f))
                {
                    Music.Play("ChipTitle");
                    wasChipTheme = true;
                }
                else
                {
                    Music.Play("AltTitle");
                    wasChipTheme = false;
                }
            }
            else
            {
                Music.Play("Title");
            }
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
            var InterceptOptionsStart = ctx.Module.GetType("TowerFall.MainMenu").FindMethod("System.Void ChangeOptionsStart(System.Collections.Generic.List`1<TowerFall.OptionsButton>)");
            var cursor = new ILCursor(ctx);

            cursor.GotoNext(MoveType.After, 
                instr => instr.MatchCallOrCallvirt("System.Collections.Generic.List`1<TowerFall.OptionsButton>", "Add"));

            if (IsWindows)
            {
                cursor.Emit(OpCodes.Ldloc_1);               
            }
            else
            {
                cursor.Emit(OpCodes.Ldloc_0);
            }
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

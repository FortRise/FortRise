using System;
using FortRise;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public class patch_PauseMenu : PauseMenu
    {
        private MenuType menuType;
        private Level level;
        private Action backAction;
        private Action oldBackAction;
        private bool settingsOpened;
        private float oldMusicVolume;


        public patch_PauseMenu(Level level, Vector2 position, MenuType menuType, int controllerDisconnected = -1) : base(level, position, menuType, controllerDisconnected)
        {
        }

        [MonoModIgnore]
        [MonoModConstructor]
        [PatchPauseMenuCtor]
        public extern void ctor(Level level, Vector2 position, MenuType menuType, int controllerDisconnected = -1);


        [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
        [MonoModIgnore]
        public void base_Update() 
        {
            base.Update();
        }

        public extern void orig_Update();

        public override void Update()
        {
            if (settingsOpened) 
            {
                MenuInput.Update();
                if (MenuInput.Back && this.backAction != null)
                {
                    this.backAction();
                }
                base_Update();
                return;
            }
            orig_Update();
        }

        [MonoModReplace]
        private void DarkWorldRestart() 
        {
            (level.Session as patch_Session).DisableTempVariants(level);
            Sounds.ui_click.Play(160f, 1f);
            var oldLevelSet = this.level.Session.GetLevelSet();
            var session = new Session(this.level.Session.MatchSettings);
            session.SetLevelSet(oldLevelSet); 
            session.StartGame();
        }

        [MonoModReplace]
        private void DarkWorldMap() 
        {
            (level.Session as patch_Session).DisableTempVariants(level);
            Sounds.ui_click.Play(160f, 1f);
            var mapScene = new MapScene(MainMenu.RollcallModes.DarkWorld);
            Engine.Instance.Scene = mapScene;
            mapScene.SetLevelSet(level.Session.GetLevelSet());
            level.Session.MatchSettings.LevelSystem.Dispose();
            ModEventsManager.Instance.OnSessionQuit.Raise(this, new(level.Session, menuType));
        }

        [MonoModReplace]
        private void DarkWorldMapAndSave() 
        {
            (level.Session as patch_Session).DisableTempVariants(level);
            Sounds.ui_click.Play(160f, 1f);
            MapScene mapScene = new MapScene(MainMenu.RollcallModes.DarkWorld);
            mapScene.ShouldSave = true;
            Engine.Instance.Scene = mapScene;
            mapScene.SetLevelSet(level.Session.GetLevelSet());
            level.Session.MatchSettings.LevelSystem.Dispose();
            ModEventsManager.Instance.OnSessionQuit.Raise(this, new(level.Session, menuType));
        }

        [MonoModReplace]
        private void QuestMap()
        {
            Sounds.ui_click.Play(160f, 1f);
            MapScene mapScene = new MapScene(MainMenu.RollcallModes.Quest);
            Engine.Instance.Scene = mapScene;
            mapScene.SetLevelSet(level.Session.GetLevelSet());
            level.Session.MatchSettings.LevelSystem.Dispose();
            ModEventsManager.Instance.OnSessionQuit.Raise(this, new(level.Session, menuType));
        }

        
        [MonoModReplace]
        private void QuestMapAndSave()
        {
            Sounds.ui_click.Play(160f, 1f);
            MapScene mapScene = new MapScene(MainMenu.RollcallModes.Quest);
            mapScene.ShouldSave = true;
            Engine.Instance.Scene = mapScene;
            mapScene.SetLevelSet(level.Session.GetLevelSet());
            level.Session.MatchSettings.LevelSystem.Dispose();
            ModEventsManager.Instance.OnSessionQuit.Raise(this, new(level.Session, menuType));
        }

        [MonoModReplace]
        private void Quit()
        {
            (level.Session as patch_Session).DisableTempVariants(level);
            Sounds.ui_clickBack.Play(160f, 1f);
            for (int i = 0; i < 4; i += 1)
            {
                TFGame.Players[i] = false;
            }

            Engine.Instance.Scene = new MainMenu(MainMenu.MenuState.Main);
            level.Session.MatchSettings.LevelSystem.Dispose();
            ModEventsManager.Instance.OnSessionQuit.Raise(this, new(level.Session, menuType));
        }

        [MonoModReplace]
        private void QuitAndSave()
        {
            (level.Session as patch_Session).DisableTempVariants(level);
            Sounds.ui_clickBack.Play(160f, 1f);
            for (int i = 0; i < 4; i += 1)
            {
                TFGame.Players[i] = false;
            }

            MainMenu mainMenu = new MainMenu(MainMenu.MenuState.Main);
            mainMenu.SaveOnTransition = true;
            Engine.Instance.Scene = mainMenu;
            level.Session.MatchSettings.LevelSystem.Dispose();
            ModEventsManager.Instance.OnSessionQuit.Raise(this, new(level.Session, menuType));
        }


        [MonoModReplace]
        private void VersusRematch() 
        {
            Sounds.ui_click.Play(160f, 1f);
            MapScene mapScene = new MapScene(MainMenu.RollcallModes.Versus);
            Engine.Instance.Scene = mapScene;
            mapScene.SetLevelSet(level.Session.GetLevelSet());
            level.Session.MatchSettings.LevelSystem.Dispose();
            ModEventsManager.Instance.OnSessionQuit.Raise(this, new(level.Session, menuType));
        }

        [MonoModReplace]
        private void VersusArcherSelect()
        {
            Sounds.ui_clickBack.Play(160f, 1f);
            for (int i = 0; i < 4; i += 1)
            {
                TFGame.Players[i] = false;
            }
            Engine.Instance.Scene = new MainMenu(MainMenu.MenuState.Rollcall);
            level.Session.MatchSettings.LevelSystem.Dispose();
            ModEventsManager.Instance.OnSessionQuit.Raise(this, new(level.Session, menuType));
        }

        [MonoModReplace]
        private void VersusMatchSettings()
        {
            Sounds.ui_clickBack.Play(160f, 1f);
            Engine.Instance.Scene = new MainMenu(MainMenu.MenuState.VersusOptions);
            level.Session.MatchSettings.LevelSystem.Dispose();
            ModEventsManager.Instance.OnSessionQuit.Raise(this, new(level.Session, menuType));
        }

        [MonoModReplace]
        private void VersusMatchSettingsAndSave()
        {
            Sounds.ui_clickBack.Play(160f, 1f);
            MainMenu mainMenu = new MainMenu(MainMenu.MenuState.VersusOptions);
            mainMenu.SaveOnTransition = true;
            Engine.Instance.Scene = mainMenu;
            level.Session.MatchSettings.LevelSystem.Dispose();
            ModEventsManager.Instance.OnSessionQuit.Raise(this, new(level.Session, menuType));
        }

        private void Settings() 
        {
            settingsOpened = true;
            Sounds.ui_click.Play(160f, 1f);
            var container = new TextContainer(180);
            container.LayerIndex = 4;
            container.WithFade = true;
            var fullscreen = new TextContainer.Toggleable("Fullscreen", SaveData.Instance.Options.Fullscreen);
            fullscreen.Change(value => {
                SaveData.Instance.Options.Fullscreen = value;
                if (SaveData.Instance.Options.Fullscreen)
                {
                    Engine.Instance.Screen.EnableFullscreen(Screen.FullscreenMode.LargestScale);
                    return;
                }
                Engine.Instance.Screen.DisableFullscreen(3f);
            });

            var music = new TextContainer.Number("Music", (int)Math.Round((double)(oldMusicVolume * 10f)), 0, 10);
            music.Change(val => {
                oldMusicVolume = (float)val / 10;
                Music.MasterVolume = (float)val / 10 * 0.5f;
                SaveData.Instance.Options.MusicVolume = oldMusicVolume;
            });

            var sfx = new TextContainer.Number("Sounds", Audio.MasterVolumeInt, 0, 10);
            sfx.Change(val => {
                Audio.MasterVolumeInt = val;
                SaveData.Instance.Options.SFXVolume = Audio.MasterVolume;
            });

            var versusLesson = new TextContainer.Toggleable("Show Versus Lessons", SaveData.Instance.Options.ShowTips);
            versusLesson.Change(val => {
                SaveData.Instance.Options.ShowTips = val;
            });

            var replaySkippable = new TextContainer.Toggleable("Replay Skippable", SaveData.Instance.Options.CanSkipReplays);
            replaySkippable.Change(val => {
                SaveData.Instance.Options.CanSkipReplays = val;
            });

            var sbDuringReplay = new TextContainer.Toggleable("Show Buttons during replays", SaveData.Instance.Options.ShowInputDuringReplays);
            sbDuringReplay.Change(val => {
                SaveData.Instance.Options.ShowInputDuringReplays = val;
            });

            var removeSlowMotionWaveEffect = new TextContainer.Toggleable("Screen wave effects", SaveData.Instance.Options.RemoveSlowMotionWaveEffect);
            removeSlowMotionWaveEffect.Change(val => {
                SaveData.Instance.Options.RemoveSlowMotionWaveEffect = val;
            });
            var removeFlashEffects = new TextContainer.Toggleable("Screen flash effects", SaveData.Instance.Options.RemoveScreenFlashEffects);
            removeFlashEffects.Change(val => {
                SaveData.Instance.Options.RemoveScreenFlashEffects = val;
            });
            var removeScrollEffects = new TextContainer.Toggleable("Screen scroll effects", SaveData.Instance.Options.RemoveScrollEffects);
            removeScrollEffects.Change(val =>
            {
                SaveData.Instance.Options.RemoveScrollEffects = val;
                if (SaveData.Instance.Options.RemoveScrollEffects)
                {
                    MainMenu.VersusMatchSettings.Variants.DisableScrollEffects();
                }
            });

            var holdToPause = new TextContainer.Toggleable("Hold to pause", SaveData.Instance.Options.HoldToPause);
            removeScrollEffects.Change(val =>
            {
                SaveData.Instance.Options.HoldToPause = val;
            });
            var gamepadDiscovery = new TextContainer.Toggleable("Gamepad discovery", SaveData.Instance.Options.GamepadDiscovery);
            gamepadDiscovery.Change(val =>
            {
                SaveData.Instance.Options.GamepadDiscovery = val;
            });
            var gamepadVibration = new TextContainer.Toggleable("Gamepad rumble", SaveData.Instance.Options.GamepadVibration);
            gamepadVibration.Change(val =>
            {
                SaveData.Instance.Options.GamepadVibration = val;
            });

            container.Add(fullscreen);
            container.Add(music);
            container.Add(sfx);
            container.Add(versusLesson);
            container.Add(replaySkippable);
            container.Add(sbDuringReplay);
            container.Add(removeSlowMotionWaveEffect);
            container.Add(removeFlashEffects);
            container.Add(removeScrollEffects);
            container.Add(holdToPause);
            container.Add(gamepadDiscovery);
            container.Add(gamepadVibration);

            Scene.Add(container);
            container.TweenIn();
            container.Selected = true;
            Visible = false;
            oldBackAction = backAction;
            backAction = () => {
                container.RemoveSelf();
                Sounds.ui_pause.Play();
                Visible = true;
                backAction = oldBackAction;
                oldBackAction = null;
                settingsOpened = false;
            };
        }

        private void addSettings() 
        {
            AddItem("SETTINGS", Settings);
        }

        [MonoModIgnore]
        private extern void AddItem(string name, Action action);

        [MonoModReplace]
        private void TrialsNextLevel()
		{
            RoundLogic.Restarted = false;
            Sounds.ui_clickBack.Play(160f, 1f);
            var trialsData = (level.Session.MatchSettings.LevelSystem as TrialsLevelSystem).TrialsLevelData;
			level.Session.MatchSettings.LevelSystem.Dispose();
            if (trialsData.IsOfficialLevelSet()) 
            {
                Point id = this.level.Session.MatchSettings.LevelSystem.ID;
                this.level.Session.MatchSettings.LevelSystem = GameData.TrialsLevels[id.X, id.Y + 1].GetLevelSystem();
            }
            else 
            {
                level.Session.MatchSettings.LevelSystem = TowerRegistry
                    .TrialsGet(trialsData.GetLevelSet(), trialsData.ID.X)[trialsData.ID.Y + 1]
                    .GetLevelSystem();
            }

            new Session(this.level.Session.MatchSettings).StartGame();
		}
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchPauseMenuCtor))]
    internal class PatchPauseMenuCtor : Attribute {}

    internal static partial class MonoModRules 
    { 
        public static void PatchPauseMenuCtor(ILContext ctx, CustomAttribute attrib) 
        {
            var addSettings = ctx.Module.GetType("TowerFall.PauseMenu").FindMethod("System.Void addSettings()");
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchLdftn("TowerFall.PauseMenu", "Resume"),
                instr => instr.MatchNewobj("System.Action"),
                instr => instr.MatchCallOrCallvirt("TowerFall.PauseMenu", "AddItem"))) 
            {
                if (cursor.Next.Next.MatchLdstr("RESTART"))
                {
                    cursor.GotoNext(MoveType.After, 
                        instr => instr.MatchLdftn("TowerFall.PauseMenu", "QuestRestart"),
                        instr => instr.MatchNewobj("System.Action"),
                        instr => instr.MatchCallOrCallvirt("TowerFall.PauseMenu", "AddItem"));
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Call, addSettings);
                }
                else if (cursor.Next.MatchLdarg(1)) 
                {
                    cursor.GotoNext(MoveType.After, 
                        instr => instr.MatchLdftn("TowerFall.PauseMenu", "DarkWorldRestart"),
                        instr => instr.MatchNewobj("System.Action"),
                        instr => instr.MatchCallOrCallvirt("TowerFall.PauseMenu", "AddItem"));
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Call, addSettings);
                }
                else 
                {
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Call, addSettings);
                }
            }
        }
    }
}

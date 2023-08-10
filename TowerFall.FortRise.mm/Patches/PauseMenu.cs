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
        private PauseMenu.MenuType menuType;
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
        [PatchPauseMenuQuestRestart]
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
            Sounds.ui_click.Play(160f, 1f);
            var oldLevelSet = this.level.Session.GetLevelSet();
            var session = new Session(this.level.Session.MatchSettings);
            session.SetLevelSet(oldLevelSet); 
            session.StartGame();
        }

        [MonoModReplace]
        private void DarkWorldMap() 
        {
            patch_DarkWorldControl.DisableTempVariants(level);
            Sounds.ui_click.Play(160f, 1f);
            var mapScene = new MapScene(MainMenu.RollcallModes.DarkWorld);
            Engine.Instance.Scene = mapScene;
            mapScene.SetLevelSet(level.Session.GetLevelSet());
            this.level.Session.MatchSettings.LevelSystem.Dispose();
        }

        [MonoModReplace]
        private void DarkWorldMapAndSave() 
        {
            patch_DarkWorldControl.DisableTempVariants(level);
            Sounds.ui_click.Play(160f, 1f);
            MapScene mapScene = new MapScene(MainMenu.RollcallModes.DarkWorld);
            mapScene.ShouldSave = true;
            Engine.Instance.Scene = mapScene;
            mapScene.SetLevelSet(level.Session.GetLevelSet());
            this.level.Session.MatchSettings.LevelSystem.Dispose();
        }

        [MonoModReplace]
        private void QuestMap()
        {
            Sounds.ui_click.Play(160f, 1f);
            MapScene mapScene = new MapScene(MainMenu.RollcallModes.Quest);
            Engine.Instance.Scene = mapScene;
            mapScene.SetLevelSet(level.Session.GetLevelSet());
            this.level.Session.MatchSettings.LevelSystem.Dispose();
        }

        
        [MonoModReplace]
        private void QuestMapAndSave()
        {
            Sounds.ui_click.Play(160f, 1f);
            MapScene mapScene = new MapScene(MainMenu.RollcallModes.Quest);
            mapScene.ShouldSave = true;
            Engine.Instance.Scene = mapScene;
            mapScene.SetLevelSet(level.Session.GetLevelSet());
            this.level.Session.MatchSettings.LevelSystem.Dispose();
        }

        private extern void orig_Quit();

        private void Quit() 
        {
            patch_DarkWorldControl.DisableTempVariants(level);
            orig_Quit();
        }

        private extern void orig_QuitAndSave();

        public void QuitAndSave() 
        {
            patch_DarkWorldControl.DisableTempVariants(level);
            orig_QuitAndSave();
        }

        [MonoModReplace]
        private void VersusRematch() 
        {
            Sounds.ui_click.Play(160f, 1f);
            MapScene mapScene = new MapScene(MainMenu.RollcallModes.Versus);
            Engine.Instance.Scene = mapScene;
            mapScene.SetLevelSet(level.Session.GetLevelSet());
            this.level.Session.MatchSettings.LevelSystem.Dispose();
        }

        private void Settings() 
        {
            settingsOpened = true;
            Sounds.ui_click.Play(160f, 1f);
            var container = new TextContainer(180);
            container.LayerIndex = 4;
            container.FadeBlack = true;
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
                Music.MasterVolume = ((float)val / 10) * 0.5f;
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

            container.Add(fullscreen);
            container.Add(music);
            container.Add(sfx);
            container.Add(versusLesson);
            container.Add(replaySkippable);
            container.Add(sbDuringReplay);
            container.Add(removeSlowMotionWaveEffect);
            container.Add(removeFlashEffects);
            container.Add(removeScrollEffects);

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
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchPauseMenuQuestRestart))]
    internal class PatchPauseMenuQuestRestart : Attribute {}

    internal static partial class MonoModRules 
    { 
        public static void PatchPauseMenuQuestRestart(ILContext ctx, CustomAttribute attrib) 
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
                else 
                {
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Call, addSettings);
                }
            }
        }
    }
}
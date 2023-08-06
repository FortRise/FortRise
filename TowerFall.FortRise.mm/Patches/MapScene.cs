using System;
using System.Collections;
using System.Collections.Generic;
using FortRise;
using FortRise.Adventure;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public class patch_MapScene : MapScene
    {
        private static int lastRandomVersusTower;
        private bool adventureLevels;
        private float crashDelay;
        private Counter counterDelay;
        private CustomMapRenderer currentCustomMapRenderer;
        public AdventureType CurrentAdventureType;
        public bool MapPaused;
        public int CustomLevelCategory;
        public string LevelSet;

        public CustomMapRenderer CurrentMapRender 
        {
            get => currentCustomMapRenderer;
            set => currentCustomMapRenderer = value;
        }
        public patch_MapScene(MainMenu.RollcallModes mode) : base(mode)
        {
        }

        private void InitializeCustoms() 
        {
            var counterHolder = new Entity();
            counterDelay = new Counter();
            counterHolder.Add(counterDelay);
            Add(counterHolder);
            CustomLevelCategory = -1;
            crashDelay = 10;
            foreach (var (contaning, mapRenderer) in patch_GameData.AdventureWorldMapRenderer) 
            {
                var entity = new Entity(-1);
                if (!contaning) 
                    continue;
                entity.Add(mapRenderer);
                Add(entity);
                mapRenderer.Visible = false;
            }
        }

        [MonoModConstructor]
        [MonoModReplace]
        public static void cctor() {}

        internal static void FixedStatic() 
        {
            lastRandomVersusTower = -1;
            MapScene.NoRandomStates = new bool[GameData.VersusTowers.Count];
        }

        [MonoModIgnore]
        [PatchMapSceneBegin]
        [PreFixing("TowerFall.MapScene", "System.Void InitializeCustoms()")]
        public extern void orig_Begin();

        public override void Begin() 
        {
            orig_Begin();
            if (!this.IsOfficialLevelSet()) 
            {
                var entity = new Entity();
                var tween = Tween.Create(Tween.TweenMode.Oneshot, null, 10);
                entity.Add(tween);
                tween.OnComplete = t => {
                    var startingID = CurrentAdventureType switch 
                    {
                        AdventureType.Quest => MainMenu.QuestMatchSettings.LevelSystem.ID.X,
                        AdventureType.DarkWorld => MainMenu.DarkWorldMatchSettings.LevelSystem.ID.X,
                        _ => 0
                    };
                    GotoAdventure(CurrentAdventureType, startingID + 1);
                    entity.RemoveSelf();
                };
                Add(entity);
                tween.Start();
            }
        }


        [MonoModReplace]
        private void StartSession() 
        {
            var session = new Session(MainMenu.CurrentMatchSettings);
            session.SetLevelSet(LevelSet);
            session.StartGame();
        }

        [MonoModReplace]
        private IEnumerator DarkWorldIntroSequence() 
        {
            int num = 0;
            for (int i = 0; i < Buttons.Count; i = num + 1) 
            {
                if (Buttons[i] is not DarkWorldMapButton)
                   continue;
                if (SaveData.Instance.DarkWorld.ShouldRevealTower(Buttons[i].Data.ID.X)) 
                {
                    Music.Stop();
                    yield return Buttons[i].UnlockSequence(false);
                }
                num = i;
            }
            yield break;
        }

        [MonoModReplace]
        private IEnumerator QuestIntroSequence()
        {
            int num = 0;
            for (int i = 0; i < this.Buttons.Count; i = num + 1)
            {
                if (Buttons[i] is not QuestMapButton)
                   continue;
                if (SaveData.Instance.Quest.ShouldRevealTower(this.Buttons[i].Data.ID.X))
                {
                    Music.Stop();
                    yield return this.Buttons[i].UnlockSequence(true);
                }
                num = i;
            }
            yield break;
        }

        

        private void InitAdventureMap(AdventureType adventureType) 
        {
            CurrentAdventureType = adventureType;
            Buttons.Add(new AdventureCategoryButton(adventureType));
        }

        private void InitAdventureMap(List<MapButton[]> list) 
        {
            var gotoAdventure = new GotoAdventureButton(CurrentAdventureType);
            Buttons.Add(gotoAdventure);
            list.Add(new MapButton[] { gotoAdventure, gotoAdventure, gotoAdventure });
        }

        public void InitAdventure(int id) 
        {
            counterDelay.Set(20);
            Add(new AdventureListLoader(this, id));
        }

        public void GotoAdventure(AdventureType type, int id = 0) 
        {
            adventureLevels = true;
            WorkshopLevels = true;
            TweenOutAllButtonsAndRemove();
            Buttons.Clear();
            InitAdventure(id);
        }

        public extern void orig_ExitWorkshop();

        public void ExitWorkshop() 
        {
            if (adventureLevels)
                ExitAdventure();
            else
                orig_ExitWorkshop();
        }

        public void ExitAdventure(int id = 1) 
        {
            adventureLevels = false;
            WorkshopLevels = false;
            TweenOutAllButtonsAndRemove();
            Buttons.Clear();
            Buttons.Add(new AdventureCategoryButton(CurrentAdventureType));
            switch (CurrentAdventureType) 
            {
            case AdventureType.Quest:
                for (int i = 0; i < GameData.QuestLevels.Length; i++)
                {
                    if (SaveData.Instance.Unlocks.GetQuestTowerUnlocked(i))
                    {
                        this.Buttons.Add(new QuestMapButton(GameData.QuestLevels[i]));
                    }
                }
                break;
            case AdventureType.DarkWorld:
                for (int j = 0; j < GameData.DarkWorldTowers.Count; j++)
                {
                    if (SaveData.Instance.Unlocks.GetDarkWorldTowerUnlocked(j))
                    {
                        Buttons.Add(new DarkWorldMapButton(GameData.DarkWorldTowers[j]));
                    }
                }
                break;
            }

            this.LinkButtonsList();
            if (id >= Buttons.Count)
                id = Buttons.Count - 1;
            InitButtons(Buttons[id]);
            foreach (var button in Buttons)
                Add(button);
            ScrollToButton(Selection);
        }

        [MonoModLinkTo("Monocle.Scene", "System.Void Update()")]
        [MonoModIgnore]
        public void base_Update() 
        {
            base.Update();
        }

        private extern void orig_Update();

        public override void Update()
        {
            if (MapPaused) 
            {
                base_Update();
                return;
            }
            if (!ScrollMode && !MatchStarting && Mode == MainMenu.RollcallModes.DarkWorld && crashDelay <= 0) 
            {
                // if (MenuInput.Alt2 && Selection is AdventureMapButton button)
                // {
                //     var id = Selection.Data.ID.X;
                //     var level = patch_GameData.AdventureWorldTowers[id];
                //     if (AdventureModule.SaveData.LevelLocations.Contains(level.StoredDirectory)) 
                //     {
                //         Add(new DeleteMenu(this, id));
                //         MapPaused = true;
                //         return;
                //     }
                //     button.Shake();
                //     MenuInput.RumblePlayers(1f, 20);
                // }
                // if (MenuInput.Up && !counterDelay) 
                // {
                //     if (CustomLevelCategory != patch_GameData.AdventureWorldModTowers.Count - 1) 
                //     {
                //         CustomLevelCategory++;
                //         var id = Buttons.IndexOf(Selection);
                //         GotoAdventure(CurrentAdventureType, id);
                //         var customMapRenderer = patch_GameData.AdventureWorldMapRenderer[CustomLevelCategory];
                //         if (customMapRenderer.contains) 
                //         {
                //             if (currentCustomMapRenderer != null)
                //                 currentCustomMapRenderer.Visible = false;
                //             currentCustomMapRenderer = customMapRenderer.renderer;
                //             Renderer.Visible = false;
                //             currentCustomMapRenderer.Visible = true;
                //             if (Selection.Data == null) 
                //             {
                //                 currentCustomMapRenderer.OnSelectionChange("");
                //                 return;
                //             }
                //             currentCustomMapRenderer.OnSelectionChange(Selection.Data.Title);
                //         }
                //         else 
                //         {
                //             if (currentCustomMapRenderer != null)
                //                 currentCustomMapRenderer.Visible = false;
                //             Renderer.Visible = true;
                //             currentCustomMapRenderer = null;
                //             if (Selection.Data == null)
                //             {
                //                 Renderer.OnSelectionChange("");
                //                 return;
                //             }
                //             Renderer.OnSelectionChange(Selection.Data.Title);
                //         }
                //     }
                // }
                // else if (MenuInput.Down && !counterDelay && patch_SaveData.AdventureActive) 
                // {
                //     if (CustomLevelCategory != -1)
                //         CustomLevelCategory--;

                //     var id = Buttons.IndexOf(Selection);
                //     if (CustomLevelCategory == -1) 
                //     {
                //         if (currentCustomMapRenderer != null)
                //             currentCustomMapRenderer.Visible = false;
                //         ExitAdventure(id);
                //         Renderer.Visible = true;
                //         currentCustomMapRenderer = null;

                //         if (Selection.Data == null)
                //         {
                //             Renderer.OnSelectionChange("");
                //             return;
                //         }
                //     }
                //     else 
                //     {
                //         GotoAdventure(CurrentAdventureType, id);
                //         var customMapRenderer = patch_GameData.AdventureWorldMapRenderer[CustomLevelCategory];
                //         if (customMapRenderer.contains) 
                //         {
                //             if (currentCustomMapRenderer != null)
                //                 currentCustomMapRenderer.Visible = false;
                //             currentCustomMapRenderer = customMapRenderer.renderer;
                //             Renderer.Visible = false;
                //             currentCustomMapRenderer.Visible = true;
                //             if (Selection.Data == null) 
                //             {
                //                 customMapRenderer.renderer.OnSelectionChange("");
                //             }
                //             else
                //                 customMapRenderer.renderer.OnSelectionChange(Selection.Data.Title);
                //             return;
                //         }
                //         else 
                //         {
                //             if (currentCustomMapRenderer != null)
                //                 currentCustomMapRenderer.Visible = false;
                //             Renderer.Visible = true;
                //             currentCustomMapRenderer = null;
                //             if (Selection.Data == null)
                //             {
                //                 Renderer.OnSelectionChange("");
                //                 return;
                //             }
                //         }
                //     }
                //     Renderer.OnSelectionChange(Selection.Data.Title);
                // }
                if (MenuInput.Back) 
                {
                    if (currentCustomMapRenderer != null)
                        currentCustomMapRenderer.Visible = false;
                    Renderer.Visible = true;
                    currentCustomMapRenderer = null;
                    CustomLevelCategory = -1;
                }

                // if (patch_SaveData.AdventureActive && MInput.Keyboard.Pressed(Keys.F5) && !counterDelay) 
                // {
                //     var id = Buttons.IndexOf(Selection);
                //     var loading = new UILoader();
                //     loading.LayerIndex = 0;
                //     loading.WaitWith(() => {
                //         MapPaused = false;
                //     });
                //     Add(loading);
                //     TaskHelper.Run("RELOAD TOWERS", () => {
                //         patch_GameData.ReloadCustomTowers();
                //         if (patch_SaveData.AdventureActive)
                //             GotoAdventure(CurrentAdventureType, id);
                //         loading.Finished = true;
                //     });
                //     MapPaused = true;
                //     Selection = null;
                // }
            }
            orig_Update();
            if (crashDelay > 0)
                crashDelay--;
        }

        [PostFixing("TowerFall.MapScene", "System.Void SelectLevelPostfix()")]
        [MonoModIgnore]
        public extern void SelectLevel(MapButton button, bool scrollTo = true);

        public void SelectLevelPostfix()
        {
            if (currentCustomMapRenderer == null)
                return;
            
            if (Selection.Data == null)
            {
                currentCustomMapRenderer.OnSelectionChange("");
                return;
            }
            currentCustomMapRenderer.OnSelectionChange(Selection.Data.Title);
        }

        public void TweenOutAllButtonsAndRemove() 
        {
            foreach (var mapButton in Buttons) 
            {
                (mapButton as patch_MapButton).TweenOutAndRemoved();
            }
        }
    }

    public static class MapSceneExt 
    {
        public static void SetLevelSet(this MapScene mapScene, string levelSet) 
        {
            ((patch_MapScene)mapScene).LevelSet = levelSet;
        }

        public static string GetLevelSet(this MapScene mapScene) 
        {
            return ((patch_MapScene)mapScene).LevelSet ?? "TowerFall";
        }

        public static bool IsOfficialLevelSet(this MapScene mapScene) 
        {
            return ((patch_MapScene)mapScene).GetLevelSet() == "TowerFall";
        }

        public static AdventureType GetCurrentAdventureType(this MapScene mapScene) 
        {
            return ((patch_MapScene)mapScene).CurrentAdventureType;
        }
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchMapSceneBegin))]
    internal class PatchMapSceneBegin : Attribute {}

    internal static partial class MonoModRules 
    {

        public static void PatchMapSceneBegin(ILContext ctx, CustomAttribute attrib) 
        {
            var method = ctx.Method.DeclaringType.FindMethod("System.Void InitAdventureMap(FortRise.Adventure.AdventureType)");
            // var methodWithList = 
            //     ctx.Method.DeclaringType.FindMethod(
            //         "System.Void InitAdventureMap(System.Collections.Generic.List`1<TowerFall.MapButton[]>)");

            ILCursor cursor = new ILCursor(ctx);

            cursor.GotoNext(MoveType.After, instr => instr.MatchLdcI4(0));
            cursor.GotoNext(MoveType.Before, instr => instr.MatchLdcI4(0));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Call, method);
            cursor.GotoNext();
            cursor.GotoNext(MoveType.Before, instr => instr.MatchLdcI4(0));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Call, method);

            // Disabled for now
            // cursor.GotoNext(MoveType.After, 
            //     instr => instr.MatchNewobj("System.Collections.Generic.List`1<TowerFall.MapButton[]>"),
            //     instr => instr.MatchStloc(4));
            // cursor.Emit(OpCodes.Ldarg_0);
            // cursor.Emit(OpCodes.Ldloc_S, ctx.Body.Variables[4]);
            // cursor.Emit(OpCodes.Call, methodWithList);
        }
    }
}
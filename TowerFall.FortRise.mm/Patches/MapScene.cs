using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FortRise;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall.Patching 
{
    [MonoModPatch("TowerFall.MapScene")]
    public class MapScene : TowerFall.MapScene
    {
        private static int lastRandomVersusTower;
        private static string lastWorkshopVersusTowerTitle;
        private static string lastWorkshopVersusTowerAuthor;
        private TrialsLevelSelectOverlay trialsOverlay;
        private DarkWorldLevelSelectOverlay darkWorldOverlay;
        private QuestLevelSelectOverlay questOverlay;
        private Tween cameraTween;
        public bool MapPaused;
        public string LevelSet;
        public patch_MapRenderer Renderer;

        public MapScene(MainMenu.RollcallModes mode) : base(mode)
        {
        }

        [MonoModConstructor]
        [MonoModReplace]
        public static void cctor() { }

        internal static void FixedStatic()
        {
            lastRandomVersusTower = -1;
            MapScene.NoRandomStates = new bool[GameData.VersusTowers.Count];
        }

        [MonoModIgnore]
        [MonoModLinkTo("Monocle.Scene", "Begin")]
        public void base_Begin() { }

        [MonoModReplace]
        public override void Begin()
        {
            if (!this.IsOfficialLevelSet())
            {
                WorkshopLevels = true;
            }
            Entity entity = new Entity(-1);
            entity.Add(this.Renderer = new patch_MapRenderer(false));
            Add<Entity>(entity);

            this.Buttons = new List<MapButton>();

            switch (Mode)
            {
                case MainMenu.RollcallModes.Versus:
                    InitVersusButtons();
                    break;
                case MainMenu.RollcallModes.Quest:
                    InitQuestButtons();
                    break;
                case MainMenu.RollcallModes.DarkWorld:
                    InitDarkWorldButtons();
                    break;
                case MainMenu.RollcallModes.Trials:
                    InitTrialsButtons();
                    break;
                default:
                    throw new Exception("Mode not recognized!");
            }

            foreach (MapButton mapButton in this.Buttons)
            {
                base.Add<MapButton>(mapButton);
            }

            MapButton buttonSelected;
            switch (Mode)
            {
                case MainMenu.RollcallModes.Versus:
                    if (MainMenu.VersusMatchSettings.LevelSystem.CustomTower)
                    {
                        buttonSelected = this.Buttons[0];
                    }
                    else if (MainMenu.VersusMatchSettings.RandomVersusTower)
                    {
                        buttonSelected = this.Buttons[1];
                        for (int num6 = 0; num6 < this.Buttons.Count; num6++)
                        {
                            if (this.Buttons[num6] is VersusRandomSelect)
                            {
                                buttonSelected = this.Buttons[num6];
                                break;
                            }
                        }
                    }
                    else
                    {
                        buttonSelected = this.GetButtonFromID(MainMenu.VersusMatchSettings.LevelSystem.ID);
                        if (buttonSelected == null)
                        {
                            buttonSelected = this.Buttons[1];
                        }
                    }
                    break;
                case MainMenu.RollcallModes.Trials:
                    buttonSelected = this.GetButtonFromID(MainMenu.TrialsMatchSettings.LevelSystem.ID);
                    if (buttonSelected == null)
                    {
                        buttonSelected = this.Buttons[0];
                    }
                    break;
                case MainMenu.RollcallModes.Quest:
                    buttonSelected = this.GetButtonFromID(MainMenu.QuestMatchSettings.LevelSystem.ID);
                    if (buttonSelected == null)
                    {
                        buttonSelected = this.Buttons[0];
                    }
                    break;
                case MainMenu.RollcallModes.DarkWorld:
                    buttonSelected = this.GetButtonFromID(MainMenu.DarkWorldMatchSettings.LevelSystem.ID);
                    if (buttonSelected == null)
                    {
                        buttonSelected = this.Buttons[0];
                    }
                    break;
                default:
                    throw new Exception("Mode not recognized!");
            }

            if (buttonSelected.Data == null)
            {
                Renderer.OnStartSelection("");
            }
            else
            {
                Renderer.OnStartSelection(buttonSelected.Data.Title);
            }
            InitButtons(buttonSelected);
            CanAct = false;
            Add<CoroutineEntity>(new CoroutineEntity(this.IntroSequence()));
            Camera.Position = MapScene.FixedClampCamera(this.Selection.MapPosition, this);
            Cursor = new MapCursor(this.Selection);
            Add<MapCursor>(this.Cursor);

            switch (Mode)
            {
                case MainMenu.RollcallModes.Trials:
                    Add(this.trialsOverlay = new TrialsLevelSelectOverlay(this));
                    break;
                case MainMenu.RollcallModes.Quest:
                    Add(this.questOverlay = new QuestLevelSelectOverlay(this));
                    break;
                case MainMenu.RollcallModes.DarkWorld:
                    Add(this.darkWorldOverlay = new DarkWorldLevelSelectOverlay(this));
                    break;
            }
            if ((this.Mode == MainMenu.RollcallModes.Trials || this.Mode == MainMenu.RollcallModes.Versus) && !GameData.DarkWorldDLC)
            {
                base.Add(new MapDarkWorldGate(this));
            }
            base_Begin();

            FortRise.RiseCore.Events.Invoke_OnMapBegin(this);
        }

        [MonoModReplace]
        private void StartSession()
        {
            var session = new Session(MainMenu.CurrentMatchSettings);
            session.SetLevelSet(LevelSet);
            session.StartGame();
        }

        [MonoModReplace]
        [MonoModPatch("DarkWorldIntroSequence")] // This is a hack to fix System.TypeLoadException on MacOS and Linux
        private IEnumerator DarkWorldIntroSequence_Patch()
        {
            for (int i = 0; i < Buttons.Count; i += 1)
            {
                if (Buttons[i] is not DarkWorldMapButton || !this.IsOfficialLevelSet())
                {
                    continue;
                }

                if (SaveData.Instance.DarkWorld.ShouldRevealTower(Buttons[i].Data.ID.X))
                {
                    Music.Stop();
                    yield return Buttons[i].UnlockSequence(false);
                }
            }
            yield break;
        }

        [MonoModReplace]
        private IEnumerator QuestIntroSequence()
        {
            for (int i = 0; i < this.Buttons.Count; i += 1)
            {
                if (Buttons[i] is not QuestMapButton || !this.IsOfficialLevelSet())
                {
                    continue;
                }

                if (SaveData.Instance.Quest.ShouldRevealTower(this.Buttons[i].Data.ID.X))
                {
                    Music.Stop();
                    yield return this.Buttons[i].UnlockSequence(true);
                }
            }
            yield break;
        }

        [MonoModIgnore]
        private extern IEnumerator IntroSequence();

        internal void ChangeLevelSet()
        {
            WorkshopLevels = true;
            TweenOutAllButtonsAndRemove();
            Buttons.Clear();
            Add(new CustomLevelListLoader(this, 0));
        }

        public extern void orig_ExitWorkshop();

        public void ExitWorkshop()
        {
            if (LevelSet == "TowerFall")
            {
                orig_ExitWorkshop();
                return;
            }

            ExitCustomLevels();
        }

        public void ExitCustomLevels(int id = 1)
        {
            WorkshopLevels = false;
            TweenOutAllButtonsAndRemove();
            LevelSet = "TowerFall";

            Renderer.ChangeLevelSet(LevelSet);
            Buttons.Clear();

            switch (Mode)
            {
                case MainMenu.RollcallModes.Versus:
                    InitVersusButtons();
                    break;
                case MainMenu.RollcallModes.Quest:
                    InitQuestButtons();
                    break;
                case MainMenu.RollcallModes.DarkWorld:
                    InitDarkWorldButtons();
                    break;
                case MainMenu.RollcallModes.Trials:
                    InitTrialsButtons();
                    break;
            }

            if (Mode != MainMenu.RollcallModes.Trials)
            {
                this.LinkButtonsList();
            }

            if (id >= Buttons.Count)
            {
                id = Buttons.Count;
            }

            InitButtons(Buttons[0]);
            foreach (var button in Buttons)
            {
                Add(button);
            }

            ScrollToButton(Selection);
        }

        [MonoModReplace]
        public void InitVersusButtons()
        {
            Buttons.Add(new CustomLevelCategoryButton(MainMenu.RollcallModes.Versus));

            List<VersusTowerData> towers;

            if (this.IsOfficialLevelSet())
            {
                Buttons.Add(new GoToWorkshopMapButton());
                towers = GameData.VersusTowers;
            }
            else
            {
                towers = TowerRegistry.VersusTowerSets[this.GetLevelSet()];
            }

            Buttons.Add(new VersusRandomSelect());

            for (int i = 0; i < towers.Count; i++)
            {
                var tower = towers[i];
                if (tower.IsOfficialLevelSet())
                {
                    if (SaveData.Instance.Unlocks.GetTowerUnlocked(i))
                    {
                        Buttons.Add(new VersusMapButton(tower));
                    }
                    continue;
                }
                var customTower = TowerRegistry.VersusTowers[tower.GetLevelID()];
                var hidden = customTower.Configuration.IsHidden;
                if (hidden is null || !hidden.Invoke(customTower))
                {
                    Buttons.Add(new VersusMapButton(tower));
                }
            }

            LinkButtonsList();
            if (HasBegun)
            {
                InitButtons(Buttons[2]);
                foreach (MapButton mapButton in Buttons)
                {
                    Add(mapButton);
                }
            }
        }

        public void InitDarkWorldButtons()
        {
            Buttons.Add(new CustomLevelCategoryButton(MainMenu.RollcallModes.DarkWorld));

            List<DarkWorldTowerData> towers;

            if (this.IsOfficialLevelSet())
            {
                towers = GameData.DarkWorldTowers;
            }
            else
            {
                towers = TowerRegistry.DarkWorldTowerSets[this.GetLevelSet()];
            }

            for (int i = 0; i < towers.Count; i++)
            {
                var tower = towers[i];
                if (tower.IsOfficialLevelSet())
                {
                    if (SaveData.Instance.Unlocks.GetDarkWorldTowerUnlocked(i))
                    {
                        Buttons.Add(new DarkWorldMapButton(tower));
                    }
                    continue;
                }
                var customTower = TowerRegistry.DarkWorldTowers[tower.GetLevelID()];
                var hidden = customTower.Configuration.IsHidden;
                if (hidden is null || !hidden.Invoke(customTower))
                {
                    Buttons.Add(new DarkWorldMapButton(tower));
                }
            }

            LinkButtonsList();
        }

        public void InitQuestButtons()
        {
            Buttons.Add(new CustomLevelCategoryButton(MainMenu.RollcallModes.Quest));

            List<QuestLevelData> towers;

            if (this.IsOfficialLevelSet())
            {
                towers = GameData.QuestLevels.ToList();
            }
            else
            {
                towers = TowerRegistry.QuestTowerSets[this.GetLevelSet()];
            }

            for (int i = 0; i < towers.Count; i++)
            {
                var tower = towers[i];
                if (tower.IsOfficialLevelSet())
                {
                    if (SaveData.Instance.Unlocks.GetQuestTowerUnlocked(i))
                    {
                        Buttons.Add(new QuestMapButton(tower));
                    }
                    continue;
                }

                var customTower = TowerRegistry.QuestTowers[tower.GetLevelID()];
                var hidden = customTower.Configuration.IsHidden;
                if (hidden is null || !hidden.Invoke(customTower))
                {
                    Buttons.Add(new QuestMapButton(tower));
                }
            }
            LinkButtonsList();
        }

        public void InitTrialsButtons()
        {
            var list = new List<MapButton[]>();
            var adv = new CustomLevelCategoryButton(MainMenu.RollcallModes.Trials);
            Buttons.Add(adv);
            list.Add([adv, adv, adv]);

            TrialsLevelData[,] towers;

            if (this.IsOfficialLevelSet())
            {
                towers = GameData.TrialsLevels;
            }
            else
            {
                var rawTowers = TowerRegistry.TrialsTowerSets[this.GetLevelSet()];
                towers = new TrialsLevelData[rawTowers.Count, 3];
                for (int i = 0; i < rawTowers.Count; i++)
                {
                    for (int j = 0; j < rawTowers[i].Length; j++)
                    {
                        towers[i, j] = rawTowers[i][j];
                    }
                }
            }

            for (int k = 0; k < towers.GetLength(0); k++)
            {
                bool show = false;
                if (this.IsOfficialLevelSet())
                {
                    show = SaveData.Instance.Unlocks.GetTowerUnlocked(k);
                }
                else
                {
                    var levelID = towers[k, 0].GetLevelID();
                    
                    var customTower = TowerRegistry.TrialTowers[levelID[0..(levelID.Length - 2)]];
                    var hidden = customTower.Configuration.IsHidden;
                    show = hidden is null || !hidden.Invoke(customTower);
                }

                if (show)
                {
                    MapButton[] array = new MapButton[towers.GetLength(1)];
                    for (int l = 0; l < array.Length; l++)
                    {
                        array[l] = new TrialsMapButton(towers[k, l]);
                        this.Buttons.Add(array[l]);
                    }
                    for (int m = 0; m < array.Length; m++)
                    {
                        if (m > 0)
                        {
                            array[m].UpButton = array[m - 1];
                        }
                        if (m < array.Length - 1)
                        {
                            array[m].DownButton = array[m + 1];
                        }
                    }
                    list.Add(array);
                }
            }
            for (int n = 0; n < list.Count; n++)
            {
                if (n > 0)
                {
                    for (int num3 = 0; num3 < list[n].Length; num3++)
                    {
                        list[n][num3].LeftButton = list[n - 1][num3];
                    }
                }
                if (n < list.Count - 1)
                {
                    for (int num4 = 0; num4 < list[n].Length; num4++)
                    {
                        list[n][num4].RightButton = list[n + 1][num4];
                    }
                }
                for (int num5 = 0; num5 < list[n].Length; num5++)
                {
                    list[n][num5].MapXIndex = n;
                }
            }
        }


        [MonoModLinkTo("Monocle.Scene", "System.Void Update()")]
        [MonoModIgnore]
        public void base_Update()
        {
            base.Update();
        }

        [FixClampCamera]
        private extern void orig_Update();

        public override void Update()
        {
            if (MapPaused)
            {
                base_Update();
                return;
            }
            orig_Update();
        }

        public void TweenOutAllButtonsAndRemove()
        {
            foreach (var mapButton in Buttons)
            {
                (mapButton as patch_MapButton).TweenOutAndRemoved();
            }
        }

        public void TweenOutAllButtonsAndRemoveExcept(MapButton button)
        {
            foreach (var mapButton in Buttons)
            {
                if (button == mapButton)
                    continue;
                (mapButton as patch_MapButton).TweenOutAndRemoved();
            }
        }

        public static Vector2 FixedClampCamera(Vector2 position, MapScene map)
        {
            return new Vector2(
                MathHelper.Clamp(position.X, 160f, map.Renderer.GetInstanceWidth() - 160),
                MathHelper.Clamp(position.Y, 120f, map.Renderer.GetInstanceHeight() - 120));
        }

        [MonoModReplace]
        public void ScrollToPosition(Vector2 position)
        {
            Vector2 start = Camera.Position;
            cameraTween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 30, true);
            cameraTween.OnUpdate = t =>
            {
                Camera.Position = Vector2.Lerp(start, FixedClampCamera(position, this), t.Eased);
            };
        }

        public MapButton GetRandomWorkshopTower()
        {
            var list = new List<MapButton>(Buttons);
            list.RemoveAll(b => b is not CustomMapButton);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Locked)
                {
                    list.RemoveAt(i);
                    i--;
                }
            }

            if (list.Count > 1 && !string.IsNullOrEmpty(lastWorkshopVersusTowerTitle))
            {
                foreach (MapButton mapButton in list)
                {
                    if (mapButton.Data.Title == lastWorkshopVersusTowerTitle && 
                        mapButton.Data.Author == lastWorkshopVersusTowerAuthor)
                    {
                        list.Remove(mapButton);
                        break;
                    }
                }
            }

            list.Shuffle(new Random());
            lastWorkshopVersusTowerTitle = list[0].Data.Title;
            lastWorkshopVersusTowerAuthor = list[0].Data.Author;
            return list[0];
        }
    }

    public static class MapSceneExt 
    {
        public static void SetLevelSet(this TowerFall.MapScene mapScene, string levelSet) 
        {
            ((TowerFall.Patching.MapScene)mapScene).LevelSet = levelSet;
        }

        public static string GetLevelSet(this TowerFall.MapScene mapScene) 
        {
            return ((TowerFall.Patching.MapScene)mapScene).LevelSet ?? "TowerFall";
        }

        public static bool IsOfficialLevelSet(this TowerFall.MapScene mapScene) 
        {
            return ((TowerFall.Patching.MapScene)mapScene).GetLevelSet() == "TowerFall";
        }
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixClampCamera))]
    internal class FixClampCamera : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void FixClampCamera(ILContext ctx, CustomAttribute attrib) 
        {
            var method = ctx.Method.DeclaringType.FindMethod("Microsoft.Xna.Framework.Vector2 FixedClampCamera(Microsoft.Xna.Framework.Vector2,TowerFall.MapScene)");

            var cursor = new ILCursor(ctx);

            cursor.GotoNext(instr => instr.MatchCall("TowerFall.MapScene", "ClampCamera"));
            cursor.Next.Operand = method;

            cursor.Emit(OpCodes.Ldarg_0);
        }
    }
}

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
        public string TowerSet;
        public patch_MapRenderer Renderer;
        public static HashSet<string> NoRandom = [];

        [Obsolete("Use 'NoRandom' instead")]
        public static bool[] NoRandomStates;

        public MapScene(MainMenu.RollcallModes mode) : base(mode)
        {
        }

        [MonoModConstructor]
        [MonoModReplace]
        public static void cctor()
        {
            NoRandom = [];
        }

        internal static void FixedStatic()
        {
            lastRandomVersusTower = -1;
            TowerFall.MapScene.NoRandomStates = new bool[GameData.VersusTowers.Count];
        }

        [MonoModIgnore]
        [MonoModLinkTo("Monocle.Scene", "Begin")]
        public void base_Begin() { }

        [MonoModReplace]
        public override void Begin()
        {
            if (!this.IsOfficialTowerSet)
            {
                WorkshopLevels = true;
            }
            Entity entity = new Entity(-1);
            entity.Add(Renderer = new patch_MapRenderer(false));
            Add(entity);

            Buttons = [];

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

            foreach (MapButton mapButton in Buttons)
            {
                Add(mapButton);
            }

            MapButton buttonSelected;
            switch (Mode)
            {
                case MainMenu.RollcallModes.Versus:
                    if (MainMenu.VersusMatchSettings.LevelSystem.CustomTower)
                    {
                        buttonSelected = Buttons[0];
                    }
                    else if (MainMenu.VersusMatchSettings.RandomVersusTower)
                    {
                        buttonSelected = Buttons[1];
                        for (int num6 = 0; num6 < Buttons.Count; num6 += 1)
                        {
                            if (Buttons[num6] is VersusRandomSelect)
                            {
                                buttonSelected = Buttons[num6];
                                break;
                            }
                        }
                    }
                    else
                    {
                        buttonSelected = this.GetButtonFromID(MainMenu.VersusMatchSettings.LevelSystem.ID);
                        buttonSelected ??= Buttons[1];
                    }
                    break;
                case MainMenu.RollcallModes.Trials:
                    buttonSelected = this.GetButtonFromID(MainMenu.TrialsMatchSettings.LevelSystem.ID);
                    buttonSelected ??= Buttons[0];
                    break;
                case MainMenu.RollcallModes.Quest:
                    buttonSelected = this.GetButtonFromID(MainMenu.QuestMatchSettings.LevelSystem.ID);
                    buttonSelected ??= Buttons[0];
                    break;
                case MainMenu.RollcallModes.DarkWorld:
                    buttonSelected = this.GetButtonFromID(MainMenu.DarkWorldMatchSettings.LevelSystem.ID);
                    buttonSelected ??= Buttons[0];
                    break;
                default:
                    throw new Exception("Mode not recognized!");
            }

            if (buttonSelected.Data is null)
            {
                Renderer.OnStartSelection("");
            }
            else
            {
                Renderer.OnStartSelection(buttonSelected.Data.LevelData.LevelID);
            }
            InitButtons(buttonSelected);
            CanAct = false;
            Add(new CoroutineEntity(this.IntroSequence()));
            Camera.Position = MapScene.FixedClampCamera(this.Selection.MapPosition, this);
            Cursor = new MapCursor(this.Selection);
            Add(this.Cursor);

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
        }

        [MonoModReplace]
        private void StartSession()
        {
            var session = new Session(MainMenu.CurrentMatchSettings);
            session.TowerSet = TowerSet;
            session.StartGame();
        }

        [MonoModReplace]
        [MonoModPatch("DarkWorldIntroSequence")] // This is a hack to fix System.TypeLoadException on MacOS and Linux
        private IEnumerator DarkWorldIntroSequence_Patch()
        {
            for (int i = 0; i < Buttons.Count; i += 1)
            {
                if (Buttons[i] is not DarkWorldMapButton || !this.IsOfficialTowerSet)
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
                if (Buttons[i] is not QuestMapButton || !this.IsOfficialTowerSet)
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
            if (TowerSet == "TowerFall")
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
            TowerSet = "TowerFall";

            Renderer.ChangeLevelSet(TowerSet);
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

            if (this.IsOfficialTowerSet)
            {
                Buttons.Add(new GoToWorkshopMapButton());
                towers = GameData.VersusTowers;
            }
            else
            {
                towers = [];
                var tempTowers = TowerRegistry.VersusTowerSets[this.TowerSet];
                foreach (var tow in tempTowers)
                {
                    if (tow.Levels.Count > 0)
                    {
                        towers.Add(tow);
                    }
                }
            }

            if (towers.Count > 0)
            {
                Buttons.Add(new VersusRandomSelect());
            }


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
                InitButtons(Buttons[0]);
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

            if (this.IsOfficialTowerSet)
            {
                towers = GameData.DarkWorldTowers;
            }
            else
            {
                towers = [];
                var tempTowers = TowerRegistry.DarkWorldTowerSets[this.TowerSet];
                foreach (var tow in tempTowers)
                {
                    if (tow.Levels.Count > 0)
                    {
                        towers.Add(tow);
                    }
                }
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

            if (this.IsOfficialTowerSet)
            {
                towers = GameData.QuestLevels.ToList();
            }
            else
            {
                towers = TowerRegistry.QuestTowerSets[this.TowerSet];
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

            if (this.IsOfficialTowerSet)
            {
                towers = GameData.TrialsLevels;
            }
            else
            {
                var rawTowers = TowerRegistry.TrialsTowerSets[this.TowerSet];
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
                if (this.IsOfficialTowerSet)
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

        [MonoModReplace]
        public void SelectLevel(MapButton button, bool scrollTo = true)
        {
            if (button is null)
            {
                return;
            }

            Selection.OnDeselect();
            Selection = button;
            Selection.OnSelect();

            if (scrollTo)
            {
                ScrollToButton(Selection);
            }

            if (button.Data is null)
            {
                MapButton.PlayTowerSound(MapButton.TowerType.Random);
                Renderer.OnSelectionChange("");
            }
            else 
            {
                MapButton.PlayTowerSound(button.Data.IconTile);
                Renderer.OnSelectionChange(button.Data.LevelData.LevelID);
            }
        }
    }
}

namespace TowerFall
{
    internal static class TowerMapDataExt
    {
        extension(TowerMapData data)
        {
            public LevelData LevelData 
            {
                get => ((patch_TowerMapData)data).LevelData;
            }
        }
    }

    public static class MapSceneExt 
    {
        extension(TowerFall.MapScene mapScene)
        {
            [Obsolete("Use 'MapScene.TowerSet' property instead.")]
            public void SetLevelSet(string levelSet)
            {
                ((Patching.MapScene)mapScene).TowerSet = levelSet;
            }

            [Obsolete("Use 'MapScene.TowerSet' property instead.")]
            public string GetLevelSet() 
            {
                return ((Patching.MapScene)mapScene).TowerSet ?? "TowerFall";
            }

            // TODO: Deprecate this when .NET 10 comes out
            [Obsolete("Use 'MapScene.IsOfficialTowerSet' property instead.")]
            public bool IsOfficialLevelSet() 
            {
                return mapScene.IsOfficialTowerSet;
            }

            public string TowerSet
            {
                get => ((Patching.MapScene)mapScene).TowerSet ?? "TowerFall";
                set 
                {
                    ((Patching.MapScene)mapScene).TowerSet = value;
                }
            }

            public bool IsOfficialTowerSet
            {
                get
                {
                    return ((Patching.MapScene)mapScene).TowerSet == "TowerFall";
                }
            }
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

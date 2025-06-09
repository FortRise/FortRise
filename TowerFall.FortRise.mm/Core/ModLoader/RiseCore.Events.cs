using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TowerFall;

namespace FortRise;

public static partial class RiseCore
{
    /// <summary>
    /// List of built-in useful events that can be subscribe by a modules.
    /// </summary>
    public static partial class Events
    {
        public delegate void AfterModdedLoadContentHandler(FortContent content);
        public static event AfterModdedLoadContentHandler OnAfterModdedLoadContent;
        internal static void Invoke_OnAfterModdedLoadContent(FortContent content) 
        {
            OnAfterModdedLoadContent?.Invoke(content);
        }

        public delegate void QuestSpawnWaveHandler(
            QuestControl control, int waveNum,
            List<IEnumerator> groups, int[] floors,
            bool dark, bool slow, bool scroll);

        /// <summary>
        /// Called when the quest wave spawn
        /// </summary>
        public static event QuestSpawnWaveHandler OnQuestSpawnWave;
        internal static void Invoke_OnQuestSpawnWave(
            QuestControl control, int waveNum,
            List<IEnumerator> groups, int[] floors,
            bool dark, bool slow, bool scroll)
        {
            OnQuestSpawnWave?.Invoke(control, waveNum, groups, floors, dark, slow, scroll);
        }

        public delegate void SlotVariantCreated(MatchVariants matchVariants, List<List<VariantItem>> variantSlots);

        /// <summary>
        /// Called when the quest wave spawn
        /// </summary>
        public static event SlotVariantCreated OnSlotVariantCreated;
        internal static void Invoke_OnSlotVariantCreated(MatchVariants matchVariants, List<List<VariantItem>> variantSlots)
        {
            OnSlotVariantCreated?.Invoke(matchVariants, variantSlots);
        }

        /// <summary>
        /// Called when the main menu has started
        /// </summary>
        public static event Action<MainMenu> OnMainBegin;
        internal static void Invoke_OnMainBegin(MainMenu menu)
        {
            OnMainBegin?.Invoke(menu);
        }

        /// <summary>
        /// Called when the map scene has started
        /// </summary>
        public static event Action<MapScene> OnMapBegin;
        internal static void Invoke_OnMapBegin(MapScene map)
        {
            OnMapBegin?.Invoke(map);
        }

        /// <summary>
        /// Called when a enemy has been killed by a player
        /// </summary>
        public static event Action<QuestRoundLogic, Vector2, int, int> OnQuestRegisterEnemyKills;
        internal static void Invoke_OnQuestRegisterEnemyKills(QuestRoundLogic roundLogic, Vector2 at, int killerIndex, int points)
        {
            OnQuestRegisterEnemyKills?.Invoke(roundLogic, at, killerIndex, points);
        }

        /// <summary>
        /// Called after the level has been loaded
        /// </summary>
        public static event Action<RoundLogic> OnLevelLoaded;
        internal static void Invoke_OnLevelLoaded(RoundLogic logic)
        {
            OnLevelLoaded?.Invoke(logic);
        }

        /// <summary>
        /// Called when entered a level.
        /// </summary>
        public static event Action OnLevelEntered;
        internal static void Invoke_OnLevelEntered()
        {
            OnLevelEntered?.Invoke();
        }

        /// <summary>
        /// Called when exited the level via Quit or Map.
        /// </summary>
        public static event Action OnLevelExited;
        internal static void Invoke_OnLevelExited()
        {
            OnLevelExited?.Invoke();
        }

        /// <summary>
        /// Called before the GameData.Load() called.
        /// </summary>
        public static event Action OnBeforeDataLoad;
        internal static void Invoke_OnBeforeDataLoad()
        {
            OnBeforeDataLoad?.Invoke();
        }

        /// <summary>
        /// Called after the GameData.Load() called.
        /// </summary>
        public static event Action OnAfterDataLoad;
        internal static void Invoke_OnAfterDataLoad()
        {
            OnAfterDataLoad?.Invoke();
        }

        /// <summary>
        /// Called before the game initialization state.
        /// </summary>
        public static event Action OnPreInitialize;

        /// <summary>
        /// Called after the game initialization state.
        /// </summary>
        public static event Action OnPostInitialize;
        internal static void Invoke_OnPreInitialize()
        {
            OnPreInitialize?.Invoke();
        }

        internal static void Invoke_OnPostInitialize()
        {
            OnPostInitialize?.Invoke();
        }

        /// <summary>
        /// Called before the game loop.
        /// </summary>
        public static event Action<GameTime> OnBeforeUpdate;
        internal static void Invoke_BeforeUpdate(GameTime gameTime)
        {
            OnBeforeUpdate?.Invoke(gameTime);
        }

        /// <summary>
        /// Called during the game loop.
        /// </summary>
        public static event Action<GameTime> OnUpdate;
        internal static void Invoke_Update(GameTime gameTime)
        {
            OnUpdate?.Invoke(gameTime);
        }

        /// <summary>
        /// Called after the game loop.
        /// </summary>
        public static event Action<GameTime> OnAfterUpdate;
        internal static void Invoke_AfterUpdate(GameTime gameTime)
        {
            OnAfterUpdate?.Invoke(gameTime);
        }

        /// <summary>
        /// Called before the backbuffer renders.
        /// </summary>
        public static event Action<SpriteBatch> OnBeforeRender;
        internal static void Invoke_BeforeRender(SpriteBatch spriteBatch)
        {
            OnBeforeRender?.Invoke(spriteBatch);
        }

        /// <summary>
        /// Called when the backbuffer renders.
        /// </summary>
        public static event Action<SpriteBatch> OnRender;
        internal static void Invoke_Render(SpriteBatch spriteBatch)
        {
            OnRender?.Invoke(spriteBatch);
        }

        /// <summary>
        /// Called after the backbuffer renders.
        /// </summary>
        public static event Action<SpriteBatch> OnAfterRender;
        internal static void Invoke_AfterRender(SpriteBatch spriteBatch)
        {
            OnAfterRender?.Invoke(spriteBatch);
        }

        /// <summary>
        /// Called after a mod initialized.
        /// </summary>
        public static event Action<FortModule> OnModInitialized;
        internal static void Invoke_OnModInitialized(FortModule module)
        {
            OnModInitialized?.Invoke(module);
        }

        public delegate void ResourceAssignTypeHandler(string path, string filename, ref Type ResourceType);
        public static event ResourceAssignTypeHandler OnResourceAssignType;
        internal static void Invoke_OnResourceAssignType(string path, string filename, ref Type ResourceType)
        {
            OnResourceAssignType?.Invoke(path, filename, ref ResourceType);
        }

        public delegate void AdventureTowerAddHandler<T>(string levelSet, T towerData)
        where T : LevelData;

        public delegate void AdventureTowersAddHandler<T>(string levelSet, T[] towerData)
        where T : LevelData;

        /// <summary>
        /// Called after an adventure dark world tower has been added.
        /// </summary>
        public static event AdventureTowerAddHandler<DarkWorldTowerData> OnAdventureDarkWorldTowerDataAdd;

        internal static void Invoke_OnAdventureDarkWorldTowerDataAdd(string levelSet, DarkWorldTowerData towerData)
        {
            OnAdventureDarkWorldTowerDataAdd?.Invoke(levelSet, towerData);
        }

        /// <summary>
        /// Called after an adventure quest tower has been added.
        /// </summary>
        public static event AdventureTowerAddHandler<QuestLevelData> OnAdventureQuestTowerDataAdd;
        internal static void Invoke_OnAdventureQuestTowerDataAdd(string levelSet, QuestLevelData towerData)
        {
            OnAdventureQuestTowerDataAdd?.Invoke(levelSet, towerData);
        }

        /// <summary>
        /// Called after an adventure versus tower has been added.
        /// </summary>
        public static event AdventureTowerAddHandler<VersusTowerData> OnAdventureVersusTowerDataAdd;
        internal static void Invoke_OnAdventureVersusTowerDataAdd(string levelSet, VersusTowerData towerData)
        {
            OnAdventureVersusTowerDataAdd?.Invoke(levelSet, towerData);
        }

        /// <summary>
        /// Called after an adventure trials towers has been added.
        /// </summary>
        public static event AdventureTowersAddHandler<TrialsLevelData> OnAdventureTrialsTowerDatasAdd;
        internal static void Invoke_OnAdventureTrialsTowerDatasAdd(string levelSet, TrialsLevelData[] towerData)
        {
            OnAdventureTrialsTowerDatasAdd?.Invoke(levelSet, towerData);
        }
    }
}

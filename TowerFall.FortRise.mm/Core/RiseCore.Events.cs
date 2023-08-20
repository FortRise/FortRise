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
        /// Called when the map scene has started
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
    }
}
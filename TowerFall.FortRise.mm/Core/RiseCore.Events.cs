using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FortRise;

public static partial class RiseCore 
{
    /// <summary>
    /// List of built-in useful events that can be subscribe by a modules.
    /// </summary>
    public static partial class Events 
    {
        /// <summary>
        /// Called after the level has been loaded
        /// </summary>
        public static event Action OnLevelLoaded;
        internal static void Invoke_OnLevelLoaded()  
        {
            OnLevelLoaded?.Invoke();
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
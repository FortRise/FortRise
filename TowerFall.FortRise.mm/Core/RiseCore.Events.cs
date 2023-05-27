using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FortRise;

public static partial class RiseCore 
{
    [Obsolete("Use Events.OnBeforeUpdate instead")]
    public static event Action<GameTime> OnBeforeUpdate
    {
        add => Events.OnBeforeUpdate += value;
        remove => Events.OnBeforeUpdate -= value;
    }

    [Obsolete("Use Events.OnUpdate instead")]
    public static event Action<GameTime> OnUpdate
    {
        add => Events.OnUpdate += value;
        remove => Events.OnUpdate -= value;
    }
    
    [Obsolete("Use Events.OnAfterUpdate instead")]
    public static event Action<GameTime> OnAfterUpdate
    {
        add => Events.OnAfterUpdate += value;
        remove => Events.OnAfterUpdate -= value;
    }

    [Obsolete("Use Events.OnBeforeRender instead")]
    public static event Action<SpriteBatch> OnBeforeRender 
    {
        add => Events.OnBeforeRender += value;
        remove => Events.OnBeforeRender -= value;
    }
    
    [Obsolete("Use Events.OnRender instead")]
    public static event Action<SpriteBatch> OnRender 
    {
        add => Events.OnRender += value;
        remove => Events.OnRender -= value;
    }
    
    [Obsolete("Use Events.OnAfterRender instead")]
    public static event Action<SpriteBatch> OnAfterRender 
    {
        add => Events.OnAfterRender += value;
        remove => Events.OnAfterRender -= value;
        
    }

    public static partial class Events 
    {
        public static event Action OnPreInitialize;
        public static event Action OnPostInitialize;
        internal static void Invoke_OnPreInitialize() 
        {
            OnPreInitialize?.Invoke();
        }

        internal static void Invoke_OnPostInitialize() 
        {
            OnPostInitialize?.Invoke();
        }

        public static event Action<GameTime> OnBeforeUpdate;
        internal static void Invoke_BeforeUpdate(GameTime gameTime) 
        {
            OnBeforeUpdate?.Invoke(gameTime);
        }
        public static event Action<GameTime> OnUpdate;
        internal static void Invoke_Update(GameTime gameTime) 
        {
            OnUpdate?.Invoke(gameTime);
        }
        public static event Action<GameTime> OnAfterUpdate;
        internal static void Invoke_AfterUpdate(GameTime gameTime) 
        {
            OnAfterUpdate?.Invoke(gameTime);
        }

        public static event Action<SpriteBatch> OnBeforeRender;
        internal static void Invoke_BeforeRender(SpriteBatch spriteBatch) 
        {
            OnBeforeRender?.Invoke(spriteBatch);
        }
        public static event Action<SpriteBatch> OnRender;
        internal static void Invoke_Render(SpriteBatch spriteBatch) 
        {
            OnRender?.Invoke(spriteBatch);
        }
        public static event Action<SpriteBatch> OnAfterRender; 
        internal static void Invoke_AfterRender(SpriteBatch spriteBatch) 
        {
            OnAfterRender?.Invoke(spriteBatch);
        }
    }
}
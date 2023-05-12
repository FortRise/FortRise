using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FortRise;

public static partial class RiseCore 
{
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
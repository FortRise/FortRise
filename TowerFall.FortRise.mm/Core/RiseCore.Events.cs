using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TowerFall;

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

    // TODO Move this on to the Events 
    public delegate void DarkWorldComplete_ResultHandler(
        int levelID, DarkWorldDifficulties difficulties,
        int playerAmount, long time, int continues, int deaths, int curses);

    public static event DarkWorldComplete_ResultHandler OnDarkWorldComplete_Result;

    internal static void InvokeDarkWorldComplete_Result(
        int levelID, DarkWorldDifficulties difficulties,
        int playerAmount, long time, int continues, int deaths, int curses) 
    {
        if (patch_SaveData.AdventureActive)
        {
            patch_GameData.AdventureWorldTowers[levelID].Stats.Complete(
                difficulties, playerAmount, time,
                continues, deaths, curses);
            return;
        }
        SaveData.Instance.DarkWorld.Towers[levelID].Complete(
            difficulties, playerAmount, time, continues, deaths, curses
        );
        OnDarkWorldComplete_Result?.Invoke(levelID, difficulties, playerAmount, time, continues, deaths, curses);
    }
}
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

    public delegate bool GetArrowTypesHandler(string name, out ArrowTypes types);
    public static event GetArrowTypesHandler OnGetSkeletonArrowTypes;

    public delegate void MainMenu_CreateOptionsHandler(List<OptionsButton> optionsList);
    public static event MainMenu_CreateOptionsHandler OnMainMenu_CreateOptions;
    internal static void InvokeMainMenu_CreateOptions(List<OptionsButton> optionsList) 
    {
        OnMainMenu_CreateOptions?.Invoke(optionsList);
    }


    public delegate bool QuestSpawnPortal_FinishSpawnHandler(string entityName, Vector2 position, Facing facing, Level level);

    public static event QuestSpawnPortal_FinishSpawnHandler OnQuestSpawnPortal_FinishSpawn;

    internal static void InvokeQuestSpawnPortal_FinishSpawn(string name, Vector2 position, Facing facing, Level level) 
    {
        if (EnemyLoader.TryGetValue(name, out EnemyLoader loader)) 
        {
            level.Add(loader?.Invoke(position + new Vector2(0f, 2f), facing));
            return;
        }
        if (name.Contains("Skeleton") || name.Contains("Jester")) 
        {
            ArrowTypes arrows = ArrowTypes.Normal;
            bool hasShields = false;
            bool hasWings = false;
            bool canMimic = false;
            bool jester = false;
            bool boss = false;

            if (name.EndsWith("S"))
                hasShields = true;

            if (name.Contains("Wing"))
                hasWings = true;

            if (name.Contains("Mimic"))
                canMimic = true;

            if (name.Contains("Boss"))
                boss = true;
            
            if (name.Contains("Jester"))
                jester = true;
            
            arrows = GetSkeletonArrowTypes(name);
            level.Add(new Skeleton(position + new Vector2(0f, 2f), facing, arrows, hasShields, hasWings, canMimic, jester, boss));
            return;
        }
        var invoked = OnQuestSpawnPortal_FinishSpawn?.Invoke(name, position, facing, level);
        if (invoked == null || !invoked.Value)
        {
            Logger.Error($"Entity name: {name} failed to spawn as it does not exists!");
        }

        ArrowTypes GetSkeletonArrowTypes(string name) 
        {
            if (OnGetSkeletonArrowTypes != null && OnGetSkeletonArrowTypes(name, out ArrowTypes types)) 
                return types;
            
            if (name.Contains("Bomb"))
                return ArrowTypes.Bomb;
            if (name.Contains("SuperBomb"))
                return ArrowTypes.SuperBomb;
            if (name.Contains("Bramble"))
                return ArrowTypes.Bramble;
            if (name.Contains("Drill"))
                return ArrowTypes.Drill;
            if (name.Contains("Trigger"))
                return ArrowTypes.Trigger;
            if (name.Contains("Toy"))
                return ArrowTypes.Toy;
            if (name.Contains("Feather"))
                return ArrowTypes.Feather;
            if (name.Contains("Laser"))
                return ArrowTypes.Laser;
            if (name.Contains("Prism"))
                return ArrowTypes.Prism;
            return ArrowTypes.Normal;
        }
    }

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
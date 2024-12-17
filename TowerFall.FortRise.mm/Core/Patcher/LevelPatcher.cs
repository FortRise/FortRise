#pragma warning disable CS0618
using TowerFall;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FortRise;

/// <summary>
/// Patch any towers by deriving from this class.
/// Name your class with the tower name you want to patch, must not have any spaces.
/// Make sure that you properly spelled it and it is case sensitive.
/// </summary>
public abstract class TowerPatch
{
    public virtual void VersusPatch(VersusTowerPatchContext ctx) { }
    public virtual void DarkWorldPatch(DarkWorldTowerPatchContext ctx) { }

    public virtual TowerTheme ThemeOverride() 
    {
        return null;
    }
}

public static class TowerPatchRegistry 
{
    // garbage
    private static List<Type> toPatch = new();

    private static VersusTowerPatchContext versusCtx = new();
    private static DarkWorldTowerPatchContext dwCtx = new();

    public static void Initialize() 
    {
        foreach (var type in toPatch) 
        {
            var instance = Activator.CreateInstance(type) as TowerPatch;
            if (instance == null)
                continue;

            Versus(type, instance);
        }
        toPatch = null;
    }

    public static void Register<T>(FortModule module) 
    where T : TowerPatch
    {
        Register(typeof(T), module);
    }

    public static void Register(Type type, FortModule module) 
    {
        if (type.IsSubclassOf(typeof(TowerPatch))) 
        {
            toPatch.Add(type);
        }
    }

    private static void Versus(Type levelPatchType, TowerPatch patcher) 
    {
        var tower = GameData.VersusTowers
            .Where(x => levelPatchType.Name
                .StartsWith(x.GetLevelID().Replace("'", "").Replace(" ", "")))
            .FirstOrDefault();
        if (tower == null) 
        {
            if (levelPatchType.Name.StartsWith("Global")) 
            {
                foreach (var tow in GameData.VersusTowers) 
                {
                    versusCtx.Patch(tow);
                    patcher.VersusPatch(versusCtx);
                    tow.ApplyPatch(versusCtx);
                }
            }
            return;
        }
        versusCtx.Patch(tower);
        patcher.VersusPatch(versusCtx);
        tower.ApplyPatch(versusCtx);
    }

    private static void DarkWorld(Type levelPatchType, TowerPatch patcher) 
    {
        var tower = GameData.DarkWorldTowers
            .Where(x => levelPatchType.Name
                .StartsWith(x.GetLevelID().Replace("'", "").Replace(" ", "")))
            .FirstOrDefault();
        if (tower == null) 
        {
            if (levelPatchType.Name.StartsWith("Global")) 
            {
                foreach (var tow in GameData.DarkWorldTowers) 
                {
                    dwCtx.Patch(tow);
                    patcher.VersusPatch(versusCtx);
                    // tow.ApplyPatch(versusCtx);
                }
            }
            return;
        }
        dwCtx.Patch(tower);
        patcher.DarkWorldPatch(dwCtx);
        // tower.ApplyPatch(dwCtx);
    }
}

public interface ITowerPatchContext
{
    public LevelData Data { get; }
}

public class VersusTowerPatchContext : ITowerPatchContext
{

    public LevelData Data => data;

    private VersusTowerData data;

    private Dictionary<string, int[]> modifiedTreasureMasks = new();

    internal VersusTowerPatchContext() {}


    public void Patch(VersusTowerData data)
    {
        this.data = data;
        if (!modifiedTreasureMasks.ContainsKey(data.GetLevelID())) 
        {
            var origTreasureMask = data.TreasureMask;
            var moddedTreasureMask = new int[origTreasureMask.Length];
            Array.Copy(origTreasureMask, moddedTreasureMask, origTreasureMask.Length);
            modifiedTreasureMasks.Add(data.GetLevelID(), moddedTreasureMask);
        }
    }

    public int[] ReceivePatch(string levelID) 
    {
        return modifiedTreasureMasks[levelID];
    }

#region TreasureRates
    /// <summary>
    /// Increase the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">An enum pickup to add or increase</param>
    /// <param name="rate">How many pickups in this tower will appear, default is 1</param>
    /// <returns>A context of this struct</returns>
    public VersusTowerPatchContext IncreaseTreasureRates(Pickups pickup, int rate = 1) 
    {
        var moddedTreasureMask = modifiedTreasureMasks[data.GetLevelID()];
        var currentRate = moddedTreasureMask[(int)pickup];
        moddedTreasureMask[(int)pickup] = currentRate + rate;
        return this;
    }

    /// <summary>
    /// Increase the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">A string pickup to add or increase</param>
    /// <param name="rate">How many pickups in this tower will appear, default is 1</param>
    /// <returns>A context of this struct</returns>
    public VersusTowerPatchContext IncreaseTreasureRates(string pickup, int rate = 1) 
    {
        var moddedTreasureMask = modifiedTreasureMasks[data.GetLevelID()];
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        var currentRate = moddedTreasureMask[(int)enumPickup];
        moddedTreasureMask[(int)enumPickup] = currentRate + rate;
        return this;
    }

    /// <summary>
    /// Decrease the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">A string pickup to decrease or completely removed if reached to 0</param>
    /// <param name="rate">How many rates of the pickup in this tower will be decrease, default is 1</param>
    /// <returns>A context of this struct</returns>
    public VersusTowerPatchContext DecreaseTreasureRates(string pickup, int rate = 1) 
    {
        var moddedTreasureMask = modifiedTreasureMasks[data.GetLevelID()];
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        var currentRate = moddedTreasureMask[(int)enumPickup];
        if (currentRate == 0)
            return this;
        moddedTreasureMask[(int)enumPickup] = currentRate + rate;
        return this;
    }

    /// <summary>
    /// Decrease the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">An enum pickup to decrease or completely removed if reached to 0</param>
    /// <param name="rate">How many rates of the pickup in this tower will be decrease, default is 1</param>
    /// <returns>A context of this struct</returns>
    public VersusTowerPatchContext DecreaseTreasureRates(Pickups pickup, int rate = 1) 
    {
        var moddedTreasureMask = modifiedTreasureMasks[data.GetLevelID()];
        var currentRate = moddedTreasureMask[(int)pickup];
        if (currentRate == 0)
            return this;
        moddedTreasureMask[(int)pickup] = currentRate - rate;
        return this;
    }

    /// <summary>
    /// Remove the treasure rate of a pickup in this tower. (Note that this will not be completely removed,
    /// if there is any mods that adding the same pickup as you want to remove them).
    /// </summary>
    /// <param name="pickup">An enum pickup to remove in this tower</param>
    /// <returns>A context of this struct</returns>
    public VersusTowerPatchContext RemoveTreasureRates(Pickups pickup) 
    {
        var moddedTreasureMask = modifiedTreasureMasks[data.GetLevelID()];
        moddedTreasureMask[(int)pickup] = 0;
        return this;
    }

    /// <summary>
    /// Remove the treasure rate of a pickup in this tower. (Note that this will not be completely removed,
    /// if there is any mods that adding the same pickup as you want to remove them).
    /// </summary>
    /// <param name="pickup">A string pickup to remove in this tower</param>
    /// <returns>A context of this struct</returns>
    public VersusTowerPatchContext RemoveTreasureRates(string pickup) 
    {
        var moddedTreasureMask = modifiedTreasureMasks[data.GetLevelID()];
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        moddedTreasureMask[(int)enumPickup] = 0;
        return this;
    }

#endregion
}

public class DarkWorldTowerPatchContext : ITowerPatchContext
{
    public LevelData Data => data;

    private DarkWorldTowerData data;

    internal DarkWorldTowerPatchContext() 
    {
    }

    public void Patch(DarkWorldTowerData data)
    {
        this.data = data;
    }


    public void AddTreasure(int level, Pickups pickup, DarkWorldDifficulties difficulty) 
    {
        for (int i = 0; i < 4; i++) 
        {
            AddTreasure(level, pickup, i, difficulty);
        }
    }

    public void AddTreasure(int level, string pickup, DarkWorldDifficulties difficulty) 
    {
        var realPickup = PickupsRegistry.StringToTypes[pickup];
        AddTreasure(level, realPickup, difficulty);
    }

    public void AddTreasure(
        int level, string pickup, int playerAmount,
        DarkWorldDifficulties difficulty) 
    {
        var realPickup = PickupsRegistry.StringToTypes[pickup];
        AddTreasure(level, realPickup, playerAmount, difficulty);
    }

    public void AddTreasure(
        int level, Pickups pickup, int playerAmount, 
        DarkWorldDifficulties difficulty) 
    {
        var diff = difficulty switch 
        {
            DarkWorldDifficulties.Normal => data.Normal,
            DarkWorldDifficulties.Hardcore => data.Hardcore,
            _ => data.Legendary
        };
        var treasureData = diff[level].TreasureData;
        treasureData[playerAmount].Add(pickup);
    }
}
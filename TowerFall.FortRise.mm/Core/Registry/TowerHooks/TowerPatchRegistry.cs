#pragma warning disable CS0618
using TowerFall;
using Monocle;
using System.Collections.Generic;

namespace FortRise;

/// <summary>
/// Hook any towers by implementing from this interface.
/// </summary>
public interface ITowerHook 
{
    /// <summary>
    /// This will ignore the <see cref="ITowerHook.TargetTowers" /> property when implemented.
    /// </summary>
    public bool IsGlobal => false;

    /// <summary>
    /// Filter which tower id will be hooked.
    /// </summary>
    public HashSet<string> TargetTowers { get; }

    /// <summary>
    /// Sets to true to prevent IgnoreTowerItemSet not running this patch.
    /// </summary>
    public bool AffectedByIgnoreTowerItemSetVariant => true;

    /// <summary>
    /// Allows you to modify treasure rates from the target towers.
    /// </summary>
    /// <param name="ctx">The utility to modify treasure rates</param>
    public void VersusTowerTreasurePatch(IVersusTowerTreasurePatchContext ctx) { }
}


public static class TowerPatchRegistry 
{
    public static Dictionary<string, ITowerHookEntry> Hooks = new Dictionary<string, ITowerHookEntry>();
}

public interface IVersusTowerTreasurePatchContext
{
    void DecreaseTreasureRates(string pickup, int rate = 1);
    void DecreaseTreasureRates(Pickups pickup, int rate = 1);
    void IncreaseTreasureRates(Pickups pickup, int rate = 1);
    void IncreaseTreasureRates(string pickup, int rate = 1);
    void RemoveTreasureRates(Pickups pickup);
    void RemoveTreasureRates(string pickup);
}

internal class VersusTowerTreasurePatchContext : IVersusTowerTreasurePatchContext
{
    private int[] treasureRates;


    internal VersusTowerTreasurePatchContext(int[] treasureRates)
    {
        this.treasureRates = treasureRates;
    }

    #region TreasureRates
    /// <summary>
    /// Increase the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">An enum pickup to add or increase</param>
    /// <param name="rate">How many pickups in this tower will appear, default is 1</param>
    /// <returns>A context of this struct</returns>
    public void IncreaseTreasureRates(Pickups pickup, int rate = 1)
    {
        var currentRate = treasureRates[(int)pickup];
        treasureRates[(int)pickup] = currentRate + rate;
    }

    /// <summary>
    /// Increase the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">A string pickup to add or increase</param>
    /// <param name="rate">How many pickups in this tower will appear, default is 1</param>
    /// <returns>A context of this struct</returns>
    public void IncreaseTreasureRates(string pickup, int rate = 1)
    {
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        var currentRate = treasureRates[(int)enumPickup];
        treasureRates[(int)enumPickup] = currentRate + rate;
    }

    /// <summary>
    /// Decrease the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">A string pickup to decrease or completely removed if reached to 0</param>
    /// <param name="rate">How many rates of the pickup in this tower will be decrease, default is 1</param>
    /// <returns>A context of this struct</returns>
    public void DecreaseTreasureRates(string pickup, int rate = 1)
    {
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        var currentRate = treasureRates[(int)enumPickup];
        if (currentRate == 0)
        {
            return;
        }
        treasureRates[(int)enumPickup] = currentRate + rate;
    }

    /// <summary>
    /// Decrease the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">An enum pickup to decrease or completely removed if reached to 0</param>
    /// <param name="rate">How many rates of the pickup in this tower will be decrease, default is 1</param>
    /// <returns>A context of this struct</returns>
    public void DecreaseTreasureRates(Pickups pickup, int rate = 1)
    {
        var currentRate = treasureRates[(int)pickup];
        if (currentRate == 0)
        {
            return;
        }
        treasureRates[(int)pickup] = currentRate - rate;
    }

    /// <summary>
    /// Remove the treasure rate of a pickup in this tower. (Note that this will not be completely removed,
    /// if there is any mods that adding the same pickup as you want to remove them).
    /// </summary>
    /// <param name="pickup">An enum pickup to remove in this tower</param>
    /// <returns>A context of this struct</returns>
    public void RemoveTreasureRates(Pickups pickup)
    {
        treasureRates[(int)pickup] = 0;
    }

    /// <summary>
    /// Remove the treasure rate of a pickup in this tower. (Note that this will not be completely removed,
    /// if there is any mods that adding the same pickup as you want to remove them).
    /// </summary>
    /// <param name="pickup">A string pickup to remove in this tower</param>
    /// <returns>A context of this struct</returns>
    public void RemoveTreasureRates(string pickup)
    {
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        treasureRates[(int)enumPickup] = 0;
    }

    #endregion
}

public class DarkWorldTowerPatchContext 
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
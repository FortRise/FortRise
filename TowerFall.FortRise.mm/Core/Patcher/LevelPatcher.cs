using TowerFall;
using Monocle;
using System;

namespace FortRise;

/// <summary>
/// Patch Vanilla versus towers with this struct. 
/// If you want to patch outside of <see cref="ITowerPatcher.PatchTower(FortRise.OnTower)"/>,
/// you just create this struct and pass around your Fort Module.
/// </summary>
public struct OnTower 
{
    public OnTower(FortModule module) 
    {
        // Future use...
    }

    public VersusPatch VERSUS_SacredGround => new VersusPatch(0);
    public VersusPatch VERSUS_TwilightSpire => new VersusPatch(1);
    public VersusPatch VERSUS_Backfire => new VersusPatch(2);
    public VersusPatch VERSUS_Flight => new VersusPatch(3);
    public VersusPatch VERSUS_Mirage => new VersusPatch(4);
    public VersusPatch VERSUS_Thornwood => new VersusPatch(5);
    public VersusPatch VERSUS_FrostfangKeep => new VersusPatch(6);
    public VersusPatch VERSUS_KingsCourt => new VersusPatch(7);
    public VersusPatch VERSUS_SunkenCity => new VersusPatch(8);
    public VersusPatch VERSUS_Moonstone => new VersusPatch(9);
    public VersusPatch VERSUS_Towerforge => new VersusPatch(10);
    public VersusPatch VERSUS_Ascension => new VersusPatch(11);
    public VersusPatch VERSUS_TheAmaranth => new VersusPatch(12);
    public VersusPatch VERSUS_Dreadwood => new VersusPatch(13);
    public VersusPatch VERSUS_Darkfang => new VersusPatch(14);
    public VersusPatch VERSUS_Cataclysm => new VersusPatch(15);

    public void VERSUS_All(Action<VersusPatch> patchAll) 
    {
        for (int i = 0; i < 16; i++) 
        {
            var versusPatch = new VersusPatch(i);
            patchAll(versusPatch);
        }
    }
}

/// <summary>
/// An interface to directly inject the Tower Patcher code in Fort Modules.
/// </summary>
public interface ITowerPatcher 
{
    /// <summary>
    /// A method to perform the tower patching.
    /// </summary>
    /// <param name="tower">A struct containing the logic of tower patching</param>
    void PatchTower(OnTower tower);
}

/// <summary>
/// A struct that patch the versus towers. Please use this <see cref="ITowerPatcher.PatchTower(FortRise.OnTower)"/> 
/// to handle the patch.
/// </summary>
public struct VersusPatch 
{
    internal int LevelID;
    internal VersusPatch(int id) 
    {
        if (id >= GameData.VersusTowers.Count)
        {
            Logger.Warning("[Tower Patcher] Level ID does not exists!");
        }
        LevelID = id;
    }

    private bool IsSafe() 
    {
        if (LevelID >= GameData.VersusTowers.Count)
        {
            Logger.Error($"Cannot patch tower id: {LevelID} as it does not exists");
            return false;
        }
        return true;
    }

#region TreasureRates
    /// <summary>
    /// Increase the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">An enum pickup to add or increase</param>
    /// <param name="rate">How many pickups in this tower will appear, default is 1</param>
    /// <returns>A context of this struct</returns>
    public VersusPatch IncreaseTreasureRates(Pickups pickup, int rate = 1) 
    {
        if (!IsSafe()) return this;

        var tower = GameData.VersusTowers[LevelID];
        var currentRate = tower.TreasureMask[(int)pickup];
        tower.TreasureMask[(int)pickup] = currentRate + rate;
        return this;
    }

    /// <summary>
    /// Increase the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">A string pickup to add or increase</param>
    /// <param name="rate">How many pickups in this tower will appear, default is 1</param>
    /// <returns>A context of this struct</returns>
    public VersusPatch IncreaseTreasureRates(string pickup, int rate = 1) 
    {
        if (!IsSafe()) return this;

        var tower = GameData.VersusTowers[LevelID];
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        var currentRate = tower.TreasureMask[(int)enumPickup];
        tower.TreasureMask[(int)enumPickup] = currentRate + rate;
        return this;
    }

    /// <summary>
    /// Decrease the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">A string pickup to decrease or completely removed if reached to 0</param>
    /// <param name="rate">How many rates of the pickup in this tower will be decrease, default is 1</param>
    /// <returns>A context of this struct</returns>
    public VersusPatch DecreaseTreasureRates(string pickup, int rate = 1) 
    {
        if (!IsSafe()) return this;
        var tower = GameData.VersusTowers[LevelID];
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        var currentRate = tower.TreasureMask[(int)enumPickup];
        if (currentRate == 0)
            return this;
        tower.TreasureMask[(int)enumPickup] = currentRate + rate;
        return this;
    }

    /// <summary>
    /// Decrease the treasure rate of a pickup in this tower.
    /// </summary>
    /// <param name="pickup">An enum pickup to decrease or completely removed if reached to 0</param>
    /// <param name="rate">How many rates of the pickup in this tower will be decrease, default is 1</param>
    /// <returns>A context of this struct</returns>
    public VersusPatch DecreaseTreasureRates(Pickups pickup, int rate = 1) 
    {
        if (!IsSafe()) return this;
        var tower = GameData.VersusTowers[LevelID];
        var currentRate = tower.TreasureMask[(int)pickup];
        if (currentRate == 0)
            return this;
        tower.TreasureMask[(int)pickup] = currentRate - rate;
        return this;
    }

    /// <summary>
    /// Remove the treasure rate of a pickup in this tower. (Note that this will not be completely removed,
    /// if there is any mods that adding the same pickup as you want to remove them).
    /// </summary>
    /// <param name="pickup">An enum pickup to remove in this tower</param>
    /// <returns>A context of this struct</returns>
    public VersusPatch RemoveTreasureRates(Pickups pickup) 
    {
        if (!IsSafe()) return this;
        var tower = GameData.VersusTowers[LevelID];
        tower.TreasureMask[(int)pickup] = 0;
        return this;
    }

    /// <summary>
    /// Remove the treasure rate of a pickup in this tower. (Note that this will not be completely removed,
    /// if there is any mods that adding the same pickup as you want to remove them).
    /// </summary>
    /// <param name="pickup">A string pickup to remove in this tower</param>
    /// <returns>A context of this struct</returns>
    public VersusPatch RemoveTreasureRates(string pickup) 
    {
        if (!IsSafe()) return this;
        var tower = GameData.VersusTowers[LevelID];
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        tower.TreasureMask[(int)enumPickup] = 0;
        return this;
    }
#endregion
}
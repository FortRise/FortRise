using TowerFall;
using Monocle;

namespace FortRise;

public static class VanillaTowerPatcher
{
    public const int SACRED_GROUND = 0;
    public const int TWILIGHT_SPIRE = 1;
    public const int BACKFIRE = 2;
    public const int FLIGHT = 3;
    public const int MIRAGE = 4;
    public const int THORNWOOD = 5;
    public const int FROSTFANG_KEEP = 6;
    public const int KINGS_COURT = 7;
    public const int SUNKEN_CITY = 8;
    public const int MOONSTONE = 9;
    public const int TOWERFORGE = 10;
    public const int ASCENSION = 11;
    public const int THE_AMARANTH = 12;
    public const int DREADWOOD = 13;
    public const int DARKFANG = 14;
    public const int CATACLYSM = 15;

#region TreasureRates

    public static void IncreaseVersusTreasureRates(int id, Pickups pickup, int rate = 1) 
    {
        if (id >= GameData.VersusTowers.Count)
        {
            Logger.Error($"Cannot patch tower id: {id} as it does not exists");
            return;
        }
        var tower = GameData.VersusTowers[id];
        var currentRate = tower.TreasureMask[(int)pickup];
        tower.TreasureMask[(int)pickup] = currentRate + rate;
    }

    public static void IncreaseVersusTreasureRates(int id, string pickup, int rate = 1) 
    {
        if (id >= GameData.VersusTowers.Count)
        {
            Logger.Error($"Cannot patch tower id: {id} as it does not exists");
            return;
        }
        var tower = GameData.VersusTowers[id];
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        var currentRate = tower.TreasureMask[(int)enumPickup];
        tower.TreasureMask[(int)enumPickup] = currentRate + rate;
    }

    public static void DecreaseVersusTreasureRates(int id, string pickup, int rate = 1) 
    {
        if (id >= GameData.VersusTowers.Count)
        {
            Logger.Error($"Cannot patch tower id: {id} as it does not exists");
            return;
        }
        var tower = GameData.VersusTowers[id];
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        var currentRate = tower.TreasureMask[(int)enumPickup];
        if (currentRate == 0)
            return;
        tower.TreasureMask[(int)enumPickup] = currentRate + rate;
    }

    public static void DecreaseVersusTreasureRates(int id, Pickups pickups, int rate = 1) 
    {
        if (id >= GameData.VersusTowers.Count)
        {
            Logger.Error($"Cannot patch tower id: {id} as it does not exists");
            return;
        }
        var tower = GameData.VersusTowers[id];
        var currentRate = tower.TreasureMask[(int)pickups];
        if (currentRate == 0)
            return;
        tower.TreasureMask[(int)pickups] = currentRate - rate;
    }

    public static void IncreaseAllVersusTreasureRates(Pickups pickup, int rate = 1) 
    {
        for (int i = 0; i < GameData.VersusTowers.Count; i++) 
        {
            IncreaseVersusTreasureRates(i, pickup, rate);
        }
    }

    public static void DecreaseAllVersusTreasureRates(Pickups pickup, int rate = 1) 
    {
        for (int i = 0; i < GameData.VersusTowers.Count; i++) 
        {
            DecreaseVersusTreasureRates(i, pickup, rate);
        }
    }

    public static void IncreaseAllVersusTreasureRates(string pickup, int rate = 1) 
    {
        for (int i = 0; i < GameData.VersusTowers.Count; i++) 
        {
            IncreaseVersusTreasureRates(i, pickup, rate);
        }
    }

    public static void DecreaseAllVersusTreasureRates(string pickup, int rate = 1) 
    {
        for (int i = 0; i < GameData.VersusTowers.Count; i++) 
        {
            DecreaseVersusTreasureRates(i, pickup, rate);
        }
    }

    public static void RemoveAllVersusTreasureRates(string pickup) 
    {
        for (int i = 0; i < GameData.VersusTowers.Count; i++) 
        {
            RemoveVersusTreasureRates(i, pickup);
        }
    }

    public static void RemoveAllVersusTreasureRates(Pickups pickup) 
    {
        for (int i = 0; i < GameData.VersusTowers.Count; i++) 
        {
            RemoveVersusTreasureRates(i, pickup);
        }
    }

    public static void RemoveVersusTreasureRates(int id, Pickups pickup) 
    {
        if (id >= GameData.VersusTowers.Count)
        {
            Logger.Error($"Cannot patch tower id: {id} as it does not exists");
            return;
        }
        var tower = GameData.VersusTowers[id];
        tower.TreasureMask[(int)pickup] = 0;
    }

    public static void RemoveVersusTreasureRates(int id, string pickup) 
    {
        if (id >= GameData.VersusTowers.Count)
        {
            Logger.Error($"Cannot patch tower id: {id} as it does not exists");
            return;
        }
        var tower = GameData.VersusTowers[id];
        var enumPickup = Calc.StringToEnum<Pickups>(pickup);
        tower.TreasureMask[(int)enumPickup] = 0;
    }
#endregion
}

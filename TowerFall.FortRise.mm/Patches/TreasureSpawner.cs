using System;
using FortRise;

namespace TowerFall;

public class patch_TreasureSpawner : TreasureSpawner
{
    public static float[][] ChestChances;
    public static float[] DefaultTreasureChances;
    public static int[] FullTreasureMask;
    public static bool[] DarkWorldTreasures;


    public patch_TreasureSpawner(Session session, VersusTowerData versusTowerData) : base(session, versusTowerData)
    {
    }

    internal static void ExtendTreasures() 
    {
        var treasureCount = 21 + RiseCore.PickupRegistry.Count;
        // We don't need to resize, if left unchanged
        if (treasureCount == 21)
            return;
        Array.Resize(ref DefaultTreasureChances, treasureCount);
        Array.Resize(ref FullTreasureMask, treasureCount);
        Array.Resize(ref DarkWorldTreasures, treasureCount);
        // We don't want gem to spawn at all
        DefaultTreasureChances[20] = 0;
        FullTreasureMask[20] = 0;

        // Put every customs pickups including the arrows to be in the treasure pools
        foreach (var pickup in RiseCore.PickupRegistry.Values) 
        {
            var id = pickup.ID;
            var chance = pickup.Chance;
            DefaultTreasureChances[(int)id] = chance;
            FullTreasureMask[(int)id] = 1;
        }
    }
}
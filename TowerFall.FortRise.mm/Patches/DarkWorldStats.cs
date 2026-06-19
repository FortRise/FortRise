using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.DarkWorldStats")]
public class DarkWorldStats : TowerFall.DarkWorldStats
{
    public int TotalGoldEyes
    {
        get
        {
            int goldEyes = 0;
            foreach (DarkWorldTowerStats darkWorldTowerStats in Towers)
            {
                if (darkWorldTowerStats.EarnedGoldEye)
                {
                    goldEyes++;
                }
            }
            return goldEyes;
        }
    }

    [MonoModReplace]
    public void Verify()
    {
        // Backup the data from savedata to mod if possible
        // this should only be possible if the mod data exists on the TowerFall savedata side
        for (int i = 0; i < Towers.Length; i += 1)
        {
            if (Towers[i] is not TowerFall.Patching.DarkWorldTowerStats tower || tower.LevelID is null)
            {
                continue;
            }

            if (!tower.LevelID.Contains('/')) // only selects the level with slash for backup
            {
                continue;
            }

            if (FortRiseModule.SaveData.AdventureWorld.Towers.ContainsKey(tower.LevelID))
            {
                // do not backup if it already exists
                continue;
            }

            FortRiseModule.SaveData.AdventureWorld.Towers[tower.LevelID] = tower;
        }


        Towers = Towers.VerifyLength(GameData.DarkWorldTowers.Count);
        for (int i = 0; i < Towers.Length; i++)
        {
            if (Towers[i] == null)
            {
                Towers[i] = new DarkWorldTowerStats();
            }
        }

        Towers[0].Revealed = true;
        Towers[1].Revealed = true;
        Towers[2].Revealed = true;

        var restoreIndex = new Dictionary<string, int>();

        // Assign the LevelID for the SaveData
        for (int i = 0; i < GameData.DarkWorldTowers.Count; i += 1)
        {
            var dw = GameData.DarkWorldTowers[i];
            DarkWorldTowerStats stats = Towers[dw.ID.X] as DarkWorldTowerStats;
            stats.LevelID = dw.LevelID;

            if (dw.LevelID.Contains('/')) // only selects the level with slash for restoration
            {
                restoreIndex[dw.LevelID] = i;
            }
        }

        // Restore the data from mod to savedata if possible

        foreach (var (k, v) in TowerRegistry.DarkWorldTowers)
        {
            ref var stats = ref CollectionsMarshal.GetValueRefOrNullRef(FortRiseModule.SaveData.AdventureWorld.Towers, v.ID);
            if (Unsafe.IsNullRef(ref stats))
            {
                // cannot find the stats for the level
                continue;
            }

            var level = restoreIndex[k];
            Towers[level] = stats;

            restoreIndex.Remove(k);
        }
    }
}

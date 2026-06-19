using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.TrialsStats")]
public class TrialsStats : TowerFall.TrialsStats
{
    [MonoModReplace]
    public void Verify()
    {
        // Backup the data from savedata to mod if possible
        // this should only be possible if the mod data exists on the TowerFall savedata side
        for (int i = 0; i < Levels.Length; i += 1)
        {
            var level = Levels[i];

            for (int j = 0; j < level.Length; j += 1)
            {
                var stats = Unsafe.BitCast<TowerFall.TrialsLevelStats, TowerFall.Patching.TrialsLevelStats>(level[j]);
                if (stats.LevelID is null)
                {
                    continue;
                }

                if (!GameData.TrialsLevels[i, j].LevelID.Contains('/')) // only selects the level with slash for backup
                {
                    continue;
            }

                if (FortRiseModule.SaveData.AdventureTrials.Towers.ContainsKey(stats.LevelID))
                {
                    // do not backup if it already exists
                    continue;
                }

                FortRiseModule.SaveData.AdventureTrials.Towers[stats.LevelID] = stats;
            }
        }

        Levels = Levels.VerifyLength(GameData.TrialsLevels.GetLength(0), GameData.TrialsLevels.GetLength(1));

        var restoreIndex = new Dictionary<string, (int, int)>();

        // Assign the LevelID for the SaveData
        for (int i = 0; i < Levels.Length; i += 1)
        {
            var levels = Levels[i];
            for (int j = 0; j < levels.Length; j += 1)
            {
                var tl = GameData.TrialsLevels[i, j];
                TrialsLevelStats stats = Unsafe.BitCast<TowerFall.TrialsLevelStats, TowerFall.Patching.TrialsLevelStats>(Levels[tl.ID.X][tl.ID.Y]);

                stats.LevelID = tl.LevelID;
                Levels[tl.ID.X][tl.ID.Y] = Unsafe.BitCast<TowerFall.Patching.TrialsLevelStats, TowerFall.TrialsLevelStats>(stats);

                if (tl.LevelID.Contains('/')) // only selects the level with slash for restoration
                {
                    restoreIndex[tl.LevelID] = (i, j);
                }
            }
        }

        // Restore the data from mod to savedata if possible

        foreach (var (k, v) in TowerRegistry.TrialTowers)
        {
            for (int i = 0; i < 3; i += 1)
            {
                var id = i switch
                {
                    0 => v.TrialsLevelDataTier1.LevelID,
                    1 => v.TrialsLevelDataTier2.LevelID,
                    2 => v.TrialsLevelDataTier3.LevelID,
                    _ => throw new NotSupportedException()
                };

                ref var stats = ref CollectionsMarshal.GetValueRefOrNullRef(FortRiseModule.SaveData.AdventureTrials.Towers, id);

                if (Unsafe.IsNullRef(ref stats))
                {
                    // cannot find the stats for the level
                    continue;
                }


                var (x, y) = restoreIndex[id];
                Levels[x][y] = Unsafe.BitCast<TowerFall.Patching.TrialsLevelStats, TowerFall.TrialsLevelStats>(stats);

                restoreIndex.Remove(k);
            }
        }
    }
}

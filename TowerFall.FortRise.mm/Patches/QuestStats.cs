using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.QuestStats")]
public class QuestStats : TowerFall.QuestStats
{
    [MonoModReplace]
    public void Verify()
    {
        // Backup the data from savedata to mod if possible
        // this should only be possible if the mod data exists on the TowerFall savedata side
        for (int i = 0; i < Towers.Length; i += 1)
        {
            if (Towers[i] is not QuestTowerStats tower || tower.LevelID is null)
            {
                continue;
            }

            if (!tower.LevelID.Contains('/')) // only selects the level with slash for backup
            {
                continue;
            }

            if (FortRiseModule.SaveData.AdventureQuest.Towers.ContainsKey(tower.LevelID))
            {
                // do not backup if it already exists
                continue;
            }

            FortRiseModule.SaveData.AdventureQuest.Towers[tower.LevelID] = tower;
        }

        Towers = Towers.VerifyLength(GameData.QuestLevels.Length);
        for (int i = 0; i < Towers.Length; i++)
        {
            if (Towers[i] == null)
            {
                Towers[i] = new QuestTowerStats();
            }
        }
        Towers[0].Revealed = true;

        var restoreIndex = new Dictionary<string, int>();

        // Assign the LevelID for the SaveData
        for (int i = 0; i < GameData.QuestLevels.Length; i += 1)
        {
            var ql = GameData.QuestLevels[i];
            QuestTowerStats stats = Towers[ql.ID.X] as QuestTowerStats;
            stats.LevelID = ql.LevelID;

            if (ql.LevelID.Contains('/')) // only selects the level with slash for restoration
            {
                restoreIndex[ql.LevelID] = i;
            }
        }

        // Restore the data from mod to savedata if possible
        foreach (var (k, v) in TowerRegistry.QuestTowers)
        {
            ref var stats = ref CollectionsMarshal.GetValueRefOrNullRef(FortRiseModule.SaveData.AdventureQuest.Towers, v.ID);
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
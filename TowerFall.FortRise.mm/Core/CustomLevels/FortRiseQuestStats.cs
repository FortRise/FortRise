using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace FortRise;

public sealed partial class FortRiseQuestStats 
{
    [JsonInclude]
    public Dictionary<string, TowerFall.Patching.QuestTowerStats> Towers = new Dictionary<string, TowerFall.Patching.QuestTowerStats>();

    public TowerFall.Patching.QuestTowerStats AddOrGet(string name) 
    {
        ref var stats = ref CollectionsMarshal.GetValueRefOrAddDefault(Towers, name, out bool exists);
        if (!exists) 
        {
            stats = new TowerFall.Patching.QuestTowerStats();
        }

        stats.LevelID ??= name;
        return stats;
    }
}

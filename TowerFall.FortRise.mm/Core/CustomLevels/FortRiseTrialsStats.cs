using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace FortRise;

public sealed partial class FortRiseTrialsStats 
{
    [JsonInclude]
    public Dictionary<string, TowerFall.Patching.TrialsLevelStats> Towers = new Dictionary<string, TowerFall.Patching.TrialsLevelStats>();

    public TowerFall.Patching.TrialsLevelStats AddOrGet(string name) 
    {
        ref var stats = ref CollectionsMarshal.GetValueRefOrAddDefault(Towers, name, out bool exists);
        if (!exists)
        {
            stats = new TowerFall.Patching.TrialsLevelStats();
        }

        stats.LevelID ??= name;
        return stats;
    }
}

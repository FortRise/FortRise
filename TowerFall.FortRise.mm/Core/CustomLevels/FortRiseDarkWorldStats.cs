using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace FortRise;

public sealed partial class FortRiseDarkWorldStats 
{
    [JsonInclude]
    public Dictionary<string, TowerFall.Patching.DarkWorldTowerStats> Towers = new Dictionary<string, TowerFall.Patching.DarkWorldTowerStats>();

    public TowerFall.DarkWorldTowerStats AddOrGet(string name) 
    {
        ref var stats = ref CollectionsMarshal.GetValueRefOrAddDefault(Towers, name, out bool exists);
        if (!exists) 
        {
            stats = new TowerFall.Patching.DarkWorldTowerStats();
        }

        stats.LevelID ??= name;

        return stats;
    }
}

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace FortRise;

public sealed partial class FortRiseDarkWorldStats 
{
    [JsonInclude]
    public Dictionary<string, FortRiseDarkWorldTowerStats> Towers = new Dictionary<string, FortRiseDarkWorldTowerStats>();

    public FortRiseDarkWorldTowerStats AddOrGet(string name) 
    {
        ref var stats = ref CollectionsMarshal.GetValueRefOrAddDefault(Towers, name, out bool exists);
        if (!exists) 
        {
            stats = new FortRiseDarkWorldTowerStats();
        }
        return stats;
    }
}

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace FortRise;

public sealed partial class FortRiseQuestStats 
{
    [JsonInclude]
    public Dictionary<string, FortRiseQuestTowerStats> Towers = new Dictionary<string, FortRiseQuestTowerStats>();

    public FortRiseQuestTowerStats AddOrGet(string name) 
    {
        ref var stats = ref CollectionsMarshal.GetValueRefOrAddDefault(Towers, name, out bool exists);
        if (!exists) 
        {
            stats = new FortRiseQuestTowerStats();
        }
        return stats;
    }
}

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace FortRise;

public sealed partial class FortRiseTrialsStats 
{
    [JsonInclude]
    public Dictionary<string, FortRiseTrialsTowerStats> Towers = new Dictionary<string, FortRiseTrialsTowerStats>();

    public FortRiseTrialsTowerStats AddOrGet(string name) 
    {
        ref var stats = ref CollectionsMarshal.GetValueRefOrAddDefault(Towers, name, out bool exists);
        if (!exists)
        {
            stats = new FortRiseTrialsTowerStats();
        }
        return stats;
    }
}

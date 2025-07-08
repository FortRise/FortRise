using System.Collections.Generic;
using System.Text.Json.Serialization;
using TowerFall;

namespace FortRise;

public class FortRiseModuleSaveData : ModuleSaveData
{
    [JsonPropertyName("World")]
    [JsonInclude]
    public FortRiseDarkWorldStats AdventureWorld = new FortRiseDarkWorldStats();
    [JsonPropertyName("Quest")]
    [JsonInclude]
    public FortRiseQuestStats AdventureQuest = new FortRiseQuestStats();
    [JsonPropertyName("Trials")]
    [JsonInclude]
    public FortRiseTrialsStats AdventureTrials = new FortRiseTrialsStats();
    [JsonPropertyName("Locations")]
    [JsonInclude]
    public List<string> LevelLocations = new List<string>();
}
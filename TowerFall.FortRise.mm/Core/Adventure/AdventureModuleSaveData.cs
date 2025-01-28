using System.Collections.Generic;
using System.Text.Json.Serialization;
using TowerFall;

namespace FortRise.Adventure;

public class AdventureModuleSaveData : ModuleSaveData
{
    [JsonPropertyName("World")]
    [JsonInclude]
    public AdventureWorldStats AdventureWorld = new AdventureWorldStats();
    [JsonPropertyName("Quest")]
    [JsonInclude]
    public AdventureQuestStats AdventureQuest = new AdventureQuestStats();
    [JsonPropertyName("Trials")]
    [JsonInclude]
    public AdventureTrialsStats AdventureTrials = new AdventureTrialsStats();
    [JsonPropertyName("Locations")]
    [JsonInclude]
    public List<string> LevelLocations = new List<string>();
}
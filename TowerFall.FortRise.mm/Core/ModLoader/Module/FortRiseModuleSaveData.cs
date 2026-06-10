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

    [JsonPropertyName("VariantPresets")]
    [JsonInclude]
    public List<CustomVariantPreset> VariantPresets = [];
}

public sealed class CustomVariantPreset
{
    [JsonPropertyName("Name")]
    [JsonInclude]
    public string Name;

    [JsonPropertyName("Variants")]
    [JsonInclude]
    public List<string> Variants = [];

    [JsonPropertyName("Color")]
    [JsonInclude]
    public string Color = "#FFFFFF";
}
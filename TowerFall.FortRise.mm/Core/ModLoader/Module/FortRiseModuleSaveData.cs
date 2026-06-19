using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

    public override void OnVerify()
    {
        foreach (var (_, v) in TowerRegistry.QuestTowers)
        {
            ref var stats = ref CollectionsMarshal.GetValueRefOrAddDefault(AdventureQuest.Towers, v.ID, out bool exists);
            if (!exists) 
            {
                stats = new TowerFall.Patching.QuestTowerStats
                {
                    LevelID = v.ID
                };
            }
        }

        foreach (var (_, v) in TowerRegistry.DarkWorldTowers)
        {
            ref var stats = ref CollectionsMarshal.GetValueRefOrAddDefault(AdventureWorld.Towers, v.ID, out bool exists);
            if (!exists) 
            {
                stats = new TowerFall.Patching.DarkWorldTowerStats
                {
                    LevelID = v.ID
                };
            }
        }

        foreach (var (_, v) in TowerRegistry.TrialTowers)
        {
            for (int i = 0; i < 3; i += 1)
            {
                var id = i switch
                {
                    0 => v.TrialsLevelDataTier1.LevelID,
                    1 => v.TrialsLevelDataTier2.LevelID,
                    2 => v.TrialsLevelDataTier3.LevelID,
                    _ => throw new NotSupportedException()
                };

                ref var stats = ref CollectionsMarshal.GetValueRefOrAddDefault(AdventureTrials.Towers, id, out bool exists);

                if (!exists) 
                {
                    stats = new TowerFall.Patching.TrialsLevelStats
                    {
                        LevelID = id
                    };
                }
            }

        }
    }
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
using System.Collections.Generic;
using TeuJson;
using TowerFall;

namespace FortRise.Adventure;

public class AdventureModuleSaveData : ModuleSaveData
{
    public AdventureWorldStats AdventureWorld = new AdventureWorldStats();
    public AdventureQuestStats AdventureQuest = new AdventureQuestStats();
    public List<string> LevelLocations = new();

    public AdventureModuleSaveData() : base(new JsonSaveDataFormat())
    {
    }

    public override void Load(SaveDataFormat formatter)
    {
        var obj = formatter.CastTo<JsonSaveDataFormat>().GetJsonObject();
        AdventureWorld = JsonConvert.Deserialize<AdventureWorldStats>(obj["World"]) ?? new AdventureWorldStats();
        AdventureQuest = JsonConvert.Deserialize<AdventureQuestStats>(obj["Quest"]) ?? new AdventureQuestStats();
        LevelLocations = obj["Locations"].ConvertToListString() ?? new List<string>();
    }

    public override ClosedFormat Save(FortModule fortModule)
    {
        var world = JsonConvert.Serialize(AdventureWorld);
        var quest = JsonConvert.Serialize(AdventureQuest);

        var jsonObj = new JsonObject 
        {
            ["World"] = world,
            ["Quest"] = quest,
            ["Locations"] = LevelLocations.ConvertToJsonArray()
        };
        return Formatter.Close(jsonObj);
    }
}
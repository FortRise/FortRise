using System.Collections.Generic;
using TeuJson;
using TowerFall;

namespace FortRise.Adventure;

public class AdventureModuleSaveData : ModuleSaveData
{
    public AdventureWorldStats AdventureWorld = new AdventureWorldStats();
    public List<string> LevelLocations = new();

    public AdventureModuleSaveData() : base(new JsonSaveDataFormat())
    {
    }

    public override void Load(SaveDataFormat formatter)
    {
        var obj = formatter.CastTo<JsonSaveDataFormat>().GetJsonObject();
        AdventureWorld = JsonConvert.Deserialize<AdventureWorldStats>(obj["World"]);
        LevelLocations = obj["Locations"].ConvertToListString() ?? new List<string>();
    }

    public override ClosedFormat Save(FortModule fortModule)
    {
        var serialized = JsonConvert.Serialize(AdventureWorld);

        var jsonObj = new JsonObject 
        {
            ["World"] = serialized,
            ["Locations"] = LevelLocations.ConvertToJsonArray()
        };
        return Formatter.Close(jsonObj);
    }
}
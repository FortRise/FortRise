using System.Text.Json.Serialization;
using System.Xml.Serialization;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.QuestTowerStats")]
public class QuestTowerStats : TowerFall.QuestTowerStats
{
    [JsonInclude]
    public bool Revealed;
    
    [JsonInclude]
    public bool CompletedNormal;
    
    [JsonInclude]
    public bool CompletedHardcore;
    
    [JsonInclude]
    public bool CompletedNoDeaths;
    
    [JsonInclude]
    public long Best1PTime;
    
    [JsonInclude]
    public long Best2PTime;
    
    [JsonInclude]
    public ulong TotalDeaths;
    
    [JsonInclude]
    public ulong TotalAttempts;

    [XmlAttribute]
    [JsonInclude]
    public string LevelID;
}

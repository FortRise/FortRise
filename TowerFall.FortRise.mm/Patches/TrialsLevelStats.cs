using System.Text.Json.Serialization;
using System.Xml.Serialization;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.TrialsLevelStats")]
public struct TrialsLevelStats
{
    [JsonInclude]
    public bool UnlockedGold;

    [JsonInclude]
    public bool UnlockedDiamond;

    [JsonInclude]
    public bool UnlockedDevTime;

    [JsonInclude]
    public long BestTime;

    [JsonInclude]
    public ulong Attempts;

    [XmlAttribute]
    [JsonInclude]
    public string LevelID;

    [JsonIgnore]
    public int NextGoal
    {
        [MonoModIgnore]
        get
        {
            throw null;
        }
    }
}

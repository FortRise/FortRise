using System.Text.Json.Serialization;
using System.Xml.Serialization;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.DarkWorldTowerStats")]
public class DarkWorldTowerStats : TowerFall.DarkWorldTowerStats
{
    [JsonInclude]
    public bool Revealed;

    [JsonInclude]
    public bool CompletedNormal;

    [JsonInclude]
    public bool CompletedHardcore;

    [JsonInclude]
    public bool CompletedLegendary;

    [JsonInclude]
    public bool EarnedEye;

    [JsonInclude]
    public bool EarnedGoldEye;

    [JsonInclude]
    public long Best1PTime;

    [JsonInclude]
    public long Best2PTime;

    [JsonInclude]
    public long Best3PTime;

    [JsonInclude]
    public long Best4PTime;

    [JsonInclude]
    public int Most1PCurses;

    [JsonInclude]
    public int Most2PCurses;

    [JsonInclude]
    public int Most3PCurses;

    [JsonInclude]
    public int Most4PCurses;
    [JsonInclude]
    public ulong Deaths;

    [JsonInclude]
    public ulong Attempts;

    [XmlAttribute]
    [JsonInclude]
    public string LevelID;
}
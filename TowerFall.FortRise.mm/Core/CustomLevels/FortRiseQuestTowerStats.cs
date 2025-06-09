using System.Text.Json.Serialization;
using TowerFall;

namespace FortRise;

public partial class FortRiseQuestTowerStats : QuestTowerStats
{
    [JsonInclude]
    public bool Revealed
    {
        get => base.Revealed;
        set => base.Revealed = value;
    }
    [JsonInclude]
    public bool CompletedNormal
    {
        get => base.CompletedNormal;
        set => base.CompletedNormal = value;
    }
    [JsonInclude]
    public bool CompletedHardcore
    {
        get => base.CompletedHardcore;
        set => base.CompletedHardcore = value;
    }
    [JsonInclude]
    public bool CompletedNoDeaths
    {
        get => base.CompletedNoDeaths;
        set => base.CompletedNoDeaths = value;
    }
    [JsonInclude]
    public long Best1PTime
    {
        get => base.Best1PTime;
        set => base.Best1PTime = value;
    }
    [JsonInclude]
    public long Best2PTime
    {
        get => base.Best2PTime;
        set => base.Best2PTime = value;
    }
    [JsonInclude]
    public ulong TotalDeaths
    {
        get => base.TotalDeaths;
        set => base.TotalDeaths = value;
    }
    [JsonInclude]
    public ulong TotalAttempts
    {
        get => base.TotalAttempts;
        set => base.TotalAttempts = value;
    }
}

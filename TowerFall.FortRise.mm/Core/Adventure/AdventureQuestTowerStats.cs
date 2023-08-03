using System;
using System.Collections.Generic;
using System.Text;
using TeuJson;
using TeuJson.Attributes;
using TowerFall;

namespace FortRise.Adventure;

public sealed partial class AdventureQuestStats : IDeserialize, ISerialize 
{
    [TeuObject]
    public Dictionary<string, AdventureQuestTowerStats> Towers = new Dictionary<string, AdventureQuestTowerStats>();

    public AdventureQuestTowerStats AddOrGet(string name) 
    {
        Towers ??= new Dictionary<string, AdventureQuestTowerStats>();
        if (Towers.TryGetValue(name, out AdventureQuestTowerStats stats)) 
        {
            return stats;
        }
        var newStats = new AdventureQuestTowerStats();
        Towers.Add(name, newStats);
        return newStats;
    }
}

public partial class AdventureQuestTowerStats : QuestStats, IDeserialize, ISerialize
{
    [TeuObject]
    public bool Revealed;
    [TeuObject]
    public bool CompletedNormal;
    [TeuObject]
    public bool CompletedHardcore;
    [TeuObject]
    public bool CompletedNoDeaths;
    [TeuObject]
    public long Best1PTime;
    [TeuObject]
    public long Best2PTime;
    [TeuObject]
    public ulong TotalDeaths;
    [TeuObject]
    public ulong TotalAttempts;


    public void BeatNormal()
    {
        CompletedNormal = true;
    }

    public void BeatHardcore(int players, long time, bool noDeaths)
    {
        CompletedNormal = (CompletedHardcore = true);
        if (noDeaths)
        {
            CompletedNoDeaths = true;
        }
        if (players == 1)
        {
            if (Best1PTime == 0L)
            {
                Best1PTime = time;
                return;
            }
            Best1PTime = Math.Min(Best1PTime, time);
            return;
        }
        else
        {
            if (Best2PTime == 0L)
            {
                Best2PTime = time;
                return;
            }
            Best2PTime = Math.Min(this.Best2PTime, time);
            return;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Monocle;
using TowerFall;

namespace FortRise.Adventure;

public sealed partial class AdventureQuestStats 
{
    [JsonInclude]
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

public partial class AdventureQuestTowerStats
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


public sealed partial class AdventureTrialsStats 
{
    [JsonInclude]
    public Dictionary<string, AdventureTrialsTowerStats> Towers = new Dictionary<string, AdventureTrialsTowerStats>();

    public AdventureTrialsTowerStats AddOrGet(string name) 
    {
        Towers ??= new Dictionary<string, AdventureTrialsTowerStats>();
        if (Towers.TryGetValue(name, out AdventureTrialsTowerStats stats)) 
        {
            return stats;
        }
        var newStats = new AdventureTrialsTowerStats();
        Towers.Add(name, newStats);
        return newStats;
    }
}

public partial class AdventureTrialsTowerStats 
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

    [JsonIgnore]
    public bool this[int index]
    {
        get
        {
            switch (index)
            {
            case 0:
                return this.UnlockedGold;
            case 1:
                return this.UnlockedDiamond;
            case 2:
                return this.UnlockedDevTime;
            default:
                throw new Exception("Index out of range!");
            }
        }
        set
        {
            switch (index)
            {
            case 0:
                this.UnlockedGold = value;
                return;
            case 1:
                this.UnlockedDiamond = value;
                return;
            case 2:
                this.UnlockedDevTime = value;
                return;
            default:
                throw new Exception("Index out of range!");
            }
        }
    }

    [JsonIgnore]
    public int NextGoal
    {
        get
        {
            for (int i = 0; i < 2; i++)
            {
                if (!this[i])
                {
                    return i;
                }
            }
            return -1;
        }
    }

    public int CalculateChecksum(int salt, long timePlayed)
    {
        salt += (int)(timePlayed % 933L);
        return 0 + (int)(this.Attempts % (ulong)(9L + (long)salt)) * 23 + Calc.Count<bool>(true, this.UnlockedGold, this.UnlockedDiamond, this.UnlockedDevTime) * (salt + 2) * 7 + (int)(this.BestTime % (1113L + (long)salt)) * 7;
    }

    public Sprite<int> GetAwardIcon()
    {
        if (this.UnlockedDevTime)
        {
            return TFGame.MenuSpriteData.GetSpriteInt("DevTime");
        }
        if (this.UnlockedDiamond)
        {
            return TFGame.MenuSpriteData.GetSpriteInt("Diamond");
        }
        if (this.UnlockedGold)
        {
            return TFGame.MenuSpriteData.GetSpriteInt("Gold");
        }
        return null;
    }

    public Sprite<int> GetSmallAwardIcon()
    {
        if (this.UnlockedDevTime)
        {
            return TFGame.MenuSpriteData.GetSpriteInt("DevTimeSmall");
        }
        if (this.UnlockedDiamond)
        {
            return TFGame.MenuSpriteData.GetSpriteInt("DiamondSmall");
        }
        if (this.UnlockedGold)
        {
            return TFGame.MenuSpriteData.GetSpriteInt("GoldSmall");
        }
        return null;
    }

    public Sprite<int> GetNextAwardIcon()
    {
        if (this.UnlockedDiamond)
        {
            return null;
        }
        if (this.UnlockedGold)
        {
            return TFGame.MenuSpriteData.GetSpriteInt("Diamond");
        }
        return TFGame.MenuSpriteData.GetSpriteInt("Gold");
    }

    public Sprite<int> GetNextSmallAwardIcon()
    {
        if (this.UnlockedDiamond)
        {
            return null;
        }
        if (this.UnlockedGold)
        {
            return TFGame.MenuSpriteData.GetSpriteInt("DiamondSmall");
        }
        return TFGame.MenuSpriteData.GetSpriteInt("GoldSmall");
    }
	
}
using System;
using System.Text.Json.Serialization;
using Monocle;
using TowerFall;

namespace FortRise;

// can't extends this from TrialsLevelStats since is a struct for gosh sake..
public partial class FortRiseTrialsTowerStats 
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
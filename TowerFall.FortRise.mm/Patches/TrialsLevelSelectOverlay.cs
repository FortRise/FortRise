using FortRise;
using Microsoft.Xna.Framework;
using Monocle;

namespace TowerFall;

public class patch_TrialsLevelSelectOverlay : TrialsLevelSelectOverlay
{
    private MapScene map;
    private Point statsID;
    private Sprite<int> levelMedalIcon;
    private Sprite<int> diamondIcon;
    private Sprite<int> goldIcon;
    private Sprite<int> devIcon;
    private string totalTimeString;
    private string totalGoldsString;
    private string totalDiamondsString;
    private string totalDevtimesString;
    private float drawStatsLerp;
    private string levelBestTimeString;
    private string levelAttemptsString;

    public patch_TrialsLevelSelectOverlay(MapScene map) : base(map)
    {
    }

    public extern void orig_Update();

    public override void Update()
    {
        if (map.Selection is TrialsMapButton)
            orig_Update();
    }

    private extern void orig_RefreshLevelStats();

    private void RefreshLevelStats()
    {
        var levelSet = map.GetLevelSet();
        long bestTime;
        bool unlockedDevTime;
        bool unlockedDiamond;
        bool unlockedGold;
        ulong attempts;
        if (levelSet == "TowerFall")
        {
            var trialsLevelStats = SaveData.Instance.Trials.Levels[statsID.X][statsID.Y];
            bestTime = trialsLevelStats.BestTime;
            unlockedDevTime = trialsLevelStats.UnlockedDevTime;
            unlockedDiamond = trialsLevelStats.UnlockedDiamond;
            unlockedGold = trialsLevelStats.UnlockedGold;
            attempts = trialsLevelStats.Attempts;
        }
        else
        {
            var trialsLevelStats = FortRiseModule.SaveData.AdventureTrials.AddOrGet(TowerRegistry.TrialsGet(levelSet, statsID.X, statsID.Y).GetLevelID());
            bestTime = trialsLevelStats.BestTime;
            unlockedDevTime = trialsLevelStats.UnlockedDevTime;
            unlockedDiamond = trialsLevelStats.UnlockedDiamond;
            unlockedGold = trialsLevelStats.UnlockedGold;
            attempts = trialsLevelStats.Attempts;
        }
        if (bestTime == 0L)
        {
            levelBestTimeString = "";
        }
        else
        {
            levelBestTimeString = TrialsResults.GetTimeString(bestTime);
            if (unlockedDevTime)
            {
                levelMedalIcon = devIcon;
            }
            else if (unlockedDiamond)
            {
                levelMedalIcon = diamondIcon;
            }
            else if (unlockedGold)
            {
                levelMedalIcon = goldIcon;
            }
        }
        if (attempts == 0UL)
        {
            levelAttemptsString = "";
            return;
        }
        levelAttemptsString = attempts.ToString();
    }
}
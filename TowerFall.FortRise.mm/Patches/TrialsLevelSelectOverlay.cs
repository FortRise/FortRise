using FortRise.Adventure;
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
        if (map.Selection is TrialsMapButton or AdventureMapButton)
            orig_Update();
    }

    private extern void orig_RefreshLevelStats();

    private void RefreshLevelStats()
    {
        var levelSet = map.GetLevelSet();
        if (levelSet == "TowerFall")
        {
            orig_RefreshLevelStats();
            return;
        }
        var trialsLevelStats = ((AdventureTrialsTowerData[])TowerRegistry.TrialsGet(levelSet, statsID.X))[statsID.Y].Stats;
        if (trialsLevelStats.BestTime == 0L)
        {
            levelBestTimeString = "";
        }
        else
        {
            levelBestTimeString = TrialsResults.GetTimeString(trialsLevelStats.BestTime);
            if (trialsLevelStats.UnlockedDevTime)
            {
                levelMedalIcon = devIcon;
            }
            else if (trialsLevelStats.UnlockedDiamond)
            {
                levelMedalIcon = diamondIcon;
            }
            else if (trialsLevelStats.UnlockedGold)
            {
                levelMedalIcon = goldIcon;
            }
        }
        if (trialsLevelStats.Attempts == 0UL)
        {
            levelAttemptsString = "";
            return;
        }
        levelAttemptsString = trialsLevelStats.Attempts.ToString();
    }
}
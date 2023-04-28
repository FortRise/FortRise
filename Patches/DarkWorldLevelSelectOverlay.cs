#pragma warning disable CS0626
#pragma warning disable CS0108

using MonoMod;

namespace TowerFall;

public class patch_DarkWorldLevelSelectOverlay : DarkWorldLevelSelectOverlay
{
    private int statsID;
    private MapScene map;

    private string levelAttemptsString;
    private string levelDeathsString;
    private string levelBestTimeString;
    private string levelCursesString;

    public patch_DarkWorldLevelSelectOverlay(MapScene map) : base(map)
    {
    }

    [MonoModIgnore]
    [MonoModConstructor]
    [PatchDarkWorldLevelSelectOverlayCtor]
    public extern void ctor(MapScene map);
    

    public extern void orig_Update();

    private void CheckUpdate() 
    {
        if (map.Selection is DarkWorldMapButton or AdventureMapButton)
            orig_Update();
    }

    public override void Update()
    {
        CheckUpdate();
    }

    private extern void orig_RefreshLevelStats();
    
    private void RefreshLevelStats() 
    {
        if (!patch_SaveData.AdventureActive)
        {
            orig_RefreshLevelStats();
            return;
        }
        AdventureWorldTowerStats adventureWorldTowerStats = patch_GameData.AdventureWorldTowers[statsID].Stats;
        long num;
        int num2;
        switch (TFGame.PlayerAmount)
        {
        default:
            num = adventureWorldTowerStats.Best1PTime;
            num2 = adventureWorldTowerStats.Most1PCurses;
            break;
        case 2:
            num = adventureWorldTowerStats.Best2PTime;
            num2 = adventureWorldTowerStats.Most2PCurses;
            break;
        case 3:
            num = adventureWorldTowerStats.Best3PTime;
            num2 = adventureWorldTowerStats.Most3PCurses;
            break;
        case 4:
            num = adventureWorldTowerStats.Best4PTime;
            num2 = adventureWorldTowerStats.Most4PCurses;
            break;
        }
        if (num <= 0L)
        {
            this.levelBestTimeString = "";
        }
        else
        {
            this.levelBestTimeString = TrialsResults.GetTimeString(num);
        }
        if (num2 <= 0)
        {
            this.levelCursesString = "";
        }
        else if (num2 == 1)
        {
            this.levelCursesString = "1 CURSE";
        }
        else
        {
            this.levelCursesString = num2.ToString() + " CURSES";
        }
        if (adventureWorldTowerStats.Deaths == 0UL)
        {
            this.levelDeathsString = "";
        }
        else
        {
            this.levelDeathsString = adventureWorldTowerStats.Deaths.ToString();
        }
        if (adventureWorldTowerStats.Attempts == 0UL)
        {
            this.levelAttemptsString = "";
            return;
        }
        this.levelAttemptsString = adventureWorldTowerStats.Attempts.ToString();
    }
}
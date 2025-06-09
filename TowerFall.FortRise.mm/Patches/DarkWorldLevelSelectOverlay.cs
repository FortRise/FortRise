using FortRise;
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

    public override void Update()
    {
        if (map.Selection is DarkWorldMapButton)
            orig_Update();
        else
            base_Update();
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    [MonoModIgnore]
    public void base_Update() 
    {
        base.Update();
    }
    
    [MonoModReplace]
    private void RefreshLevelStats() 
    {
        var levelSet = map.GetLevelSet();
        DarkWorldTowerStats stats;
        if (levelSet == "TowerFall")
        {
            stats = SaveData.Instance.DarkWorld.Towers[statsID];
        }
        else
        {
            var data = TowerRegistry.DarkWorldGet(levelSet, statsID);
            stats = FortRiseModule.SaveData.AdventureWorld.AddOrGet(data.GetLevelID());
        }

        long bestTime;
        int mostCurses;
        switch (TFGame.PlayerAmount)
        {
        default:
            bestTime = stats.Best1PTime;
            mostCurses = stats.Most1PCurses;
            break;
        case 2:
            bestTime = stats.Best2PTime;
            mostCurses = stats.Most2PCurses;
            break;
        case 3:
            bestTime = stats.Best3PTime;
            mostCurses = stats.Most3PCurses;
            break;
        case 4:
            bestTime = stats.Best4PTime;
            mostCurses = stats.Most4PCurses;
            break;
        }
        if (bestTime <= 0L)
        {
            this.levelBestTimeString = "";
        }
        else
        {
            this.levelBestTimeString = TrialsResults.GetTimeString(bestTime);
        }
        if (mostCurses <= 0)
        {
            this.levelCursesString = "";
        }
        else if (mostCurses == 1)
        {
            this.levelCursesString = "1 CURSE";
        }
        else
        {
            this.levelCursesString = mostCurses.ToString() + " CURSES";
        }
        if (stats.Deaths == 0UL)
        {
            this.levelDeathsString = "";
        }
        else
        {
            this.levelDeathsString = stats.Deaths.ToString();
        }
        if (stats.Attempts == 0UL)
        {
            this.levelAttemptsString = "";
            return;
        }
        this.levelAttemptsString = stats.Attempts.ToString();
    }
}
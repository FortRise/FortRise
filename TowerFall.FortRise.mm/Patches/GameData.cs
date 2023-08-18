using FortRise;
using FortRise.Adventure;

namespace TowerFall;

public static class patch_GameData 
{
    public static extern void orig_Load();

    public static void Load() 
    {
        // Should be safe to restart here
        RiseCore.CantRestart = false;
        RiseCore.Events.Invoke_OnBeforeDataLoad();
        orig_Load();
        RiseCore.GameData.Load();

        // Assign its LevelID
        foreach (var darkWorldTowers in GameData.DarkWorldTowers) 
        {
            darkWorldTowers.SetLevelID("TowerFall");
        }
        TFGame.WriteLineToLoadLog("Loading Adventure World Tower Data...");

        TowerRegistry.LoadQuest();
        TowerRegistry.LoadDarkWorld();
        TowerRegistry.LoadVersus();
        TowerRegistry.LoadTrials();

        TFGame.WriteLineToLoadLog("  " + TowerRegistry.DarkWorldTowerSets.Count + " loaded");
        patch_MapScene.FixedStatic();
        RiseCore.Events.Invoke_OnAfterDataLoad();
    }
}


using FortRise;

namespace TowerFall;

public static class patch_GameData 
{
    public static extern void orig_Load();

    public static void Load() 
    {
        // Should be safe to restart here
        RiseCore.CantRestart = false;
        ModEventsManager.Instance.OnBeforeDataLoad.Raise(null, new OnDataLoadEventArgs(RiseCore.WillRestart));
        orig_Load();

        // Assign its LevelID
        foreach (var questTowers in GameData.QuestLevels) 
        {
            var name = (questTowers.Theme as patch_TowerTheme).ID;
            questTowers.SetLevelID(name);
            questTowers.SetLevelSet("TowerFall");
        }

        foreach (var versusTowers in GameData.VersusTowers) 
        {
            var name = (versusTowers.Theme as patch_TowerTheme).ID;
            versusTowers.SetLevelID(name);
            versusTowers.SetLevelSet("TowerFall");
        }

        foreach (var darkWorldTowers in GameData.DarkWorldTowers) 
        {
            var name = (darkWorldTowers.Theme as patch_TowerTheme).ID;
            darkWorldTowers.SetLevelID(name);
            darkWorldTowers.SetLevelSet("TowerFall");
        }

        foreach (var trialTowers in GameData.TrialsLevels) 
        {
            var name = (trialTowers.Theme as patch_TowerTheme).ID;
            trialTowers.SetLevelID(name + trialTowers.ID.Y);
            trialTowers.SetLevelSet("TowerFall");
        }

        TowerFall.Patching.MapScene.FixedStatic();
        ModEventsManager.Instance.OnAfterDataLoad.Raise(null, new OnDataLoadEventArgs(RiseCore.WillRestart));
    }
}


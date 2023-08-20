using System.Globalization;
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
        foreach (var questTowers in GameData.QuestLevels) 
        {
            var name = questTowers.Theme.Name;
            var correctName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLower());
            questTowers.SetLevelID(correctName);
        }

        foreach (var versusTowers in GameData.VersusTowers) 
        {
            var name = versusTowers.Theme.Name;
            var correctName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLower());
            versusTowers.SetLevelID(correctName);
        }

        foreach (var darkWorldTowers in GameData.DarkWorldTowers) 
        {
            var name = darkWorldTowers.Theme.Name;
            var correctName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLower());
            darkWorldTowers.SetLevelID(correctName);
        }

        foreach (var trialTowers in GameData.TrialsLevels) 
        {
            var name = trialTowers.Theme.Name;
            var id = trialTowers.ID.Y switch {
                0 => "I",
                1 => "II",
                _ => "III"
            };
            var correctName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLower()) + " " + id;
            trialTowers.SetLevelID(correctName);
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


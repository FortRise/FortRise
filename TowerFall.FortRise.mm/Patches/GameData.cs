using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public static class patch_GameData 
{
    [MonoModIgnore]
    private extern static string GetDarkWorldDirectory(string directory);

    [MonoModReplace]
    public static void Load() 
    {
        // Should be safe to restart here
        RiseCore.CantRestart = false;
        ModEventsManager.Instance.OnBeforeDataLoad.Raise(null, new DataLoadEventArgs(RiseCore.WillRestart));
        TFGame.WriteToLoadLog("Loading Background Data...");

        GameData.BGs = new Dictionary<string, XmlElement>(StringComparer.InvariantCultureIgnoreCase);

        XmlDocument bgXml = Calc.LoadXML(Path.Combine(Calc.LOADPATH, "Atlas", "GameData", "bgData.xml"));
        foreach (XmlElement bgData in bgXml.GetElementsByTagName("BG"))
        {
            GameData.BGs.Add(bgData.Attr("id"), bgData);
        }

        TFGame.WriteLineToLoadLog("  " + GameData.BGs.Count + " loaded");
        TFGame.WriteToLoadLog("Loading Tileset Data...");
        GameData.Tilesets = new Dictionary<string, TilesetData>(StringComparer.InvariantCultureIgnoreCase);

        XmlDocument tilesetXml = Calc.LoadXML(Path.Combine(Calc.LOADPATH, "Atlas", "GameData", "tilesetData.xml"));
        foreach (XmlElement tilesetData in tilesetXml.GetElementsByTagName("Tileset"))
        {
            GameData.Tilesets.Add(tilesetData.Attr("id"), new TilesetData(tilesetData));
        }

        TFGame.WriteLineToLoadLog("  " + GameData.Tilesets.Count + " loaded");
        TFGame.WriteToLoadLog("Loading Theme Data...");
        GameData.Themes = new Dictionary<string, TowerTheme>(StringComparer.InvariantCultureIgnoreCase);

        XmlDocument themeXml = Calc.LoadXML(Path.Combine(Calc.LOADPATH, "Atlas", "GameData", "themeData.xml"));
        foreach (XmlElement themeData in themeXml.GetElementsByTagName("Theme"))
        {
            GameData.Themes.Add(themeData.Attr("id"), new TowerTheme(themeData));
        }

        TFGame.WriteLineToLoadLog("  " + GameData.Themes.Count + " loaded");
        TFGame.WriteToLoadLog("Loading Versus Tower Data...");
        GameData.VersusTowers = [];

        string[] levels = Directory.GetDirectories(Path.Combine(Calc.LOADPATH, "Levels", "Versus"));
        Array.Sort(levels);
        foreach (string text in levels)
        {
            string text2 = GetDarkWorldDirectory(text);
            VersusTowerData versusTowerData = new VersusTowerData();
            versusTowerData.Load(GameData.VersusTowers.Count, text, text2);
            GameData.VersusTowers.Add(versusTowerData);
        }
        TFGame.WriteLineToLoadLog("  " + GameData.VersusTowers.Count + " loaded");
        TFGame.WriteToLoadLog("Loading Quest Level Data...");
        XmlDocument questAndTrialXml = Calc.LoadXML(Path.Combine(Calc.LOADPATH, "Levels", "Quest", "quest.xml"));
        int count = questAndTrialXml["quest"].GetElementsByTagName("level").Count;
        GameData.QuestLevels = new QuestLevelData[count];

        int id = 0;
        foreach (XmlElement levelChild in questAndTrialXml["quest"].GetElementsByTagName("level"))
        {
            GameData.QuestLevels[id] = new QuestLevelData(id, levelChild);
            id++;
        }
        TFGame.WriteLineToLoadLog("  " + GameData.QuestLevels.Length + " loaded");
        TFGame.WriteLineToLoadLog("Loading Dark World Tower Data...");
        GameData.DarkWorldTowers = [];
        levels = Directory.GetDirectories(Path.Combine(Calc.LOADPATH, "Levels", "DarkWorld"));
        Array.Sort(levels);

        foreach (string text in levels)
        {
            string text2 = GetDarkWorldDirectory(text);
            DarkWorldTowerData darkWorldTowerData = new DarkWorldTowerData();
            darkWorldTowerData.Load(GameData.DarkWorldTowers.Count, text, text2);
            GameData.DarkWorldTowers.Add(darkWorldTowerData);
        }

        TFGame.WriteLineToLoadLog("  " + GameData.DarkWorldTowers.Count + " loaded");
        TFGame.WriteToLoadLog("Loading Trials Level Data...");
        questAndTrialXml = Calc.LoadXML(Path.Combine(Calc.LOADPATH, "Levels", "Trials", "trials.xml"));
        int trialCount = questAndTrialXml["trials"].GetElementsByTagName("tier").Count;
        int tierCount = questAndTrialXml["trials"]["tier"].GetElementsByTagName("level").Count;
        GameData.TrialsLevels = new TrialsLevelData[tierCount, trialCount];

        int trialLevelCount = 0;
        int tierID = 0;
        foreach (XmlElement tierXml in questAndTrialXml["trials"].GetElementsByTagName("tier"))
        {
            int levelID = 0;
            foreach (XmlElement trialLevelXml in tierXml.GetElementsByTagName("level"))
            {
                GameData.TrialsLevels[levelID, tierID] = new TrialsLevelData(new Point(levelID, tierID), trialLevelXml);
                levelID++;
                trialLevelCount++;
            }
            tierID++;
        }

        TFGame.WriteLineToLoadLog("  " + trialLevelCount + " loaded");

        // Assign its LevelID
        foreach (var questTowers in GameData.QuestLevels) 
        {
            var name = (questTowers.Theme as patch_TowerTheme).ID;
            questTowers.LevelID = name;
            questTowers.TowerSet = "TowerFall";
        }

        foreach (var versusTowers in GameData.VersusTowers) 
        {
            var name = (versusTowers.Theme as patch_TowerTheme).ID;
            versusTowers.LevelID = name;
            versusTowers.TowerSet = "TowerFall";
        }

        foreach (var darkWorldTowers in GameData.DarkWorldTowers) 
        {
            var name = (darkWorldTowers.Theme as patch_TowerTheme).ID;
            darkWorldTowers.LevelID = name;
            darkWorldTowers.TowerSet = "TowerFall";
        }

        foreach (var trialTowers in GameData.TrialsLevels) 
        {
            var name = (trialTowers.Theme as patch_TowerTheme).ID;
            trialTowers.LevelID = name + trialTowers.ID.Y;
            trialTowers.TowerSet = "TowerFall";
        }

        TowerFall.Patching.MapScene.FixedStatic();
        ModEventsManager.Instance.OnAfterDataLoad.Raise(null, new DataLoadEventArgs(RiseCore.WillRestart));
    }
}


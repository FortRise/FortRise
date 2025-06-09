using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise;

public static class TowerRegistry 
{
    public static Dictionary<string, List<DarkWorldTowerData>> DarkWorldTowerSets = new();
    public static List<string> DarkWorldLevelSets = new();

    public static Dictionary<string, List<QuestLevelData>> QuestTowerSets = new();
    public static List<string> QuestLevelSets = new();

    public static Dictionary<string, List<VersusTowerData>> VersusTowerSets = new();
    public static List<string> VersusLevelSets = new();

    public static Dictionary<string, List<TrialsLevelData[]>> TrialsTowerSets = new();
    public static List<string> TrialsLevelSet = new();

    public static void PlayDarkWorld(string levelSet, string levelID, DarkWorldDifficulties difficulty, int startLevel = 0) 
    {
        // Checks if any players are present
        foreach (var player in TFGame.Players) 
        {
            if (player)
                // if there is, go to the level loading immediately
                goto Proceed;
        }
        //if not, activate atleast one player
        TFGame.Players[0] = true;
        Proceed:
        var level = DarkWorldGet(levelSet, levelID);
        patch_DarkWorldLevelSystem system = level.GetLevelSystem() as patch_DarkWorldLevelSystem;
        MainMenu.DarkWorldMatchSettings.DarkWorldDifficulty = difficulty;
        system.StartLevel = startLevel;
        MainMenu.DarkWorldMatchSettings.LevelSystem = system;
        Session session = new Session(MainMenu.DarkWorldMatchSettings);
        session.SetLevelSet(levelSet);
        session.StartGame();
    }

    public static void DarkWorldAdd(string levelSet, DarkWorldTowerData data) 
    {
        if (levelSet == string.Empty)
            levelSet = "UNCATEGORIZED";
        if (DarkWorldTowerSets.TryGetValue(levelSet, out var val)) 
        {
            data.ID.X = val.Count;
            val.Add(data);
            return;
        }
        DarkWorldLevelSets.Add(levelSet);
        var list = new List<DarkWorldTowerData>();
        data.ID.X = 0;
        list.Add(data);
        DarkWorldTowerSets[levelSet] = list; 
    }

    public static DarkWorldTowerData DarkWorldGet(string levelSet, int levelID) 
    {
        return DarkWorldTowerSets[levelSet][levelID];
    }

    public static bool TryDarkWorldGet(string levelSet, int levelID, out DarkWorldTowerData data) 
    {
        if (DarkWorldTowerSets.TryGetValue(levelSet, out var arr)) 
        {
            if (levelID < arr.Count && levelID > -1)
            {
                data = arr[levelID];
                return true;
            }
        }
        data = null;
        return false;
    }

    public static bool TryDarkWorldGet(string levelSet, string levelID, out DarkWorldTowerData data) 
    {
        if (DarkWorldTowerSets.TryGetValue(levelSet, out var arr)) 
        {
            foreach (var level in arr) 
            {
                if (level.GetLevelID() == levelID) 
                {
                    data = level;
                    return true;
                }
            }
        }
        data = null;
        return false;
    }

    public static DarkWorldTowerData DarkWorldGet(string levelSet, string levelID) 
    {
        var darkWorldLevel = DarkWorldTowerSets[levelSet];
        foreach (var level in darkWorldLevel) 
        {
            if (level.GetLevelID() == levelID) 
            {
                return level;
            }
        }
        return null;
    }

    public static DarkWorldTowerData DarkWorldGet(string levelSet, string levelID, out int id) 
    {
        var darkWorldLevel = DarkWorldTowerSets[levelSet];
        int i = 0;
        foreach (var level in darkWorldLevel) 
        {
            if (level.GetLevelID() == levelID) 
            {
                id = i;
                return level;
            }
            i++;
        }
        id = -1;
        return null;
    }

    public static bool TryDarkWorldGet(string levelSet, int levelID, out QuestLevelData data) 
    {
        if (QuestTowerSets.TryGetValue(levelSet, out var arr)) 
        {
            if (levelID < arr.Count && levelID > -1)
            {
                data = arr[levelID];
                return true;
            }
        }
        data = null;
        return false;
    }

    public static bool TryDarkWorldGet(string levelSet, string levelID, out QuestLevelData data) 
    {
        if (QuestTowerSets.TryGetValue(levelSet, out var arr)) 
        {
            foreach (var level in arr) 
            {
                if (level.GetLevelID() == levelID) 
                {
                    data = level;
                    return true;
                }
            }
        }
        data = null;
        return false;
    }


    public static void QuestAdd(string levelSet, QuestLevelData data) 
    {
        if (QuestTowerSets.TryGetValue(levelSet, out var val))  
        {
            data.ID.X = val.Count;
            val.Add(data);
            return;
        }
        QuestLevelSets.Add(levelSet);
        var list = new List<QuestLevelData>();
        data.ID.X = 0;
        list.Add(data);
        QuestTowerSets[levelSet] = list; 
    }

    public static QuestLevelData QuestGet(string levelSet, int levelID) 
    {
        return QuestTowerSets[levelSet][levelID];
    }

    public static QuestLevelData QuestGet(string levelSet, string levelID) 
    {
        var questLevel = QuestTowerSets[levelSet];
        foreach (var level in questLevel) 
        {
            if (level.GetLevelID() == levelID) 
            {
                return level;
            }
        }
        return null;
    }

    public static void VersusAdd(string levelSet, VersusTowerData data) 
    {
        if (VersusTowerSets.TryGetValue(levelSet, out var val)) 
        {
            data.ID.X = val.Count;
            val.Add(data);
            return;
        }
        VersusLevelSets.Add(levelSet);
        var list = new List<VersusTowerData>();
        data.ID.X = 0;
        list.Add(data);
        VersusTowerSets[levelSet] = list; 
    }


    public static VersusTowerData VersusGet(string levelSet, int levelID) 
    {
        return VersusTowerSets[levelSet][levelID];
    }

    public static VersusTowerData VersusGet(string levelSet, string levelID) 
    {
        var versusLevel = VersusTowerSets[levelSet];
        foreach (var level in versusLevel) 
        {
            if (level.GetLevelID() == levelID) 
            {
                return level;
            }
        }
        return null;
    }


    public static void TrialsAdd(TrialsLevelData[] datas) 
    {
        if (datas.Length == 0)
            return;    
        var levelSet = datas[0].GetLevelSet();
        if (TrialsTowerSets.TryGetValue(levelSet, out var val)) 
        {
            int i = 0;
            foreach (var data in datas) 
            {
                data.ID.X = val.Count;
                data.ID.Y = i;
                i++;
            }
            val.Add(datas);
            return;
        }
        TrialsLevelSet.Add(levelSet);
        var list = new List<TrialsLevelData[]>();
        datas[0].ID.X = 0;
        datas[1].ID.X = 0;
        datas[2].ID.X = 0;
        datas[0].ID.Y = 0;
        datas[1].ID.Y = 1;
        datas[2].ID.Y = 2;
        list.Add(datas);
        TrialsTowerSets[levelSet] = list; 
    }

    public static TrialsLevelData[] TrialsGet(string levelSet, int x, string levelID) 
    {
        var versusLevel = TrialsTowerSets[levelSet];
        foreach (var level in versusLevel) 
        {
            if (level[x].GetLevelID() == levelID) 
            {
                return level;
            }
        }
        return null;
    }

    public static TrialsLevelData[] TrialsGet(string levelSet, int levelID) 
    {
        return TrialsTowerSets[levelSet][levelID];
    }

    public static TrialsLevelData TrialsGet(string levelSet, int x, int y) 
    {
        return TrialsTowerSets[levelSet][x][y];
    }

    internal static void LoadQuest() 
    {
        foreach (var map in RiseCore.ResourceTree.TreeMap.Values
            .Where(folder => folder.ResourceType == typeof(RiseCore.ResourceTypeQuestTowerFolder)))
        {
            var fullPath = map.FullPath;
            var path = fullPath.Substring(4).Replace("Content/Levels/Quest/", string.Empty);

            var levelData = new patch_QuestLevelData();
            levelData.SetLevelID(path);
            levelData.SetLevelSet(path);
            levelData.Path = fullPath + "/" + "level.oel";
            levelData.DataPath = fullPath + "/" + "data.xml";

            IResourceInfo towerXmlResource = null;
            foreach (var child in map.Childrens) 
            {
                if (!child.Path.Contains("tower.xml")) 
                    continue;
                towerXmlResource = child;
                break;
            }
            if (towerXmlResource == null)
                continue;

            using var xmlStream = towerXmlResource.Stream;
            var xml = patch_Calc.LoadXML(xmlStream)["tower"];

            levelData.Author = xml.ChildText("author", string.Empty);
            levelData.Theme = LoadTheme(xml, map);

            TowerRegistry.QuestAdd(levelData.GetLevelSet(), levelData);
            RiseCore.Events.Invoke_OnAdventureQuestTowerDataAdd(levelData.GetLevelSet(), levelData);
        }
    }

    internal static void LoadVersus() 
    {
        foreach (var map in RiseCore.ResourceTree.TreeMap.Values
            .Where(folder => folder.ResourceType == typeof(RiseCore.ResourceTypeVersusTowerFolder))) 
        {
            var path = map.FullPath.Substring(4).Replace("Content/Levels/Versus/", string.Empty);

            var levelData = new patch_VersusTowerData();
            levelData.SetLevelID(path);
            levelData.SetLevelSet(path);
            levelData.Levels = new();

            IResourceInfo xmlResource = null;
            foreach (var child in map.Childrens) 
            {
                if ((child.ResourceType == typeof(RiseCore.ResourceTypeOel) || 
                child.ResourceType == typeof(RiseCore.ResourceTypeJson)) &&
                !child.Path.StartsWith("icon"))
                {
                    levelData.Levels.Add(CreateFromAdventure(child.RootPath));
                    continue;
                }

                if (child.Path.Contains("tower.xml")) 
                {
                    xmlResource = child;
                }
            }
            if (xmlResource == null)
                continue;

            using var xmlStream = xmlResource.Stream;
            var xml = patch_Calc.LoadXML(xmlStream)["tower"];
            levelData.Author = xml.ChildText("author", string.Empty);
            levelData.Theme = LoadTheme(xml, map);
            levelData.FixedFirst = xml.HasChild("fixedFirst");
            levelData.Procedural = xml.HasChild("procedural");
            if (!xml.HasChild("treasure")) 
            {
                levelData.TreasureMask = TreasureSpawner.FullTreasureMask;
                levelData.ArrowShuffle = false;
                levelData.SpecialArrowRate = 0.6f;
            }
            else 
            {
                var array = xml.ChildText("treasure").Split(',');
                levelData.TreasureMask = new int[TreasureSpawner.FullTreasureMask.Length];
                levelData.SetTreasureChances((float[])TreasureSpawner.DefaultTreasureChances.Clone());
                for (int i = 0; i < array.Length; i++)
                {
                    var treasure = array[i];
                    ParseTreasure(treasure.AsSpan().Trim(), out string resTreasure, out int chance, out int rate);
                    if (!patch_Calc.TryStringToEnum<Pickups>(resTreasure, out var pickups)) 
                    {
                        Logger.Error($"[ADVENTURE][VERSUS] The pickup name '{resTreasure}' cannot be found.");
                        continue;
                    }
                    
                    if (rate == -1)
                        levelData.TreasureMask[(int)pickups]++;
                    else
                        levelData.TreasureMask[(int)pickups] = levelData.TreasureMask[(int)pickups] + rate;

                    if (chance != -1)
                        levelData.GetTreasureChances()[(int)pickups] = (chance / 100);
                }

                levelData.ArrowShuffle = xml["treasure"].AttrBool("arrowShuffle", false);
                levelData.SpecialArrowRate = xml["treasure"].AttrFloat("arrows", 0.6f);
            }
            
            TowerRegistry.VersusAdd(levelData.GetLevelSet(), levelData);
            RiseCore.Events.Invoke_OnAdventureVersusTowerDataAdd(levelData.GetLevelSet(), levelData);
        }
    }

    private static VersusLevelData CreateFromAdventure(string path) 
    {
        var levelData = new patch_VersusLevelData();
        levelData.Path = path;
        if (RiseCore.ResourceTree.TreeMap.TryGetValue(path, out var res)) 
        {
            using var xml = res.Stream;
            var xmlElement = patch_Calc.LoadXML(xml)["level"]["Entities"];
            levelData.PlayerSpawns = xmlElement.GetElementsByTagName("PlayerSpawn").Count;
            levelData.TeamSpawns = Math.Min(xmlElement.GetElementsByTagName("TeamSpawnA").Count, 
                xmlElement.GetElementsByTagName("TeamSpawnB").Count);
        }
        return levelData;
    }

    private static void ParseTreasure(ReadOnlySpan<char> treasure, out string resultTreasure, out int chance, out int rate) 
    {
        if (treasure.IndexOf('[') != 0)
        {
            chance = -1;
            rate = -1;
            resultTreasure = treasure.ToString();
            return;
        }

        var text = treasure.Slice(1, treasure.IndexOf(']') - 1);
        resultTreasure = treasure.Slice(treasure.IndexOf(']') + 1).ToString();
        var split = text.SplitLines('*');
        chance = -1;
        rate = -1;
        foreach (var sp in split) 
        {
            var numText = sp.Line;
            if (numText.Contains("%".AsSpan(), StringComparison.InvariantCulture)) 
            {
                var chanceSlice = numText.Slice(0, numText.IndexOf('%'));
                chance = int.Parse(chanceSlice.ToString());
                continue;
            }
            rate = int.Parse(numText.ToString());
        }
    }

    internal static void LoadDarkWorld() 
    {
        foreach (var map in RiseCore.ResourceTree.TreeMap.Values
            .Where(folder => folder.ResourceType == typeof(RiseCore.ResourceTypeDarkWorldTowerFolder)))
        {
            var path = map.FullPath.Substring(4).Replace("Content/Levels/DarkWorld/", string.Empty);

            var levelData = new patch_DarkWorldTowerData();
            levelData.SetLevelID(path);
            levelData.SetLevelSet(path);
            levelData.Levels = new();

            IResourceInfo xmlResource = null;
            IResourceInfo iconResource = null;
            foreach (var child in map.Childrens) 
            {
                if ((child.ResourceType == typeof(RiseCore.ResourceTypeOel) || 
                child.ResourceType == typeof(RiseCore.ResourceTypeJson)) &&
                !child.Path.Contains("icon.json"))
                {
                    levelData.Levels.Add(child.RootPath);
                    continue;
                }

                if (child.Path.Contains("icon.json")) 
                {
                    iconResource = child;
                    continue;
                }

                if (child.Path.Contains("tower.xml")) 
                {
                    xmlResource = child;
                }
            }
            if (xmlResource == null)
                continue;

            // levelData.Stats = AdventureModule.SaveData.AdventureWorld.AddOrGet(levelData.GetLevelID());

            using var xmlStream = xmlResource.Stream;
            var xml = patch_Calc.LoadXML(xmlStream)["tower"];
            levelData.LoadExtraData(xml);
            levelData.Author = xml.ChildText("author", string.Empty);
            levelData.Theme = LoadTheme(xml, map);
            if (iconResource != null)
                levelData.BuildIcon(iconResource);
			
			levelData.TimeBase = xml["time"].ChildInt("base", 300);
			levelData.TimeAdd = xml["time"].ChildInt("add", 40);
			levelData.EnemySets = new Dictionary<string, List<DarkWorldTowerData.EnemyData>>();

			foreach (XmlElement xmlElement2 in xml["enemies"].GetElementsByTagName("set"))
			{
				string text3 = xmlElement2.Attr("id");
				List<DarkWorldTowerData.EnemyData> list = new List<DarkWorldTowerData.EnemyData>();
				foreach (XmlElement xmlElement3 in xmlElement2.GetElementsByTagName("spawn"))
				{
					list.Add(new DarkWorldTowerData.EnemyData(xmlElement3));
				}
				levelData.EnemySets.Add(text3, list);
			}

			levelData.Normal = levelData.LoadLevelSet_Public(xml["normal"]);
			levelData.Hardcore = levelData.LoadLevelSet_Public(xml["hardcore"]);
			levelData.Legendary = levelData.LoadLevelSet_Public(xml["legendary"]);

            TowerRegistry.DarkWorldAdd(levelData.GetLevelSet(), levelData);
            RiseCore.Events.Invoke_OnAdventureDarkWorldTowerDataAdd(levelData.GetLevelSet(), levelData);
        }
    }

    internal static void LoadTrials() 
    {
        foreach (var map in RiseCore.ResourceTree.TreeMap.Values
            .Where(folder => folder.ResourceType == typeof(RiseCore.ResourceTypeTrialsTowerFolder)))
        {
            var path = map.FullPath.Substring(4).Replace("Content/Levels/Trials/", string.Empty);
            IResourceInfo towerResource = null;
            foreach (var child in map.Childrens) 
            {
                if (child.Path.Contains("tower.xml")) 
                {
                    towerResource = child;
                }
            }
            if (towerResource == null)
                continue;
            
            {
                using var xmlStream = towerResource.Stream;
                var xml = patch_Calc.LoadXML(xmlStream)["tower"];

                if (xml.HasChild("tier"))
                    xml = xml["tier"];

                int id = 0;
                var arr = new TrialsLevelData[3];
                foreach (XmlElement element in xml.GetElementsByTagName("level")) 
                {
                    var trialData = new patch_TrialsLevelData();
                    trialData.SetLevelID(path + "-" + id);
                    trialData.SetLevelSet(path);
                    trialData.Path = map.Root + map.Path + "/" + element.Attr("path");
                    trialData.Arrows = element.ChildInt("arrows", 3);
                    trialData.SwitchBlockTimer = element.ChildInt("switchTimer", 200);
                    trialData.Theme = LoadTheme(element, map);
                    trialData.Goals = new TimeSpan[3];
                    trialData.Goals[0] = TimeSpan.FromSeconds((double)element.ChildFloat("gold", 0.3f));
                    trialData.Goals[1] = TimeSpan.FromSeconds((double)element.ChildFloat("diamond", 0.2f));
                    trialData.Goals[2] = TimeSpan.FromSeconds((double)element.ChildFloat("dev", 0.1f));
                    arr[id] = trialData;
                    id++;

                    trialData.Author = xml.ChildText("author", string.Empty);
                }
                TowerRegistry.TrialsAdd(arr);
                if (arr.Length > 0)
                {
                    RiseCore.Events.Invoke_OnAdventureTrialsTowerDatasAdd(arr[0].GetLevelSet(), arr);
                }
            }
        }
    }

    private static TowerTheme LoadTheme(XmlElement xml, IResourceInfo map) 
    {
        if (xml.HasChild("theme")) 
        {
            var xmlTheme = xml["theme"];
            if (xmlTheme.HasChild("Name")) 
            {
                return new patch_TowerTheme(xml["theme"], map);
            }
            else 
            {
                return GameData.Themes[xml.ChildText("theme").Trim()];
            }
        }
        return TowerTheme.GetDefault();
    }
}
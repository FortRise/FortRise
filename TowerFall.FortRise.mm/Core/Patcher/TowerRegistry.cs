using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise.Adventure;

public static class TowerRegistry 
{
    public static Dictionary<string, List<AdventureWorldTowerData>> DarkWorldTowerSets = new();
    public static List<string> DarkWorldLevelSets = new();

    public static Dictionary<string, List<QuestLevelData>> QuestTowerSets = new();
    public static List<string> QuestLevelSets = new();

    public static void DarkWorldAdd(string levelSet, AdventureWorldTowerData data) 
    {
        if (DarkWorldTowerSets.TryGetValue(levelSet, out var val)) 
        {
            data.ID.X = val.Count;
            val.Add(data);
            return;
        }
        DarkWorldLevelSets.Add(levelSet);
        var list = new List<AdventureWorldTowerData>();
        data.ID.X = 0;
        list.Add(data);
        DarkWorldTowerSets[levelSet] = list; 
    }

    public static AdventureWorldTowerData DarkWorldGet(string levelSet, int levelID) 
    {
        return DarkWorldTowerSets[levelSet][levelID];
    }

    public static bool TryDarkWorldGet(string levelSet, int levelID, out AdventureWorldTowerData data) 
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

    public static bool TryDarkWorldGet(string levelSet, string levelID, out AdventureWorldTowerData data) 
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

    public static AdventureWorldTowerData DarkWorldGet(string levelSet, string levelID) 
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

    public static QuestLevelData QuestGet(string levelSet, string levelID) 
    {
        var darkWorldLevel = QuestTowerSets[levelSet];
        foreach (var level in darkWorldLevel) 
        {
            if (level.GetLevelID() == levelID) 
            {
                return level;
            }
        }
        return null;
    }

    internal static void LoadQuest() 
    {
        foreach (var map in RiseCore.Resources.GlobalResources.Values
            .Where(folder => folder.Path.StartsWith("Content/Levels/Quest") && (
                !folder.Path.Contains(".oel") &&
                !folder.Path.Contains(".xml") &&
                !folder.Path.Contains(".json"))
            ))
        {
            var fullPath = map.FullPath;
            var path = fullPath.Substring(4).Replace("Content/Levels/Quest/", string.Empty);

            var levelData = new patch_QuestLevelData();
            levelData.SetLevelID(path);
            levelData.SetLevelSet(path);
            levelData.Stats = AdventureModule.SaveData.AdventureQuest.AddOrGet(levelData.GetLevelID());
            levelData.Path = fullPath + "/" + "00.oel";
            levelData.DataPath = fullPath + "/" + "tower.xml";

            RiseCore.Resource xmlResource = null;
            foreach (var child in map.Childrens) 
            {
                if (!child.Path.Contains("tower.xml")) 
                    continue;
                xmlResource = child;
                break;
            }
            if (xmlResource == null)
                continue;

            using var xmlStream = xmlResource.Stream;
            var xml = patch_Calc.LoadXML(xmlStream)["data"];

            if (xml.HasChild("theme")) 
            {
                var xmlTheme = xml["theme"];
                if (xmlTheme.NodeType == XmlNodeType.Element) 
                {
                    levelData.Theme = new patch_TowerTheme(xml["theme"]);
                }
                else 
                {
                    levelData.Theme = GameData.Themes[xml.ChildText("theme")];
                }
            }
            else 
            {
                levelData.Theme = TowerTheme.GetDefault();
            }

            TowerRegistry.QuestAdd(levelData.GetLevelSet(), levelData);
        }
    }

    internal static void LoadDarkWorld() 
    {
        foreach (var map in RiseCore.Resources.GlobalResources.Values
            .Where(folder => folder.Path.StartsWith("Content/Levels/DarkWorld") && (
                !folder.Path.Contains(".oel") &&
                !folder.Path.Contains(".xml") &&
                !folder.Path.Contains(".json"))
            )) 
        {
            var path = map.FullPath.Substring(4).Replace("Content/Levels/DarkWorld/", string.Empty);

            var levelData = new AdventureWorldTowerData(map);
            levelData.SetLevelID(path);
            levelData.SetLevelSet(path);
            levelData.Levels = new();
            levelData.Stats = AdventureModule.SaveData.AdventureWorld.AddOrGet(levelData.GetLevelID(), path);

            RiseCore.Resource xmlResource = null;
            foreach (var child in map.Childrens) 
            {
                if ((child.Path.EndsWith(".oel") || child.Path.EndsWith(".json")) && !child.Path.StartsWith("icon")) 
                {
                    levelData.Levels.Add(child.FullPath);
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
            levelData.LoadExtraData(xml);
            levelData.Author = xml.ChildText("author", string.Empty);
            if (xml.HasChild("theme")) 
            {
                var xmlTheme = xml["theme"];
                if (xmlTheme.NodeType == XmlNodeType.Element) 
                {
                    levelData.Theme = new patch_TowerTheme(xml["theme"], map);
                }
                else 
                {
                    levelData.Theme = GameData.Themes[xml.ChildText("theme")];
                }
            }
            else 
            {
                levelData.Theme = TowerTheme.GetDefault();
            }
			
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

            if (xml.HasChild("required"))
                levelData.RequiredMods = xml["required"].InnerText;
            else
                levelData.RequiredMods = string.Empty;

            TowerRegistry.DarkWorldAdd(levelData.GetLevelSet(), levelData);
        }
    }
}
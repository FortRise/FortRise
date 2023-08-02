using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_QuestLevelData : QuestLevelData
{
    [MonoModIgnore]
    public string ModID { get; set; }

    public string Path;
    public string DataPath;
    public patch_QuestLevelData(int id, XmlElement xml) :base(0, null)
    {
    }

    public patch_QuestLevelData() :base(0, null)
    {
    }

    [MonoModReplace]
    public void ctor() {}

    public static void Load() 
    {
        var levels = new List<patch_QuestLevelData>();
        foreach (var map in RiseCore.Resources.GlobalResources.Values.Where(folder => folder.Path.StartsWith("Content/Levels/Quest"))) 
        {
            var path = map.Path.Substring(21);

            var levelData = new patch_QuestLevelData();
            levelData.ModID = path;
            levelData.Path = System.IO.Path.Combine(path, "00.oel");
            levelData.DataPath = System.IO.Path.Combine(path, "tower.xml");

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
            var xml = patch_Calc.LoadXML(xmlStream)["tower"];
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

            levels.Add(levelData);
        }

        var questLevels = new List<QuestLevelData>(GameData.QuestLevels);
        questLevels.AddRange(levels);
        GameData.QuestLevels = questLevels.ToArray();

        for (int i = 0; i < GameData.QuestLevels.Length; i++)
        {
            var questLevel = GameData.QuestLevels[i];
            questLevel.ID.X = i;
        }
    }
}
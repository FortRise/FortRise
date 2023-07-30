using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;
using static FortRise.RiseCore;

namespace FortRise.Adventure;

public sealed class XmlAdventureTowerLoader : IAdventureTowerLoader<XmlElement>
{
    public string FileExtension => "xml";
    public ModResource System;
    public AdventureWorldTowerData Tower;

    public XmlAdventureTowerLoader(ModResource system, AdventureWorldTowerData towerData) 
    {
        System = system;
        Tower = towerData;
    }

    public AdventureTowerInfo Load(int id, Stream stream, string levelDirectory, string directoryPrefix, bool customIcons)
    {
        var info = new AdventureTowerInfo();

        info.StoredDirectory = levelDirectory;
        info.ID = id;
        var xmlElement =  patch_Calc.LoadXML(stream)["tower"];
        info.Theme = xmlElement.HasChild("theme") ? new patch_TowerTheme(xmlElement["theme"]) : patch_TowerTheme.GetDefault();
        info.Author = xmlElement.HasChild("author") ? xmlElement["author"].InnerText : string.Empty;
        info.Stats = AdventureModule.SaveData.AdventureWorld.AddOrGet(info.Theme.Name, levelDirectory);
        info.Extras = LoadExtraData(xmlElement);

        var guid = (info.Theme as patch_TowerTheme).GenerateThemeID();

        if (xmlElement.HasChild("time"))
        {
            info.TimeBase = xmlElement["time"].ChildInt("base", 300);
            info.TimeAdd = xmlElement["time"].ChildInt("add", 40);
        }
        else
        {
            info.TimeBase = 300;
            info.TimeAdd = 40;
        }
        info.EnemySets = new Dictionary<string, List<DarkWorldTowerData.EnemyData>>();
        foreach (object obj in xmlElement["enemies"].GetElementsByTagName("set"))
        {
            var xmlElement2 = (XmlElement)obj;
            string key = xmlElement2.Attr("id");
            List<DarkWorldTowerData.EnemyData> list = new List<DarkWorldTowerData.EnemyData>();
            foreach (object obj2 in xmlElement2.GetElementsByTagName("spawn"))
            {
                XmlElement xml = (XmlElement)obj2;
                list.Add(new DarkWorldTowerData.EnemyData(xml));
            }
            info.EnemySets.Add(key, list);
        }
        info.Normal = LoadLevelSet(xmlElement["normal"], info.EnemySets);
        info.Hardcore = LoadLevelSet(xmlElement["hardcore"], info.EnemySets);
        info.Legendary = LoadLevelSet(xmlElement["legendary"], info.EnemySets);
        if (xmlElement.HasChild("required"))
            info.RequiredMods = xmlElement["required"].InnerText;
        else
            info.RequiredMods = string.Empty;

        return info;
    }

    public ExtraAdventureTowerInfo LoadExtraData(XmlElement data)
    {
        var info = new ExtraAdventureTowerInfo();
        if (data.HasChild("lives")) 
        {
            info.StartingLives = int.Parse(data["lives"].InnerText);
        }
        if (data.HasChild("procedural"))
            info.Procedural = bool.Parse(data["procedural"].InnerText);
        if (data.HasChild("continues")) 
        {
            var continues = data["continues"];
            if (continues.HasChild("normal"))
                info.NormalContinues = int.Parse(continues["normal"].InnerText);
            if (continues.HasChild("hardcore"))
                info.HardcoreContinues = int.Parse(continues["hardcore"].InnerText);
            if (continues.HasChild("legendary"))
                info.LegendaryContinues = int.Parse(continues["legendary"].InnerText);
        }
        return info;
    }

    public List<DarkWorldTowerData.LevelData> LoadLevelSet(XmlElement data, Dictionary<string, List<DarkWorldTowerData.EnemyData>> enemySets)
    {
        List<DarkWorldTowerData.LevelData> list = new List<DarkWorldTowerData.LevelData>();
        foreach (object obj in data.GetElementsByTagName("level"))
        {
            XmlElement xmlElement = (XmlElement)obj;
            list.Add(new DarkWorldTowerData.LevelData(xmlElement, enemySets));
        }
        list[list.Count - 1].FinalLevel = true;
        return list;
    }
}
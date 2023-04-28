#pragma warning disable CS0626
#pragma warning disable CS0108

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Monocle;
using MonoMod;
using TeuJson;

namespace TowerFall;

public static class patch_GameData 
{
    public static string AW_PATH = "AdventureWorldContent" + Path.DirectorySeparatorChar;
    public static List<AdventureWorldData> AdventureWorldTowers;
    public static List<string> AdventureWorldTowersLoaded = new();


    public static extern void orig_Load();

    public static void Load() 
    {
        orig_Load();
        WorldSaveData.Load(WorldSaveData.SavePath);
        TFGame.WriteLineToLoadLog("Loading Adventure World Tower Data...");
        AdventureWorldTowers = new List<AdventureWorldData>();
        foreach (string directory2 in Directory.EnumerateDirectories(Path.Combine(
            AW_PATH, "Levels")))
        {
            if (LoadAdventureLevelsParallel(directory2))
                AdventureWorldTowersLoaded.Add(directory2);
        }

        if (File.Exists("adventureCache.json")) 
        {
            var loadAdventurePath = JsonTextReader.FromFile("adventureCache.json").ConvertToListString();
            foreach (var adventurePath in loadAdventurePath) 
            {
                if (LoadAdventureLevels(adventurePath, true))
                    AdventureWorldTowersLoaded.Add(adventurePath);
            }
        }

        TFGame.WriteLineToLoadLog("  " + AdventureWorldTowers.Count + " loaded");
    }

    public static bool LoadAdventureLevelsParallel(string directory) 
    {
        var adventureTowerData = new AdventureWorldData();
        if (adventureTowerData.AdventureLoadParallel(AdventureWorldTowers.Count, directory)) 
        {
            AdventureWorldTowers.Add(adventureTowerData);
            return true;
        }
        return false;
    }

    public static bool LoadAdventureLevels(string directory, bool json = false) 
    {
        var adventureTowerData = new AdventureWorldData();
        if (adventureTowerData.AdventureLoad(AdventureWorldTowers.Count, directory, json)) 
        {
            AdventureWorldTowers.Add(adventureTowerData);
            return true;
        }
        return false;
    }
}

public class AdventureWorldData : DarkWorldTowerData 
{
    public AdventureWorldTowerStats Stats;
    private void SequentialLookup(string directory, bool json) 
    {
        if (json) 
        {
            foreach (string level in Directory.EnumerateFiles(directory, "*.json", SearchOption.TopDirectoryOnly))
            {
                this.Levels.Add(level);
            }
        }
        else 
        {
            foreach (string level in Directory.EnumerateFiles(directory, "*.oel", SearchOption.TopDirectoryOnly))
            {
                this.Levels.Add(level);
            }
        }
    }

    private void ParallelLookup(string directory) 
    {
        foreach (string level in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
        {
            if (level.EndsWith(".json") || level.EndsWith(".oel"))
                this.Levels.Add(level);
        }
    }

    public bool AdventureLoadParallel(int id, string levelDirectory, bool json = false) 
    {
        Levels = new List<string>();
        ParallelLookup(levelDirectory);
        return InternalAdventureLoad(id, levelDirectory);
    }

    public bool AdventureLoad(int id, string levelDirectory, bool json = false) 
    {
        Levels = new List<string>();
        SequentialLookup(levelDirectory, json);
        return InternalAdventureLoad(id, levelDirectory);
    } 

    public bool InternalAdventureLoad(int id, string levelDirectory) 
    {
        if (this.Levels.Count <= 0) 
        {
            return false;
        }

        ID.X = id;
        var xmlElement =  Calc.LoadXML(Path.Combine(levelDirectory, "tower.xml"))["tower"];
        Theme = xmlElement.HasChild("theme") ? new TowerTheme(xmlElement["theme"]) : TowerTheme.GetDefault();
        Stats = WorldSaveData.Instance.AdventureWorld.AddOrGet(Theme.Name, levelDirectory);

        this.TimeBase = xmlElement["time"].ChildInt("base");
        this.TimeAdd = xmlElement["time"].ChildInt("add");
        this.EnemySets = new Dictionary<string, List<DarkWorldTowerData.EnemyData>>();
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
            this.EnemySets.Add(key, list);
        }
        this.Normal = this.LoadLevelSet(xmlElement["normal"]);
        this.Hardcore = this.LoadLevelSet(xmlElement["hardcore"]);
        this.Legendary = this.LoadLevelSet(xmlElement["legendary"]);
        return true;
    }

    [MonoModIgnore]
    private extern List<DarkWorldTowerData.LevelData> LoadLevelSet(XmlElement xml);
}
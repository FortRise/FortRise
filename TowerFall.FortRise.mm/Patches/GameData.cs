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
            LoadAdventureLevelsParallel(directory2);
        }

        if (File.Exists("adventureCache.json")) 
        {
            var loadAdventurePath = JsonTextReader.FromFile("adventureCache.json").ConvertToListString();
            foreach (var adventurePath in loadAdventurePath) 
            {
                Engine.Instance.Commands.Log("Loading " + adventurePath);
                if (LoadAdventureLevelsParallel(adventurePath))
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
            Engine.Instance.Commands.Log("[AdventureMod] Loaded " + directory);
            return true;
        }
        return false;
    }
}

public class AdventureWorldData : DarkWorldTowerData 
{
    public string StoredDirectory;
    public string Author;
    public AdventureWorldTowerStats Stats;

    private void ParallelLookup(string directory) 
    {
        foreach (string level in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
        {
            if (level.EndsWith(".json") || level.EndsWith(".oel"))
                this.Levels.Add(level);
        }
    }

    public bool AdventureLoadParallel(int id, string levelDirectory) 
    {
        Levels = new List<string>();
        ParallelLookup(levelDirectory);
        return InternalAdventureLoad(id, levelDirectory);
    }

    public bool InternalAdventureLoad(int id, string levelDirectory) 
    {
        if (this.Levels.Count <= 0) 
        {
            return false;
        }

        StoredDirectory = levelDirectory;

        ID.X = id;
        var xmlElement =  Calc.LoadXML(Path.Combine(levelDirectory, "tower.xml"))["tower"];
        Theme = xmlElement.HasChild("theme") ? new TowerTheme(xmlElement["theme"]) : TowerTheme.GetDefault();
        Author = xmlElement.HasChild("author") ? xmlElement["author"].InnerText : string.Empty;
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
        this.Normal = LoadLevelSet(xmlElement["normal"]);
        this.Hardcore = LoadLevelSet(xmlElement["hardcore"]);
        this.Legendary = LoadLevelSet(xmlElement["legendary"]);
        return true;
    }

    [MonoModIgnore]
    private extern List<DarkWorldTowerData.LevelData> LoadLevelSet(XmlElement xml);
}
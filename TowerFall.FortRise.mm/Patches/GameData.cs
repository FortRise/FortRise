using System.Collections.Generic;
using System.IO;
using System.Xml;
using FortRise;
using Monocle;
using MonoMod;
using TeuJson;
using static FortRise.Logger;

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

    private (bool, string) ParallelLookup(string directory) 
    {
        bool customIcon = false;
        string pathToIcon = string.Empty;
        foreach (string path in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
        {
            if (path.Contains("icon")) 
            {
                customIcon = true;
                pathToIcon = path;
                Logger.Log(pathToIcon);
                continue;
            }
            if (path.EndsWith(".json") || path.EndsWith(".oel"))
                Levels.Add(path);
        }
        return (customIcon, pathToIcon);
    }

    private void BuildIcon(string path) 
    {
        Logger.Log(path);
        var json = JsonConvert.DeserializeFromFile(path);
        var layers = json["layers"].AsJsonArray;
        var solids = layers[0];
        var grid2D = solids["grid2D"].ConvertToArrayString2D();
        var bitString = Ogmo3ToOel.Array2DToStraightBitString(grid2D);
        var x = grid2D.GetLength(1);
        var y = grid2D.GetLength(0);
        if (x != 16 || y != 16) 
        {
            Logger.Error("Invalid icon size, it must be 16x16 dimension or 160x160 in level dimension");
            return;
        }
        Theme.Icon = new Subtexture(new Monocle.Texture(TowerMapData.BuildIcon(bitString, Theme.TowerType)));
    }

    public bool AdventureLoadParallel(int id, string levelDirectory) 
    {
        Levels = new List<string>();
        var (customIcon, pathToIcon) = ParallelLookup(levelDirectory);
        return InternalAdventureLoad(id, levelDirectory, pathToIcon, customIcon);
    }

    public bool InternalAdventureLoad(int id, string levelDirectory, string pathToIcon, bool customIcons = false) 
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

        Logger.Log(!string.IsNullOrEmpty(pathToIcon));
        Logger.Log(customIcons);

        if (!string.IsNullOrEmpty(pathToIcon) && customIcons)
            BuildIcon(pathToIcon);

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
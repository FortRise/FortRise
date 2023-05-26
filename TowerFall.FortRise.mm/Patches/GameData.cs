using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;
using TeuJson;

namespace TowerFall;

public static class patch_GameData 
{
    public static Dictionary<string, TilesetData> CustomTilesets;
    public static Dictionary<string, XmlElement> CustomBGs;
    public static Dictionary<string, Monocle.Texture> CustomBGAtlas;
    public static string AW_PATH = "AdventureWorldContent" + Path.DirectorySeparatorChar;
    public static List<AdventureWorldTowerData> AdventureWorldTowers;
    public static List<string> AdventureWorldTowersLoaded;


    public static extern void orig_Load();

    public static void Load() 
    {
        orig_Load();
        WorldSaveData.Load(WorldSaveData.SavePath);
        TFGame.WriteLineToLoadLog("Loading Adventure World Tower Data...");
        ReloadCustomLevels();
        TFGame.WriteLineToLoadLog("  " + AdventureWorldTowers.Count + " loaded");
    }

    public static void ReloadCustomLevels() 
    {
        if (CustomTilesets != null) 
        {
            foreach (var tileset in CustomTilesets) 
            {
                tileset.Value.Texture.Texture2D.Dispose();
            }
        }
        if (CustomBGAtlas != null) 
        {
            foreach (var bg in CustomBGAtlas) 
            {
                bg.Value.Texture2D.Dispose();
            }
        }

        CustomBGAtlas ??= new();
        CustomTilesets ??= new();
        CustomBGs ??= new();

        CustomBGAtlas.Clear();
        CustomBGs.Clear();
        CustomTilesets.Clear();
        AdventureWorldTowers ??= new List<AdventureWorldTowerData>();
        AdventureWorldTowers.Clear();
        AdventureWorldTowersLoaded ??= new List<string>();
        AdventureWorldTowersLoaded.Clear();
        if (!Directory.Exists("AdventureWorldContent"))
            Directory.CreateDirectory("AdventureWorldContent");
        if (!Directory.Exists("AdventureWorldContent/Levels"))
            Directory.CreateDirectory("AdventureWorldContent/Levels");
        foreach (string directory2 in Directory.EnumerateDirectories(Path.Combine(
            AW_PATH, "Levels")))
        {
            LoadAdventureLevelsParallel(directory2);
        }

        if (File.Exists("adventureCache.json")) 
        {
            var loadAdventurePath = JsonTextReader.FromFile("adventureCache.json").ConvertToListString();
            if (loadAdventurePath == null)
                return;    
            foreach (var adventurePath in loadAdventurePath) 
            {
                if (LoadAdventureLevelsParallel(adventurePath))
                    AdventureWorldTowersLoaded.Add(adventurePath);
            }
        }
    }

    public static bool LoadAdventureLevelsParallel(string directory) 
    {
        var adventureTowerData = new AdventureWorldTowerData();
        if (adventureTowerData.AdventureLoadParallel(AdventureWorldTowers.Count, directory)) 
        {
            AdventureWorldTowers.Add(adventureTowerData);
            return true;
        }
        return false;
    }
}

public class AdventureWorldTowerData : DarkWorldTowerData 
{
    public string StoredDirectory;
    public string Author;
    public int StartingLives = -1;
    public string[] RequiredMods;
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
                continue;
            }
            if (path.EndsWith(".json") || path.EndsWith(".oel"))
                Levels.Add(path);
        }
        return (customIcon, pathToIcon);
    }

    private void BuildIcon(string path) 
    {
        var json = JsonConvert.DeserializeFromFile(path);
        var layers = json["layers"].AsJsonArray;
        var solids = layers[0];
        var grid2D = solids["grid2D"].ConvertToArrayString2D();
        var bitString = Ogmo3ToOel.Array2DToStraightBitString(grid2D);
        var x = grid2D.GetLength(1);
        var y = grid2D.GetLength(0);
        if (x != 16 || y != 16) 
        {
            Logger.Error($"{path}: Invalid icon size, it must be 16x16 dimension or 160x160 in level dimension");
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
        if (xmlElement.HasChild("lives"))
            StartingLives = int.Parse(xmlElement["lives"].InnerText);
        Stats = WorldSaveData.Instance.AdventureWorld.AddOrGet(Theme.Name, levelDirectory);

        if (!string.IsNullOrEmpty(pathToIcon) && customIcons)
            BuildIcon(pathToIcon);
        
        LoadCustomElements(xmlElement["theme"]);

        TimeBase = xmlElement["time"].ChildInt("base");
        TimeAdd = xmlElement["time"].ChildInt("add");
        EnemySets = new Dictionary<string, List<DarkWorldTowerData.EnemyData>>();
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
        Normal = LoadLevelSet(xmlElement["normal"]);
        Hardcore = LoadLevelSet(xmlElement["hardcore"]);
        Legendary = LoadLevelSet(xmlElement["legendary"]);
        if (xmlElement.HasChild("required"))
            RequiredMods = patch_Calc.ChildStringArray(xmlElement, "required");
        else
            RequiredMods = Array.Empty<string>();

        return true;
    }

    private void LoadCustomElements(XmlElement element) 
    {
        var fgTileset = element["Tileset"].InnerText.AsSpan();
        var bgTileset = element["BGTileset"].InnerText.AsSpan();
        var background = element["Background"].InnerText.AsSpan();

        if (fgTileset.StartsWith("custom:".AsSpan())) 
        {
            var sliced = fgTileset.Slice(7).ToString();
            var path = Path.Combine(StoredDirectory, sliced);
            var loadedXML = Calc.LoadXML(path)["Tileset"];
            var tilesetPath = Path.Combine(StoredDirectory, loadedXML.Attr("image"));
            patch_GameData.CustomTilesets.Add(path, patch_TilesetData.Create(loadedXML, tilesetPath));
            Theme.Tileset = path;
        }
        if (bgTileset.StartsWith("custom:".AsSpan())) 
        {
            var sliced = bgTileset.Slice(7).ToString();
            var path = Path.Combine(StoredDirectory, sliced);
            var loadedXML = Calc.LoadXML(path)["Tileset"];
            var tilesetPath = Path.Combine(StoredDirectory, loadedXML.Attr("image"));
            patch_GameData.CustomTilesets.Add(path, patch_TilesetData.Create(loadedXML, tilesetPath));
            Theme.BGTileset = path;
        }
        if (background.StartsWith("custom:".AsSpan())) 
        {
            var sliced = background.Slice(7).ToString();
            Theme.BackgroundID = sliced;
            LoadBG(sliced);
        }

        void LoadBG(string background) 
        {
            var path = Path.Combine(StoredDirectory, background);
            var loadedXML = Calc.LoadXML(path)["BG"];
            var customBGAtlasPath = loadedXML["ImagePath"]?.InnerText;
            
            if (!string.IsNullOrEmpty(customBGAtlasPath)) 
            {
                using var fs = File.OpenRead(Path.Combine(StoredDirectory, customBGAtlasPath));
                var texture2D = Texture2D.FromStream(Engine.Instance.GraphicsDevice, fs);
                patch_GameData.CustomBGAtlas.Add(customBGAtlasPath, new Monocle.Texture(texture2D));
            }

            Theme.ForegroundData = loadedXML["Foreground"];
            Theme.BackgroundData = loadedXML["Background"];
            patch_GameData.CustomBGs.Add(path, loadedXML);
        }
    }

    [MonoModIgnore]
    private extern List<DarkWorldTowerData.LevelData> LoadLevelSet(XmlElement xml);
}
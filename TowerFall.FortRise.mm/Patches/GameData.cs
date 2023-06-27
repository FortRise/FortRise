using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using FortRise;
using FortRise.Adventure;
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
    public static Dictionary<string, int> AdventureWorldModTowersLookup;
    public static List<(bool contains, CustomMapRenderer renderer)> AdventureWorldMapRenderer;
    public static List<List<AdventureWorldTowerData>> AdventureWorldModTowers;

    public static extern void orig_Load();

    public static void Load() 
    {
        RiseCore.Events.Invoke_OnBeforeDataLoad();
        orig_Load();
        TFGame.WriteLineToLoadLog("Loading Adventure World Tower Data...");
        ReloadCustomTowers();
        TFGame.WriteLineToLoadLog("  " + AdventureWorldTowers.Count + " loaded");
        patch_MapScene.FixedStatic();
        RiseCore.Events.Invoke_OnAfterDataLoad();
    }

    /// <summary>
    /// Reload custom adventure towers.
    /// </summary>
    public static void ReloadCustomTowers() 
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
        AdventureWorldTowers ??= new();
        AdventureWorldTowers.Clear();

        AdventureWorldModTowersLookup ??= new();
        AdventureWorldModTowersLookup.Clear();

        AdventureWorldModTowers ??= new();
        AdventureWorldModTowers.Clear();

        AdventureWorldMapRenderer ??= new();
        AdventureWorldMapRenderer.Clear();

        const string AdventureModPath = "Content/Mod/Adventure/DarkWorld";
        if (!Directory.Exists(AdventureModPath))
            Directory.CreateDirectory(AdventureModPath);

        var contentModDirectories = new List<string>(Directory.EnumerateDirectories(AdventureModPath));
        contentModDirectories.InsertRange(0, AdventureModule.SaveData.LevelLocations);

        if (Directory.Exists("AdventureWorldContent/Levels")) 
        {
            Logger.Warning("AdventureWorldContent path is obsolete! Use DLL-Less Mods using Mods folder or Load it inside of Content/Mod/Adventure/DarkWorld instead");
            contentModDirectories.AddRange(Directory.EnumerateDirectories(Path.Combine(AW_PATH, "Levels")));
        }


        foreach (var adventurePath in contentModDirectories) 
        {
            LoadAdventureTowers(adventurePath, null);
        }

        AdventureWorldMapRenderer.Add((false, null));

        // Load mods that contains Levels/DarkWorld folder
        foreach (var mod in RiseCore.InternalMods) 
        {
            var modPath = mod.Content.GetContentPath();
            var levelPath = Path.Combine(modPath, "Levels");
            var darkWorld = Path.Combine(levelPath, "DarkWorld");
            if (Directory.Exists(darkWorld)) 
            {
                foreach (string dir in Directory.EnumerateDirectories(darkWorld)) 
                {
                    LoadAdventureTowers(dir, mod.Metadata);
                }
                var mapXmlPath = Path.Combine(levelPath, "map.xml");
                if (!File.Exists(mapXmlPath)) 
                {
                    AdventureWorldMapRenderer.Add((false, null));
                    continue;
                }
                
                var xmlMapRenderer = new XmlMapRenderer(mapXmlPath, modPath);
                AdventureWorldMapRenderer.Add((true, xmlMapRenderer));
            }
        }
        if (AdventureWorldModTowers.Count > 0)
            AdventureWorldTowers = AdventureWorldModTowers[0];
    }


    /// <summary>
    /// Load Adventure towers by directory, and specify its metadata or null if it's global.
    /// </summary>
    /// <param name="directory">A directory path to the levels</param>
    /// <param name="mod">A mod metadata or null to categorize the level</param>
    /// <returns>A boolean determines whether the load success or fails</returns>
    public static bool LoadAdventureTowers(string directory, ModuleMetadata mod) 
    {
        string modName = mod.Name ?? "::global::";
        if (AdventureWorldModTowersLookup.TryGetValue(modName, out int id))
        {
            var tower = AdventureWorldModTowers[id];
            var adventureTowerDataOnCache = new AdventureWorldTowerData();
            if (adventureTowerDataOnCache.AdventureLoad(tower.Count, directory)) 
            {
                AdventureWorldModTowers[id].Add(adventureTowerDataOnCache);
                Logger.Verbose($"[Adventure] Added {directory} tower to {modName}.");
                return true;
            }
            return false;
        }
        var lookup = AdventureWorldModTowers.Count;
        AdventureWorldModTowersLookup.Add(modName, lookup);

        var adventureTowerData = new AdventureWorldTowerData();
        if (adventureTowerData.AdventureLoad(AdventureWorldTowers.Count, directory)) 
        {
            AdventureWorldModTowers.Add(new List<AdventureWorldTowerData>() { adventureTowerData });
            Logger.Verbose($"[Adventure] Added {directory} tower to {modName}.");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Load Adventure towers by directory, and specify its metadata or null if it's global.
    /// </summary>
    /// <param name="directory">A directory path to the levels</param>
    /// <param name="mod">A mod metadata or null to categorize the level</param>
    /// <returns>A boolean determines whether the load success or fails</returns>
    public static bool LoadAdventureLevelsParallel(string directory) 
    {
        return LoadAdventureTowers(directory, null);
    }
}

public class AdventureWorldTowerData : DarkWorldTowerData 
{
    public string StoredDirectory;
    public string Author;
    public bool Procedural;
    public int StartingLives = -1;
    public int[] MaxContinues = new int[3] { -1, -1, -1 };
    public string[] RequiredMods;
    public AdventureWorldTowerStats Stats;

    private (bool, string) Lookup(string directory) 
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
            Logger.Error($"[Adventure] {path}: Invalid icon size, it must be 16x16 dimension or 160x160 in level dimension");
            return;
        }
        Theme.Icon = new Subtexture(new Monocle.Texture(TowerMapData.BuildIcon(bitString, Theme.TowerType)));
    }

    internal bool AdventureLoad(int id, string levelDirectory) 
    {
        Levels = new List<string>();
        var (customIcon, pathToIcon) = Lookup(levelDirectory);
        return InternalAdventureLoad(id, levelDirectory, pathToIcon, customIcon);
    }

    internal bool InternalAdventureLoad(int id, string levelDirectory, string pathToIcon, bool customIcons = false) 
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
        Stats = AdventureModule.SaveData.AdventureWorld.AddOrGet(Theme.Name, levelDirectory);

        if (xmlElement.HasChild("lives")) 
        {
            StartingLives = int.Parse(xmlElement["lives"].InnerText);
        }
        if (xmlElement.HasChild("procedural"))
            Procedural = bool.Parse(xmlElement["procedural"].InnerText);
        if (xmlElement.HasChild("continues")) 
        {
            var continues = xmlElement["continues"];
            if (continues.HasChild("normal"))
                MaxContinues[0] = int.Parse(continues["normal"].InnerText);
            if (continues.HasChild("hardcore"))
                MaxContinues[1] = int.Parse(continues["hardcore"].InnerText);
            if (continues.HasChild("legendary"))
                MaxContinues[2] = int.Parse(continues["legendary"].InnerText);
        }

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;
using TeuJson;
using TowerFall;

namespace FortRise.Adventure;

public class AdventureWorldTowerData : DarkWorldTowerData 
{
    public RiseCore.ResourceSystem System;
    public string StoredDirectory;
    public string Author;
    public bool Procedural;
    public int StartingLives = -1;
    public int[] MaxContinues = new int[3] { -1, -1, -1 };
    public string[] RequiredMods;
    public AdventureWorldTowerStats Stats;

    public AdventureWorldTowerData(RiseCore.ResourceSystem system, string path) 
    {
        System = system;
        System.Open(path);
    }

    public AdventureWorldTowerData(RiseCore.ResourceSystem system) 
    {
        System = system;
    }

    private bool Lookup(string directory) 
    {
        bool customIcon = false;
        foreach (RiseCore.Resource resource in System.ListResource) 
        {
            var path = resource.Path;

            if (path.Contains("icon")) 
            {
                customIcon = true;
                continue;
            }
            if (path.EndsWith(".json") || path.EndsWith(".oel"))
                Levels.Add(path);
        }
        return customIcon;
    }

    private bool ModLookup(string directory) 
    {
        bool customIcon = false;
        foreach (RiseCore.Resource resource in System.MapResource[directory].Childrens) 
        {
            var path = resource.Path;

            if (path.Contains("icon")) 
            {
                customIcon = true;
                continue;
            }
            if (path.EndsWith(".json") || path.EndsWith(".oel"))
                Levels.Add(path);
        }
        return customIcon;
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
        var customIcon = Lookup(levelDirectory);
        return InternalAdventureLoad(id, levelDirectory, string.Empty, customIcon);
    }

    internal bool ModAdventureLoad(int id, string levelDirectory, string levelPrefix) 
    {
        Levels = new List<string>();
        var customIcon = ModLookup(levelPrefix.Remove(levelPrefix.Length - 1));
        return InternalAdventureLoad(id, levelDirectory, levelPrefix, customIcon);
    }

    private void LoadExtraData(XmlElement xmlElement) 
    {
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
    }

    internal bool InternalAdventureLoad(int id, string levelDirectory, string directoryPrefix, bool customIcons) 
    {
        if (this.Levels.Count <= 0) 
        {
            Logger.Error($"[Adventure] {levelDirectory} failed to load as there is no levels found.");
            return false;
        }

        StoredDirectory = levelDirectory;

        ID.X = id;
        using var fs = System.MapResource[directoryPrefix + "tower.xml"].Stream;
        var xmlElement =  patch_Calc.LoadXML(fs)["tower"];
        Theme = xmlElement.HasChild("theme") ? new patch_TowerTheme(xmlElement["theme"]) : patch_TowerTheme.GetDefault();
        Author = xmlElement.HasChild("author") ? xmlElement["author"].InnerText : string.Empty;
        Stats = AdventureModule.SaveData.AdventureWorld.AddOrGet(Theme.Name, levelDirectory);
        LoadExtraData(xmlElement);

        var guid = (Theme as patch_TowerTheme).GenerateThemeID();

        var pathToIcon = Path.Combine(levelDirectory, "icon.json");
        if (!string.IsNullOrEmpty(pathToIcon) && customIcons)
            BuildIcon(pathToIcon);
        
        LoadCustomElements(xmlElement["theme"], guid, directoryPrefix);

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

    private void LoadCustomElements(XmlElement element, Guid guid, string prefix) 
    {
        var fgTileset = element["Tileset"].InnerText.AsSpan();
        var bgTileset = element["BGTileset"].InnerText.AsSpan();
        var background = element["Background"].InnerText.AsSpan();

        if (fgTileset.StartsWith("custom:".AsSpan())) 
        {
            var sliced = fgTileset.Slice(7).ToString();
            var id = Path.Combine(StoredDirectory, sliced);
            var resource = System.MapResource[prefix + sliced];
            using var path = resource.Stream;
            var loadedXML = patch_Calc.LoadXML(path)["Tileset"];
            using var tilesetPath = System.MapResource[loadedXML.Attr("image")].Stream;
            patch_GameData.CustomTilesets.Add(id, patch_TilesetData.Create(loadedXML, tilesetPath));
            Theme.Tileset = id;
        }
        if (bgTileset.StartsWith("custom:".AsSpan())) 
        {
            var sliced = bgTileset.Slice(7).ToString();
            var id = Path.Combine(StoredDirectory, sliced);
            var resource = System.MapResource[prefix + sliced];
            using var path = resource.Stream;
            var loadedXML = patch_Calc.LoadXML(path)["Tileset"];
            using var tilesetPath = System.MapResource[prefix + loadedXML.Attr("image")].Stream;
            patch_GameData.CustomTilesets.Add(id, patch_TilesetData.Create(loadedXML, tilesetPath));
            Theme.BGTileset = id;
        }
        if (background.StartsWith("custom:".AsSpan())) 
        {
            var sliced = background.Slice(7).ToString();
            Theme.BackgroundID = sliced;
            LoadBG(sliced);
        }

        void LoadBG(string background) 
        {
            var path = System.MapResource[prefix + background].Stream;
            var loadedXML = patch_Calc.LoadXML(path)["BG"];

            // Old API
            if (loadedXML.HasChild("ImagePath")) 
            {
                var oldAPIPath = loadedXML.InnerText;
                Logger.Warning("[Background] Use of deprecated APIs should no longer be used");

                if (!string.IsNullOrEmpty(oldAPIPath)) 
                {
                    using var fs = System.MapResource[prefix + oldAPIPath].Stream;
                    var texture2D = Texture2D.FromStream(Engine.Instance.GraphicsDevice, fs);
                    var old_api_atlas = new patch_Atlas();
                    old_api_atlas.SetSubTextures(new Dictionary<string, Subtexture>() {{oldAPIPath, new Subtexture(new Monocle.Texture(texture2D)) }});
                    patch_GameData.CustomBGAtlas.Add(guid, new CustomBGStorage(old_api_atlas, null));
                }
                return;
            }

            // New API

            var customBGAtlas = loadedXML.Attr("atlas", null);
            var customSpriteDataAtlas = loadedXML.Attr("spriteData", null);
            
            patch_Atlas atlas = null;
            patch_SpriteData spriteData = null;
            if (customBGAtlas != null) 
            {
                var xml = System.MapResource[prefix + customBGAtlas + ".xml"].Stream;
                var png = System.MapResource[prefix + customBGAtlas + ".png"].Stream;
                atlas = AtlasExt.CreateAtlas(null, xml, png);
            }

            if (customSpriteDataAtlas != null) 
            {
                using var spriteTexture = System.MapResource[prefix + customSpriteDataAtlas + ".xml"].Stream;
                spriteData = SpriteDataExt.CreateSpriteData(null, spriteTexture, atlas);
            }

            var storage = new CustomBGStorage(atlas, spriteData);
            patch_GameData.CustomBGAtlas.Add(guid, storage);
            
            Theme.ForegroundData = loadedXML["Foreground"];
            Theme.BackgroundData = loadedXML["Background"];
        }
    }

    [MonoModIgnore]
    private extern List<DarkWorldTowerData.LevelData> LoadLevelSet(XmlElement xml);
}
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
    public ResourceSystem System;
    public AdventureWorldTowerData Tower;

    public XmlAdventureTowerLoader(ResourceSystem system, AdventureWorldTowerData towerData) 
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
            info.RequiredMods = patch_Calc.ChildStringArray(xmlElement, "required");
        else
            info.RequiredMods = Array.Empty<string>();

        return info;
    }

    public void LoadCustomElements(XmlElement element, Guid guid, string prefix) 
    {
        var fgTileset = element["Tileset"].InnerText.AsSpan();
        var bgTileset = element["BGTileset"].InnerText.AsSpan();
        var background = element["Background"].InnerText.AsSpan();

        if (fgTileset.StartsWith("custom:".AsSpan())) 
        {
            var sliced = fgTileset.Slice(7).ToString();
            var id = Path.Combine(Tower.StoredDirectory, sliced);
            var resource = System.MapResource[prefix + sliced];
            using var path = resource.Stream;
            var loadedXML = patch_Calc.LoadXML(path)["Tileset"];
            using var tilesetPath = System.MapResource[loadedXML.Attr("image")].Stream;
            patch_GameData.CustomTilesets.Add(id, patch_TilesetData.Create(loadedXML, tilesetPath));
            Tower.Theme.Tileset = id;
        }
        if (bgTileset.StartsWith("custom:".AsSpan())) 
        {
            var sliced = bgTileset.Slice(7).ToString();
            var id = Path.Combine(Tower.StoredDirectory, sliced);
            var resource = System.MapResource[prefix + sliced];
            using var path = resource.Stream;
            var loadedXML = patch_Calc.LoadXML(path)["Tileset"];
            using var tilesetPath = System.MapResource[prefix + loadedXML.Attr("image")].Stream;
            patch_GameData.CustomTilesets.Add(id, patch_TilesetData.Create(loadedXML, tilesetPath));
            Tower.Theme.BGTileset = id;
        }
        if (background.StartsWith("custom:".AsSpan())) 
        {
            var sliced = background.Slice(7).ToString();
            Tower.Theme.BackgroundID = sliced;
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
            
            Tower.Theme.ForegroundData = loadedXML["Foreground"];
            Tower.Theme.BackgroundData = loadedXML["Background"];
        }
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
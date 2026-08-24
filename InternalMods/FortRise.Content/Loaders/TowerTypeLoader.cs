using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;

namespace FortRise.Content;


public static class TowerTypeLoader
{
    internal static ITowerTypeEntry LoadTowerType(IModRegistry registry, IModContent content, XmlElement xml)
    {
        var id = xml.Attr("id");
        var blockTexture = content.LoadTexture(registry, xml["BlockTexture"], SubtextureAtlasDestination.MenuAtlas);
        var smallBlockTexture = content.LoadTexture(registry, xml["SmallBlockTexture"], SubtextureAtlasDestination.MenuAtlas);
        var tint = xml.ChildHexColor("TintColor", Color.White);
        var sfxID = xml.ChildText("TowerSound", "ui_move2");
        var sound = registry.SFXs.GetSFXEntryWithRelative(sfxID)
            ?? throw new Exception($"Cannot find <TowerSound> id: {sfxID}");

        return registry.Towers.RegisterTowerType(id, new()
        {
            BlockTexture = blockTexture,
            SmallBlockTexture = smallBlockTexture,
            Tint = tint,
            TowerSound = sound
        });
    }

    internal static List<ITowerTypeEntry> LoadTowerTypes(IModRegistry registry, IModContent content, XmlElement xml)
    {
        var list = new List<ITowerTypeEntry>();
        foreach (XmlElement xmlTileset in xml.GetElementsByTagName("TowerType"))
        {
            list.Add(LoadTowerType(registry, content, xmlTileset));
        }

        return list;
    }

    internal static void Load(IModRegistry registry, IModContent content, Loader? loader)
    {
        loader ??= new Loader() { Path = ["Content/Atlas/GameData/towerTypeData.xml"] };

        if (loader.Path is null || !loader.Enabled)
        {
            return;
        }

        List<IResourceInfo> resources = [];
        
        foreach (var path in loader.Path)
        {
            resources.AddRange(content.Root.EnumerateChildrens(path));
        }

        foreach (var res in resources)
        {
            var tilesetRes = res.Xml ??
                throw new Exception($"[{content.Metadata.Name}] Failed to load Xml file {res.Path}.");

            var xml = tilesetRes["TowerTypeData"];

            if (xml is null)
            {
                continue;
            }

            LoadTowerTypes(registry, content, xml);
        }
    }
}


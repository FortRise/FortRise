using System;
using System.Collections.Generic;
using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise.Content;

internal static class TilesetLoader
{
    internal static ITilesetEntry LoadTileset(IModRegistry registry, IModContent content, XmlElement xmlTileset)
    {
        var themeID = xmlTileset.Attr("id");

        var image = xmlTileset.Attr("image");
        ISubtextureEntry texture = null!;

        if (content.Root.TryGetRelativePath(image, out var info))
        {
            texture = registry.Subtextures.RegisterTexture(info);
        }
        else
        {
            texture = registry.Subtextures.RegisterTexture(() => TFGame.Atlas[image]);
        }

        return registry.Tilesets.RegisterTileset(themeID, new()
        {
            Texture = texture,
            AutotileData = new AutotileData(xmlTileset)
        });
    }

    internal static IList<ITilesetEntry> LoadTilesets(IModRegistry registry, IModContent content, XmlElement xml)
    {
        var list = new List<ITilesetEntry>();
        foreach (XmlElement xmlTileset in xml)
        {
            list.Add(LoadTileset(registry, content, xmlTileset));
        }

        return list;
    }

    internal static void Load(IModRegistry registry, IModContent content, IFortRiseContentApi.ILoaderAPI.ILoader? loader)
    {
        loader ??= new Loader() { Path = ["Content/Atlas/GameData/tilesetData.xml"] };

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

            var xml = tilesetRes["TilesetData"];

            if (xml is null)
            {
                continue;
            }

            LoadTilesets(registry, content, xml);
        }
    }
}

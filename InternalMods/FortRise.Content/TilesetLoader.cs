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

    internal static void Load(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Content/Atlas/GameData/tilesetData.xml", out IResourceInfo theme))
        {
            return;
        }

        var xml = theme.Xml?["TilesetData"];
        if (xml is null)
        {
            return;
        }

        LoadTilesets(registry, content, xml);
    }
}

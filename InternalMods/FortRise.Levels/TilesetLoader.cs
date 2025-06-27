using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise.Levels;

internal static class TilesetLoader
{
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

        foreach (XmlElement xmlTileset in xml)
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

            registry.Tilesets.RegisterTileset(themeID, new()
            {
                Texture = texture,
                AutotileData = new AutotileData(xmlTileset)
            });
        }
    }
}

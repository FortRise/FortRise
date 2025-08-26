using System;
using System.Collections.Generic;
using System.Xml;

namespace FortRise.Content;

internal sealed partial class ApiImplementation
{
    internal sealed class TilesetsAPI : IFortRiseContentApi.ITilesetsAPI
    {
        public ITilesetEntry RegisterTilesetWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource)
        {
            var themeXml = xmlResource.Xml ?? throw new Exception($"[{content.Metadata.Name}] Invalid or null Xml file.)");

            var xml = themeXml["TilesetData"] ?? throw new Exception($"[{content.Metadata.Name}] Missing TilesetData element.");

            return TilesetLoader.LoadTileset(registry, content, xml);
        }

        public ITilesetEntry RegisterTilesetWithXml(IModContent content, IModRegistry registry, XmlElement xml)
        {
            return TilesetLoader.LoadTileset(registry, content, xml);
        }

        public IList<ITilesetEntry> RegisterTilesetsWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource)
        {
            var themeXml = xmlResource.Xml ?? throw new Exception($"[{content.Metadata.Name}] Invalid or null Xml file.)");

            var xml = themeXml["TilesetData"] ?? throw new Exception($"[{content.Metadata.Name}] Missing TilesetData element.");

            return TilesetLoader.LoadTilesets(registry, content, xml);
        }

        public IList<ITilesetEntry> RegisterTilesetsWithXml(IModContent content, IModRegistry registry, XmlElement xml)
        {
            return TilesetLoader.LoadTilesets(registry, content, xml);
        }
    }
}


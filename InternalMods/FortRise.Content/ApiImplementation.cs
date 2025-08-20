using System;
using System.Collections.Generic;
using System.Xml;
using Monocle;

namespace FortRise.Content;

internal sealed class ApiImplementation : IFortRiseContentApi
{
    public ApiImplementation() {}

    public IFortRiseContentApi.IArcherAPI Archers { get; } = new ArcherAPI();
    public IFortRiseContentApi.ITilesetsAPI Tilesets { get; } = new TilesetsAPI();

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

    internal sealed class ArcherAPI : IFortRiseContentApi.IArcherAPI
    {
        public IArcherEntry RegisterArcherWithXml(string id, IModContent content, IModRegistry registry, XmlElement xml)
        {
            return ArcherLoader.LoadArcher(registry, content, xml, id, default);
        }

        public IArcherEntry RegisterArcherWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource)
        {
            var xml = xmlResource.Xml ?? throw new Exception($"[{content.Metadata.Name}] Invalid or null Xml file.)");

            var archer = xml["Archer"] ??
                throw new Exception($"[{content.Metadata.Name}] Missing Archers element.");

            var id = archer.Attr("id");

            return RegisterArcherWithXml(id, content, registry, archer);
        }

        public IList<IArcherEntry> RegisterArchersWithXml(IModContent content, IModRegistry registry, XmlDocument xml)
        {
            return ArcherLoader.LoadAll(content, registry, xml);
        }

        public IList<IArcherEntry> RegisterArchersWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource)
        {
            var xml = xmlResource.Xml ?? throw new Exception($"[{content.Metadata.Name}] Invalid or null Xml file.)");
            return RegisterArchersWithXml(content, registry, xml);
        }

    }
}

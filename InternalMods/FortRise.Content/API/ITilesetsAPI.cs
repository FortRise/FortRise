using System.Collections.Generic;
using System.Xml;

namespace FortRise.Content;

public partial interface IFortRiseContentApi 
{
    public interface ITilesetsAPI
    {
        IList<ITilesetEntry> RegisterTilesetsWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource);
        IList<ITilesetEntry> RegisterTilesetsWithXml(IModContent content, IModRegistry registry, XmlElement xml);
        ITilesetEntry RegisterTilesetWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource);
        ITilesetEntry RegisterTilesetWithXml(IModContent content, IModRegistry registry, XmlElement xml);
    }
}


using System.Collections.Generic;
using System.Xml;

namespace FortRise.Content;

public partial interface IFortRiseContentApi 
{
    public interface IThemeAPI
    {
        IList<IThemeEntry> RegisterThemesWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource);
        IList<IThemeEntry> RegisterThemesWithXml(IModContent content, IModRegistry registry, XmlElement xml);
        IThemeEntry RegisterThemeWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource);
        IThemeEntry RegisterThemeWithXml(IModContent content, IModRegistry registry, XmlElement xml);
    }
}



using System;
using System.Collections.Generic;
using System.Xml;
using Monocle;

namespace FortRise.Content;

internal sealed partial class ApiImplementation
{
    internal sealed class ThemeAPI : IFortRiseContentApi.IThemeAPI
    {
        public IList<IThemeEntry> RegisterThemesWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource)
        {
            var themeXml = xmlResource.Xml ?? throw new Exception($"[{content.Metadata.Name}] Invalid or null Xml file.)");

            var xml = themeXml["ThemeData"] ?? throw new Exception($"[{content.Metadata.Name}] Missing ThemeData element.");

            return ThemeLoader.LoadThemes(registry, content, xml);
        }

        public IList<IThemeEntry> RegisterThemesWithXml(IModContent content, IModRegistry registry, XmlElement xml)
        {
            return ThemeLoader.LoadThemes(registry, content, xml);
        }

        public IThemeEntry RegisterThemeWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource)
        {
            var themeXml = xmlResource.Xml ?? throw new Exception($"[{content.Metadata.Name}] Invalid or null Xml file.)");
            var xml = themeXml["Theme"] ?? throw new Exception($"[{content.Metadata.Name}] Missing Theme element.");
            return ThemeLoader.LoadTheme(xml.Attr("id"), xml, content, registry);
        }

        public IThemeEntry RegisterThemeWithXml(IModContent content, IModRegistry registry, XmlElement xml)
        {
            return ThemeLoader.LoadTheme(xml.Attr("id"), xml, content, registry);
        }
    }

    internal sealed class VersusLoaderAPI : IFortRiseContentApi.IVersusLoaderAPI
    {
        public IVersusTowerEntry RegisterVersusTowerWithXml(IModContent content, IModRegistry registry, XmlElement towerXml, string levelID, string towerSet, IResourceInfo[] levels)
        {
            return VersusLoader.LoadTowerXml(registry, content, towerXml, levelID, towerSet, levels);
        }
    }
}



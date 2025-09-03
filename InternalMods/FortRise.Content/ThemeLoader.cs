using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise.Content;

internal static class ThemeLoader
{
    internal static void Load(IModRegistry registry, IModContent content, IFortRiseContentApi.ILoaderAPI.ILoader? loader)
    {
        loader ??= new Loader() { Path = ["Content/Atlas/GameData/themeData.xml"] };

        if (loader.Path is null || !loader.Enabled)
        {
            return;
        }

        List<IResourceInfo> resources = [];
        
        foreach (var path in loader.Path)
        {
            resources.AddRange(content.Root.EnumerateChildrens(path));
        }

        foreach (var theme in resources)
        {
            var xml = theme.Xml?["ThemeData"];
            if (xml is null)
            {
                return;
            }

            foreach (XmlElement xmlTheme in xml)
            {
                var icon = xmlTheme.ChildText("Icon");
                ISubtextureEntry subIcon = null!;

                if (content.Root.TryGetRelativePath(icon, out var info))
                {
                    subIcon = registry.Subtextures.RegisterTexture(info);
                }
                else
                {
                    subIcon = registry.Subtextures.RegisterTexture(() => TFGame.MenuAtlas["towerIcons/" + icon]);
                }

                var themeID = xmlTheme.Attr("id", xmlTheme.Name);
                LoadTheme(themeID, xmlTheme, content, registry);
            }
        }
    }

    public static string LoadInlineTheme(XmlElement xml, IModContent content, IModRegistry registry)
    {
        if (xml.HasChild("theme"))
        {
            var xmlTheme = xml["theme"];
            if (xmlTheme.HasChild("Name"))
            {
                return LoadTheme(Guid.CreateVersion7().ToString(), xmlTheme!, content, registry);
            }
            else
            {
                return xml.ChildText("theme").Trim();
            }
        }
        
        return "SacredGround";
    }

    public static string LoadTheme(string id, XmlElement xmlTheme, IModContent content, IModRegistry registry)
    {
        // load inline themes
        var icon = xmlTheme.ChildText("Icon");
        ISubtextureEntry subIcon = null!;

        if (content.Root.TryGetRelativePath(icon, out var info))
        {
            subIcon = registry.Subtextures.RegisterTexture(info);
        }
        else
        {
            subIcon = registry.Subtextures.RegisterTexture(() => TFGame.MenuAtlas["towerIcons/" + icon]);
        }

        var themeLoaded = registry.Themes.RegisterTheme(id, new()
        {
            Name = xmlTheme.ChildText("Name").Trim().ToUpperInvariant(),
            Icon = subIcon,
            TowerType = xmlTheme.ChildEnum<MapButton.TowerType>("TowerType", MapButton.TowerType.Normal),
            MapPosition = xmlTheme!["MapPosition"].Position(),
            Music = xmlTheme.ChildText("Music", string.Empty),
            DarknessColor = xmlTheme.ChildHexColor("DarknessColor", Color.Black),
            DarknessOpacity = xmlTheme.ChildFloat("DarknessOpacity", 0.2f),
            Wind = xmlTheme.ChildInt("Wind", 0),
            Lanterns = xmlTheme.ChildEnum<TowerTheme.LanternTypes>("Lanterns", TowerTheme.LanternTypes.CathedralTorch),
            World = xmlTheme.ChildEnum<TowerTheme.Worlds>("World", TowerTheme.Worlds.Normal),
            Raining = xmlTheme.ChildBool("Raining", false),
            BackgroundID = xmlTheme.ChildText("Background"),
            DrillParticleColor = xmlTheme.ChildHexColor("DrillParticleColor", Color.Red),
            Cold = xmlTheme.ChildBool("Cold", false),
            CrackedBlockColor = xmlTheme.ChildHexColor("CrackedBlockColor", "4EB1E9"),
            Tileset = xmlTheme.ChildText("Tileset", "SacredGround"),
            BGTileset = xmlTheme.ChildText("BGTileset", "SacredGroundBG"),
            Cataclysm = xmlTheme.ChildBool("Cataclysm", xmlTheme.ChildText("Tileset") == "Cataclysm")
        });

        return themeLoaded.Name;
    }
}

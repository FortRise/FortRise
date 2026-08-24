using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise.Content;

internal static class ThemeLoader
{
    internal static void Load(IModRegistry registry, IModContent content, Loader? loader)
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
                var icon = xmlTheme["Icon"];
                ISubtextureEntry? subIcon = null;

                try
                {
                    subIcon = content.LoadTexture(registry, icon, SubtextureAtlasDestination.MenuAtlas);
                }
                catch (TextureNotFoundException)
                {
                    string text = icon!.InnerText.Trim();
                    subIcon = registry.Subtextures.RegisterTexture(() => TFGame.MenuAtlas["towerIcons/" + text]);
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
                var theme = LoadTheme(Guid.CreateVersion7().ToString(), xmlTheme!, content, registry);
                return theme.Name;
            }
            else
            {
                return xml.ChildTextWithRelative("theme", "SacredGround").Trim();
            }
        }
        
        return "SacredGround";
    }

    public static List<IThemeEntry> LoadThemes(IModRegistry registry, IModContent content, XmlElement xml)
    {
        var list = new List<IThemeEntry>();

        foreach (XmlElement x in xml)
        {
            list.Add(LoadTheme(x.Attr("id"), x, content, registry));
        }

        return list;
    }

    public static IThemeEntry LoadTheme(string id, XmlElement xmlTheme, IModContent content, IModRegistry registry)
    {
        // load inline themes
        var icon = xmlTheme["Icon"];
        ISubtextureEntry? subIcon = null;

        try
        {
            subIcon = content.LoadTexture(registry, icon, SubtextureAtlasDestination.MenuAtlas);
        }
        catch (TextureNotFoundException)
        {
            var text = icon!.InnerText.Trim();
            subIcon = registry.Subtextures.RegisterTexture(() => TFGame.MenuAtlas["towerIcons/" + text]);
        }

        string musicID = xmlTheme.ChildText("Music", string.Empty).Trim();

        // validate if this music exists
        string musicName = string.Empty;
        var music = registry.Musics.GetMusicWithRelative(musicID);
        if (music is null)
        {
            musicName = musicID;
        }
        else 
        {
            musicName = music.Name;
        }

        var themeLoaded = registry.Themes.RegisterTheme(id, new()
        {
            Name = xmlTheme.ChildText("Name").Trim().ToUpperInvariant(),
            Icon = subIcon,
            TowerType = xmlTheme.ChildEnum("TowerType", MapButton.TowerType.Normal),
            MapPosition = xmlTheme!["MapPosition"].Position(),
            Music = musicName,
            DarknessColor = xmlTheme.ChildHexColor("DarknessColor", Color.Black),
            DarknessOpacity = xmlTheme.ChildFloat("DarknessOpacity", 0.2f),
            Wind = xmlTheme.ChildInt("Wind", 0),
            Lanterns = xmlTheme.ChildEnum("Lanterns", TowerTheme.LanternTypes.CathedralTorch),
            World = xmlTheme.ChildEnum("World", TowerTheme.Worlds.Normal),
            Raining = xmlTheme.ChildBool("Raining", false),
            BackgroundID = xmlTheme.ChildTextWithRelative("Background", "SacredGround"),
            DrillParticleColor = xmlTheme.ChildHexColor("DrillParticleColor", Color.Red),
            Cold = xmlTheme.ChildBool("Cold", false),
            CrackedBlockColor = xmlTheme.ChildHexColor("CrackedBlockColor", "4EB1E9"),
            Tileset = xmlTheme.ChildTextWithRelative("Tileset", "SacredGround"),
            BGTileset = xmlTheme.ChildTextWithRelative("BGTileset", "SacredGroundBG"),
            Cataclysm = xmlTheme.ChildBool("Cataclysm", xmlTheme.ChildText("Tileset") == "Cataclysm")
        });

        return themeLoaded;
    }
}

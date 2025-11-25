using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;

namespace FortRise.Content;

internal static class BackgroundLoader
{
    internal static void Load(IModRegistry registry, IModContent content, Loader? loader)
    {
        loader ??= new Loader() { Path = ["Content/Atlas/GameData/bgData.xml"] };

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
            var bgDataXml = res.Xml ??
                throw new Exception($"[{content.Metadata.Name}] Failed to load Xml file {res.Path}.");

            LoadAll(content, registry, bgDataXml);
        }
    }

    internal static IList<IBackgroundEntry> LoadAll(IModContent content, IModRegistry registry, XmlDocument xml)
    {
        var backgrounds = xml["backgrounds"]
            ?? throw new Exception("Cannot find <backgrounds> element.");

        foreach (XmlElement bgXml in backgrounds.GetElementsByTagName("BG"))
        {
            var id = bgXml.Attr("id");

            var bgLayers = new List<BGLayer>();
            var fgLayers = new List<BGLayer>();
            Color bgColor;
            var background = bgXml["Background"];
            if (background is null)
            {
                bgColor = Color.White;
            }
            else
            {
                bgColor = background.AttrHexColor("bgColor");
                Load(bgLayers, background);
            }

            var foreground = bgXml["Foreground"];
            if (foreground is not null)
            {
                Load(fgLayers, foreground);
            }

            registry.Backgrounds.RegisterBackground(id, new()
            {
                BackgroundColor = bgColor,
                Background = [.. bgLayers],
                Foreground = [.. fgLayers]
            });
        }
        return [];

        void Load(List<BGLayer> bgLayers, XmlElement xml)
        {
            foreach (object b in xml)
            {
                if (b is not XmlElement elm)
                {
                    continue;
                }

                bgLayers.Add(NestedChildren(elm));

                BGLayer NestedChildren(XmlElement elm)
                {
                    var name = elm.Name;
                    Dictionary<string, object> attributes = [];
                    foreach (XmlAttribute attr in elm.Attributes)
                    {
                        attributes.Add(attr.Name, attr.Value);
                    }

                    string? singleChildren = null;
                    List<BGLayer>? layers = null;

                    if (elm.HasChildNodes && elm.ChildNodes[0] is XmlText)
                    {
                        // we need to guess
                        singleChildren = elm.InnerText;

                        if (content.Root.TryGetRelativePath(singleChildren, out var res))
                        {
                            // try to load that texture
                            singleChildren = content.LoadTexture(registry, res.Path, SubtextureAtlasDestination.BGAtlas).ID;
                        }
                        else
                        {
                            // if that does not work, maybe its a sprite? (its usually int)
                            var sprite = registry.Sprites.GetBGSpriteEntry<int>(EntryExtensions.ResolveID(singleChildren));

                            if (sprite is not null)
                            {
                                singleChildren = sprite.GetCastEntry<int>().ID;
                            }
                            // just keep it as is if all does not work
                        }
                    }
                    else
                    {
                        layers = [];
                        foreach (var nested in elm)
                        {
                            if (nested is not XmlElement o)
                            {
                                continue;
                            }
                            layers.Add(NestedChildren(o));
                        }
                    }

                    var bgLayer = new BGLayer()
                    {
                        Name = name,
                        Data = attributes,
                        SingleChildren = singleChildren,
                        Childrens = layers?.ToArray()
                    };

                    return bgLayer;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;

namespace FortRise.Content;

internal static class MapRendererLoader 
{
    internal static void Load(IModRegistry registry, IModContent content, Loader? loader)
    {
        loader ??= new Loader() { Path = ["Content/Atlas/GameData/mapData.xml"] };

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
            var mapDataXml = res.Xml ??
                throw new Exception($"[{content.Metadata.Name}] Failed to load Xml file {res.Path}.");
            
            LoadAll(content, registry, mapDataXml);
        }
    }

    /*
     * <MapData id="mapID" hideVanillaElements="False">
     *  <Land image="path/to/image.png"/>
     *  <Water image="path/to/image.png"/>
     *
     *  <Elements>
     *   <Static image="path/to/image.png" x="40" y="80"/>
     *   <Animated sprite="ModName/SpriteName" x="90" y="120"/>
     *  </Elements>
     * </MapData>
     */

    private static void LoadAll(IModContent content, IModRegistry registry, XmlDocument doc)
    {
        var mapData = doc["MapData"];
        if (mapData is null)
        {
            return;
        }
        string id = mapData.Attr("id");
        string levelSet = mapData.Attr("levelSet", id);

        int width = mapData.AttrInt("width", -1);
        int height = mapData.AttrInt("height", -1);
        bool hideVanilla = mapData.AttrBool("hideVanillaElements", false);

        ISubtextureEntry? water = null;
        if (mapData.HasChild("Water"))
        {
            string waterStr = mapData["Water"]!.Attr("image");
            water = content.LoadTexture(registry, waterStr, SubtextureAtlasDestination.MenuAtlas);
        }

        ISubtextureEntry? land = null;
        if (mapData.HasChild("Land"))
        {
            string landStr = mapData["Land"]!.Attr("image");
            land = content.LoadTexture(registry, landStr, SubtextureAtlasDestination.MenuAtlas);
        }

        List<MapElement> mapElements = [];
        var elements = mapData["Elements"];

        if (elements is not null)
        {
            foreach (object elm in elements.ChildNodes)
            {
                if (elm is not XmlElement xml)
                {
                    continue;
                }

                if (xml.Name == "Static")
                {
                    int x = xml.AttrInt("x", 0);
                    int y = xml.AttrInt("y", 0);
                    var image = xml.Attr("image");
                    var subtexture = content.LoadTexture(registry, image, SubtextureAtlasDestination.MenuAtlas);

                    mapElements.Add(new() 
                    {
                        Sprite = new(subtexture),
                        Position = new Vector2(x, y)
                    });
                }
                else if (xml.Name == "Animated")
                {
                    int x = xml.AttrInt("x", 0);
                    int y = xml.AttrInt("y", 0);
                    var sprite = xml.Attr("sprite");

                    var menuSprite = registry.Sprites.GetMenuSpriteEntry<string>(sprite);
                    menuSprite ??= registry.Sprites.GetMenuSpriteEntry<string>($"{content.Metadata.Name}/{sprite}")
                        ?? throw new Exception($"Menu Sprite: '{sprite}' cannot be found");

                    string inAnimation = xml.ChildText("In", "in").Trim();
                    string outAnimation = xml.ChildText("Out", "out").Trim();
                    string selectedAnimation = xml.ChildText("Selected", "selected").Trim();
                    string notSelectedAnimation = xml.ChildText("NotSelected", "notSelected").Trim();

                    string towerID = xml.ChildText("TowerID", null).Trim();

                    mapElements.Add(new() 
                    {
                        Sprite = new AnimatedTowerConfiguration() 
                        {
                            In = inAnimation,
                            Out = outAnimation,
                            NotSelected = notSelectedAnimation,
                            Selected = selectedAnimation,
                            Sprite = menuSprite,
                            TowerID = towerID
                        },
                        Position = new Vector2(x, y)
                    });
                }
            }
        }


        registry.MapRenderers.RegisterMapRenderer(id, new() 
        {
            LevelSet = levelSet,
            Water = water,
            Land = land,
            Width = width <= -1 ? Option<int>.None() : width,
            Height = height <= -1 ? Option<int>.None() : height,
            HideVanillaElements = hideVanilla,
            Elements = [.. mapElements]
        });
}
}


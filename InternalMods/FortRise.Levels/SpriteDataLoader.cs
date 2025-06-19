using System;
using System.Collections.Generic;
using System.Xml;
using Monocle;

namespace FortRise.Levels;

internal static class SpriteDataLoader
{
    internal static void LoadCorpseSpriteData(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Atlas/SpriteData/corpseSpriteData.xml", out IResourceInfo spriteDataRes))
        {
            return;
        }

        var spriteDataXml = spriteDataRes.Xml ??
            throw new Exception($"[{content.Metadata.Name}] Content/Atlas/GameData/corpseSpriteData.xml cannot be read.");
        var spriteData = spriteDataXml["SpriteData"] ??
            throw new Exception($"[{content.Metadata.Name}] Missing SpriteData element.");

        foreach (var sprite in spriteData)
        {
            if (sprite is not XmlElement element)
            {
                continue;
            }

            HashSet<string> nonExtra = new HashSet<string>()
            {
                "Texture",
                "FrameWidth",
                "FrameHeight",
                "OriginX",
                "OriginY",
                "X",
                "Y",
                "Animations"
            };

            if (element.Name == "sprite_string")
            {
                var id = element.Attr("id");
                var texture = registry.Subtextures.RegisterTexture(
                    content.Root.GetRelativePath(element.ChildText("Texture"))
                );

                var animationsXml = element["Animations"] ??
                    throw new Exception($"[{content.Metadata.Name}] Missing Animations element.");

                List<Animation<string>> animns = new List<Animation<string>>();

                foreach (var animationXml in animationsXml)
                {
                    if (animationXml is XmlElement animElm)
                    {
                        animns.Add(
                            new()
                            {
                                ID = animElm.Attr("id"),
                                Loop = animElm.AttrBool("loop", false),
                                Delay = animElm.AttrFloat("delay", 0),
                                Frames = Calc.ReadCSVInt(animElm.Attr("frames"))
                            }
                        );
                    }
                }

                var additionalData = new Dictionary<string, object>();

                foreach (var data in element)
                {
                    if (data is XmlElement elm && !nonExtra.Contains(elm.Name))
                    {
                        additionalData.Add(elm.Name, elm.InnerText.Trim());
                    }
                }

                registry.Sprites.RegisterCorpseSprite<string>(id, new()
                {
                    Texture = texture,
                    FrameWidth = element.ChildInt("FrameWidth"),
                    FrameHeight = element.ChildInt("FrameHeight"),
                    OriginX = element.ChildInt("OriginX", 0),
                    OriginY = element.ChildInt("OriginY", 0),
                    X = element.ChildInt("X", 0),
                    Y = element.ChildInt("Y", 0),
                    Animations = animns.ToArray(),
                    AdditionalData = additionalData
                });
            }
            else if (element.Name == "sprite_int")
            {
                var id = element.Attr("id");
                var texture = registry.Subtextures.RegisterTexture(
                    content.Root.GetRelativePath(element.ChildText("Texture")),
                    SubtextureAtlasDestination.Atlas
                );

                var animationsXml = element["Animations"] ??
                    throw new Exception($"[{content.Metadata.Name}] Missing Animations element.");

                List<Animation<int>> animns = new List<Animation<int>>();

                foreach (var animationXml in animationsXml)
                {
                    if (animationXml is XmlElement animElm)
                    {
                        animns.Add(
                            new()
                            {
                                ID = animElm.AttrInt("id"),
                                Loop = animElm.AttrBool("loop", false),
                                Delay = animElm.AttrFloat("delay", 0),
                                Frames = Calc.ReadCSVInt(animElm.Attr("frames"))
                            }
                        );
                    }
                }

                var additionalData = new Dictionary<string, object>();

                foreach (var data in element)
                {
                    if (data is XmlElement elm && !nonExtra.Contains(elm.Name))
                    {
                        additionalData.Add(elm.Name, elm.InnerText.Trim());
                    }
                }

                registry.Sprites.RegisterCorpseSprite<int>(id, new()
                {
                    Texture = texture,
                    FrameWidth = element.ChildInt("FrameWidth"),
                    FrameHeight = element.ChildInt("FrameHeight"),
                    OriginX = element.ChildInt("OriginX", 0),
                    OriginY = element.ChildInt("OriginY", 0),
                    X = element.ChildInt("X", 0),
                    Y = element.ChildInt("Y", 0),
                    Animations = animns.ToArray(),
                    AdditionalData = additionalData
                });
            }
        }
    }

    internal static void LoadMenuSpriteData(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Atlas/SpriteData/menuSpriteData.xml", out IResourceInfo spriteDataRes))
        {
            return;
        }

        var spriteDataXml = spriteDataRes.Xml ??
            throw new Exception($"[{content.Metadata.Name}] Content/Atlas/GameData/menuSpriteData.xml cannot be read.");
        var spriteData = spriteDataXml["SpriteData"] ??
            throw new Exception($"[{content.Metadata.Name}] Missing SpriteData element.");

        foreach (var sprite in spriteData)
        {
            if (sprite is not XmlElement element)
            {
                continue;
            }

            HashSet<string> nonExtra = new HashSet<string>()
            {
                "Texture",
                "FrameWidth",
                "FrameHeight",
                "OriginX",
                "OriginY",
                "X",
                "Y",
                "Animations"
            };

            if (element.Name == "sprite_string")
            {
                var id = element.Attr("id");
                var texture = registry.Subtextures.RegisterTexture(
                    content.Root.GetRelativePath(element.ChildText("Texture")),
                    SubtextureAtlasDestination.MenuAtlas
                );

                var animationsXml = element["Animations"] ??
                    throw new Exception($"[{content.Metadata.Name}] Missing Animations element.");

                List<Animation<string>> animns = new List<Animation<string>>();

                foreach (var animationXml in animationsXml)
                {
                    if (animationXml is XmlElement animElm)
                    {
                        animns.Add(
                            new()
                            {
                                ID = animElm.Attr("id"),
                                Loop = animElm.AttrBool("loop", false),
                                Delay = animElm.AttrFloat("delay", 0),
                                Frames = Calc.ReadCSVInt(animElm.Attr("frames"))
                            }
                        );
                    }
                }

                var additionalData = new Dictionary<string, object>();

                foreach (var data in element)
                {
                    if (data is XmlElement elm && !nonExtra.Contains(elm.Name))
                    {
                        additionalData.Add(elm.Name, elm.InnerText.Trim());
                    }
                }

                registry.Sprites.RegisterMenuSprite<string>(id, new()
                {
                    Texture = texture,
                    FrameWidth = element.ChildInt("FrameWidth"),
                    FrameHeight = element.ChildInt("FrameHeight"),
                    OriginX = element.ChildInt("OriginX", 0),
                    OriginY = element.ChildInt("OriginY", 0),
                    X = element.ChildInt("X", 0),
                    Y = element.ChildInt("Y", 0),
                    Animations = animns.ToArray(),
                    AdditionalData = additionalData
                });
            }
            else if (element.Name == "sprite_int")
            {
                var id = element.Attr("id");
                var texture = registry.Subtextures.RegisterTexture(
                    content.Root.GetRelativePath(element.ChildText("Texture")),
                    SubtextureAtlasDestination.MenuAtlas
                );

                var animationsXml = element["Animations"] ??
                    throw new Exception($"[{content.Metadata.Name}] Missing Animations element.");

                List<Animation<int>> animns = new List<Animation<int>>();

                foreach (var animationXml in animationsXml)
                {
                    if (animationXml is XmlElement animElm)
                    {
                        animns.Add(
                            new()
                            {
                                ID = animElm.AttrInt("id"),
                                Loop = animElm.AttrBool("loop", false),
                                Delay = animElm.AttrFloat("delay", 0),
                                Frames = Calc.ReadCSVInt(animElm.Attr("frames"))
                            }
                        );
                    }
                }

                var additionalData = new Dictionary<string, object>();

                foreach (var data in element)
                {
                    if (data is XmlElement elm && !nonExtra.Contains(elm.Name))
                    {
                        additionalData.Add(elm.Name, elm.InnerText.Trim());
                    }
                }

                registry.Sprites.RegisterMenuSprite<int>(id, new()
                {
                    Texture = texture,
                    FrameWidth = element.ChildInt("FrameWidth"),
                    FrameHeight = element.ChildInt("FrameHeight"),
                    OriginX = element.ChildInt("OriginX", 0),
                    OriginY = element.ChildInt("OriginY", 0),
                    X = element.ChildInt("X", 0),
                    Y = element.ChildInt("Y", 0),
                    Animations = animns.ToArray(),
                    AdditionalData = additionalData
                });
            }
        }
    }

    internal static void LoadSpriteData(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Atlas/SpriteData/spriteData.xml", out IResourceInfo spriteDataRes))
        {
            return;
        }

        var spriteDataXml = spriteDataRes.Xml ??
            throw new Exception($"[{content.Metadata.Name}] Content/Atlas/GameData/spriteData.xml cannot be read.");
        var spriteData = spriteDataXml["SpriteData"] ??
            throw new Exception($"[{content.Metadata.Name}] Missing SpriteData element.");

        foreach (var sprite in spriteData)
        {
            if (sprite is not XmlElement element)
            {
                continue;
            }

            HashSet<string> nonExtra = new HashSet<string>()
            {
                "Texture",
                "FrameWidth",
                "FrameHeight",
                "OriginX",
                "OriginY",
                "X",
                "Y",
                "Animations"
            };

            if (element.Name == "sprite_string")
            {
                var id = element.Attr("id");
                var texture = registry.Subtextures.RegisterTexture(
                    content.Root.GetRelativePath(element.ChildText("Texture"))
                );

                var animationsXml = element["Animations"] ??
                    throw new Exception($"[{content.Metadata.Name}] Missing Animations element.");

                List<Animation<string>> animns = new List<Animation<string>>();

                foreach (var animationXml in animationsXml)
                {
                    if (animationXml is XmlElement animElm)
                    {
                        animns.Add(
                            new()
                            {
                                ID = animElm.Attr("id"),
                                Loop = animElm.AttrBool("loop", false),
                                Delay = animElm.AttrFloat("delay", 0),
                                Frames = Calc.ReadCSVInt(animElm.Attr("frames"))
                            }
                        );
                    }
                }

                var additionalData = new Dictionary<string, object>();

                foreach (var data in element)
                {
                    if (data is XmlElement elm && !nonExtra.Contains(elm.Name))
                    {
                        additionalData.Add(elm.Name, elm.InnerText.Trim());
                    }
                }

                registry.Sprites.RegisterSprite<string>(id, new()
                {
                    Texture = texture,
                    FrameWidth = element.ChildInt("FrameWidth"),
                    FrameHeight = element.ChildInt("FrameHeight"),
                    OriginX = element.ChildInt("OriginX", 0),
                    OriginY = element.ChildInt("OriginY", 0),
                    X = element.ChildInt("X", 0),
                    Y = element.ChildInt("Y", 0),
                    Animations = animns.ToArray(),
                    AdditionalData = additionalData
                });
            }
            else if (element.Name == "sprite_int")
            {
                var id = element.Attr("id");
                var texture = registry.Subtextures.RegisterTexture(
                    content.Root.GetRelativePath(element.ChildText("Texture"))
                );

                var animationsXml = element["Animations"] ??
                    throw new Exception($"[{content.Metadata.Name}] Missing Animations element.");

                List<Animation<int>> animns = new List<Animation<int>>();

                foreach (var animationXml in animationsXml)
                {
                    if (animationXml is XmlElement animElm)
                    {
                        animns.Add(
                            new()
                            {
                                ID = animElm.AttrInt("id"),
                                Loop = animElm.AttrBool("loop", false),
                                Delay = animElm.AttrFloat("delay", 0),
                                Frames = Calc.ReadCSVInt(animElm.Attr("frames"))
                            }
                        );
                    }
                }

                var additionalData = new Dictionary<string, object>();

                foreach (var data in element)
                {
                    if (data is XmlElement elm && !nonExtra.Contains(elm.Name))
                    {
                        additionalData.Add(elm.Name, elm.InnerText.Trim());
                    }
                }

                registry.Sprites.RegisterSprite<int>(id, new()
                {
                    Texture = texture,
                    FrameWidth = element.ChildInt("FrameWidth"),
                    FrameHeight = element.ChildInt("FrameHeight"),
                    OriginX = element.ChildInt("OriginX", 0),
                    OriginY = element.ChildInt("OriginY", 0),
                    X = element.ChildInt("X", 0),
                    Y = element.ChildInt("Y", 0),
                    Animations = animns.ToArray(),
                    AdditionalData = additionalData
                });
            }
        }
    }
}

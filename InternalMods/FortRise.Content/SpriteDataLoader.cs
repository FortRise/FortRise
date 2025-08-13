using System;
using System.Collections.Generic;
using System.Xml;
using Monocle;

namespace FortRise.Content;

internal static class SpriteDataLoader
{
    internal static void LoadSpriteData(IModRegistry registry, IModContent content, ContainerSpriteType spriteType)
    {
        string file = spriteType switch
        {
            ContainerSpriteType.Menu => "menuSpriteData",
            ContainerSpriteType.BG => "bgSpriteData",
            ContainerSpriteType.Boss => "bossSpriteData",
            ContainerSpriteType.Corpse => "corpseSpriteData",
            _ => "spriteData"
        };
        if (!content.Root.TryGetRelativePath($"Content/Atlas/SpriteData/{file}.xml", out IResourceInfo spriteDataRes))
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
                "Animations",
                "RedTeam",
                "BlueTeam",
                "HeadYOrigins",
                "RedTexture",
                "BlueTexture",
                "Flash"
            };

            var dest = spriteType switch
            {
                ContainerSpriteType.Menu => SubtextureAtlasDestination.MenuAtlas,
                ContainerSpriteType.BG => SubtextureAtlasDestination.BGAtlas,
                ContainerSpriteType.Boss => SubtextureAtlasDestination.BossAtlas,
                ContainerSpriteType.Corpse or _ => SubtextureAtlasDestination.Atlas
            };

            var id = element.Attr("id");

            var texture = content.LoadTexture(registry, element.ChildText("Texture").Trim(), dest);

            ISubtextureEntry? redTexture = element.HasChild("RedTexture") ? content.LoadTexture(registry, 
                element.ChildText("RedTexture"),
                dest
            ) : null;

            ISubtextureEntry? blueTexture = element.HasChild("BlueTexture") ? content.LoadTexture(registry, 
                element.ChildText("BlueTexture"),
                dest
            ) : null;

            ISubtextureEntry? redTeam = element.HasChild("RedTeam") ? content.LoadTexture(registry, 
                element.ChildText("RedTeam"),
                dest
            ) : null;

            ISubtextureEntry? blueTeam = element.HasChild("BlueTeam") ? content.LoadTexture(registry, 
                element.ChildText("BlueTeam"),
                dest
            ) : null;

            ISubtextureEntry? flash = element.HasChild("Flash") ? content.LoadTexture(registry, 
                element.ChildText("Flash"),
                dest
            ) : null;

            int[]? headXOrigins = null;
            if (element.HasChild("HeadXOrigins"))
            {
                headXOrigins = Calc.ReadCSVInt(element.ChildText("HeadXOrigins"));
            }

            int[]? headYOrigins = null;
            if (element.HasChild("HeadYOrigins"))
            {
                headYOrigins = Calc.ReadCSVInt(element.ChildText("HeadYOrigins"));
            }

            var animationsXml = element["Animations"] ??
                throw new Exception($"[{content.Metadata.Name}] Missing Animations element.");

            var additionalData = new Dictionary<string, object>();

            foreach (var data in element)
            {
                if (data is XmlElement elm && !nonExtra.Contains(elm.Name))
                {
                    additionalData.Add(elm.Name, elm.InnerText.Trim());
                }
            }

            if (element.Name == "sprite_string")
            {

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

                SpriteConfiguration<string> conf = new()
                {
                    Texture = texture,
                    FrameWidth = element.ChildInt("FrameWidth"),
                    FrameHeight = element.ChildInt("FrameHeight"),
                    OriginX = element.ChildInt("OriginX", 0),
                    OriginY = element.ChildInt("OriginY", 0),
                    X = element.ChildInt("X", 0),
                    Y = element.ChildInt("Y", 0),
                    Animations = animns.ToArray(),
                    AdditionalData = additionalData,
                    HeadXOrigins = headXOrigins,
                    HeadYOrigins = headYOrigins,
                    RedTexture = redTexture,
                    RedTeam = redTeam,
                    BlueTeam = blueTeam,
                    BlueTexture = blueTexture,
                    Flash = flash
                };

                switch (spriteType)
                {
                    case ContainerSpriteType.Main:
                        registry.Sprites.RegisterSprite(id, conf);
                        break;
                    case ContainerSpriteType.Menu:
                        registry.Sprites.RegisterMenuSprite(id, conf);
                        break;
                    case ContainerSpriteType.Corpse:
                        registry.Sprites.RegisterCorpseSprite(id, conf);
                        break;
                    case ContainerSpriteType.BG:
                        registry.Sprites.RegisterBGSprite(id, conf);
                        break;
                    case ContainerSpriteType.Boss:
                        registry.Sprites.RegisterBossSprite(id, conf);
                        break;
                }
            }
            else if (element.Name == "sprite_int")
            {
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

                SpriteConfiguration<int> conf = new()
                {
                    Texture = texture,
                    FrameWidth = element.ChildInt("FrameWidth"),
                    FrameHeight = element.ChildInt("FrameHeight"),
                    OriginX = element.ChildInt("OriginX", 0),
                    OriginY = element.ChildInt("OriginY", 0),
                    X = element.ChildInt("X", 0),
                    Y = element.ChildInt("Y", 0),
                    Animations = animns.ToArray(),
                    AdditionalData = additionalData,
                    HeadXOrigins = headXOrigins,
                    HeadYOrigins = headYOrigins,
                    RedTexture = redTexture,
                    RedTeam = redTeam,
                    BlueTeam = blueTeam,
                    BlueTexture = blueTexture,
                    Flash = flash
                };

                switch (spriteType)
                {
                    case ContainerSpriteType.Main:
                        registry.Sprites.RegisterSprite(id, conf);
                        break;
                    case ContainerSpriteType.Menu:
                        registry.Sprites.RegisterMenuSprite(id, conf);
                        break;
                    case ContainerSpriteType.Corpse:
                        registry.Sprites.RegisterCorpseSprite(id, conf);
                        break;
                    case ContainerSpriteType.BG:
                        registry.Sprites.RegisterBGSprite(id, conf);
                        break;
                    case ContainerSpriteType.Boss:
                        registry.Sprites.RegisterBossSprite(id, conf);
                        break;
                }
            }
        }
    }
}

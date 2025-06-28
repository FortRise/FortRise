using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise.Content;

internal static class ArcherLoader
{
    internal static void Load(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Content/Atlas/GameData/archerData.xml", out IResourceInfo archerRes))
        {
            return;
        }

        var archerDataXml = archerRes.Xml ??
            throw new Exception($"[{content.Metadata.Name}] Failed to load Xml file Atlas/GameData/archerData.xml.");

        var archers = archerDataXml["Archers"] ??
            throw new Exception($"[{content.Metadata.Name}] Missing Archers element.");

        foreach (var archer in archers)
        {
            if (archer is not XmlElement element)
            {
                continue;
            }

            bool isAlt = element.Name == "AltArcher";
            bool isSecret = element.Name == "SecretArcher";

            var id = element.Attr("id");

            if (isAlt)
            {
                var altID = element.Attr("Alt");
                var archerToCopy = registry.Archers.GetArcher(altID);
                if (archerToCopy is null)
                {
                    throw new Exception($"[{content.Metadata.Name}] Invalid Archer Alt ID: {altID} for {id}, or it does not exists.");
                }

                registry.Archers.RegisterArcher(
                    id,
                    CreateArcherConfigurationWithDefaults(id, element, registry, content, archerToCopy.Configuration) with
                    {
                        AltFor = archerToCopy
                    }
                );
                continue;
            }

            if (isSecret)
            {
                var secretID = element.Attr("Secret");
                var archerToCopy = registry.Archers.GetArcher(secretID);
                if (archerToCopy is null)
                {
                    throw new Exception($"[{content.Metadata.Name}] Invalid Archer Secret ID: {secretID} for {id}, or it does not exists.");
                }

                registry.Archers.RegisterArcher(
                    id,
                    CreateArcherConfigurationWithDefaults(id, element, registry, content, archerToCopy.Configuration) with
                    {
                        SecretFor = archerToCopy
                    });
                continue;
            }


            registry.Archers.RegisterArcher(id, CreateArcherConfiguration(id, element, registry, content));
        }
    }

    private static ArcherConfiguration CreateArcherConfigurationWithDefaults(string id, XmlElement element, IModRegistry registry, IModContent content, in ArcherConfiguration original)
    {
        string name0 = element.ChildText("Name0", original.TopName).Trim();
        string name1 = element.ChildText("Name1", original.BottomName).Trim();
        Color colorA = element.ChildHexColor("ColorA", original.ColorA);
        Color colorB = element.ChildHexColor("ColorB", original.ColorB);
        Color lightBarColor = element.ChildHexColor("LightbarColor", original.LightbarColor);

        ISubtextureEntry aimer;

        if (element.HasChild("Aimer"))
        {
            aimer = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath(element.ChildText("Aimer").Trim())
            );
        }
        else
        {
            aimer = original.Aimer;
        }

        IBaseSpriteContainerEntry? corpse;
        if (element.HasChild("Corpse"))
        {
            corpse = registry.Sprites.GetCorpseSpriteEntry<string>(element.ChildText("Corpse"));
        }
        else
        {
            corpse = original.CorpseSprite;
        }


        if (corpse is null)
        {
            corpse = registry.Sprites.GetSpriteEntry<string>(element.ChildText("Corpse"));
        }

        TFGame.Genders gender = element.ChildEnum<TFGame.Genders>("Genders", original.Gender);
        bool startNoHat = element.ChildBool("StartNoHat", original.StartNoHat);

        Option<HairInfo> hairInfo = Option<HairInfo>.None();

        if (element.HasChild("Hair"))
        {
            var hairElm = element["Hair"]!;
            Vector2 offset = new Vector2(0, 0);
            if (hairElm.HasChild("Offset"))
            {
                offset.X = hairElm["Offset"].AttrInt("x");
                offset.Y = hairElm["Offset"].AttrInt("y");
            }

            Vector2 duckingOffset = new Vector2(0, 0);
            if (hairElm.HasChild("DuckingOffset"))
            {
                offset.X = hairElm["DuckingOffset"].AttrInt("x");
                offset.Y = hairElm["DuckingOffset"].AttrInt("y");
            }

            hairInfo = new HairInfo()
            {
                Offset = offset,
                DuckingOffset = duckingOffset,
                Color = hairElm.ChildHexColor("Color", Color.White),
                OutlineColor = hairElm.ChildHexColor("OutlineColor", Color.Black),
            };
        }
        else
        {
            hairInfo = original.Hair;
        }

        int sfxID;

        string sfx = element.ChildText("SFX", null);
        if (!string.IsNullOrEmpty(sfx))
        {
            sfxID = registry.CharacterSounds.RegisterCharacterSounds(id + "_Char_Sounds", new()
            {
                Ready = LoadSFXVariedQuick(id, "READY", sfx, registry, content),
                AimCancel = LoadSFXQuick(id, "AIM_CANCEL", sfx, registry, content),
                AimDir = LoadSFXQuick(id, "AIM_DIR", sfx, registry, content),
                Aim = LoadSFXQuick(id, "AIM", sfx, registry, content),
                ArrowGrab = LoadSFXQuick(id, "ARROW_GRAB", sfx, registry, content),
                ArrowRecover = LoadSFXQuick(id, "ARROW_RECOVER", sfx, registry, content),
                ArrowSteal = LoadSFXQuick(id, "ARROW_STEAL", sfx, registry, content),
                Deselect = LoadSFXQuick(id, "DESELECT", sfx, registry, content),
                DieBomb = LoadSFXQuick(id, "DIE_BOMB", sfx, registry, content),
                DieLaser = LoadSFXQuick(id, "DIE_LASER", sfx, registry, content),
                DieStomp = LoadSFXQuick(id, "DIE_STOMP", sfx, registry, content),
                DieEnv = LoadSFXQuick(id, "DIE_ENV", sfx, registry, content),
                Die = LoadSFXQuick(id, "DIE", sfx, registry, content),
                Duck = LoadSFXQuick(id, "DUCK", sfx, registry, content),
                FireArrow = LoadSFXQuick(id, "FIRE_ARROW", sfx, registry, content),
                Grab = LoadSFXQuick(id, "GRAB", sfx, registry, content),
                Jump = LoadSFXQuick(id, "JUMP", sfx, registry, content),
                Land = LoadSFXQuick(id, "LAND", sfx, registry, content),
                NoFire = LoadSFXQuick(id, "NOFIRE", sfx, registry, content),
                Revive = LoadSFXQuick(id, "REVIVE", sfx, registry, content),
                WallSlide = LoadSFXLoopedQuick(id, "WALLSLIDE_LOOP", sfx, registry, content)!,
                Sleep = LoadSFXLoopedQuick(id, "SLEEP", sfx, registry, content)
            }).SFXID;
        }
        else
        {
            sfxID = original.SFX;
        }

        PortraitInfo portraitInfo;

        if (element.HasChild("Portraits"))
        {
            var portraits = element["Portraits"];

            var joined = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath(portraits.ChildText("Joined"))
            );

            var notJoined = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath(portraits.ChildText("NotJoined"))
            );

            var win = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath(portraits.ChildText("Win"))
            );

            var lose = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath(portraits.ChildText("Lose"))
            );
            portraitInfo = new()
            {
                Joined = joined,
                NotJoined = notJoined,
                Win = win,
                Lose = lose
            };
        }
        else
        {
            portraitInfo = original.Portraits;
        }

        StatueInfo statueInfo;

        if (element.HasChild("Statue"))
        {
            var statue = element["Statue"];
            var statueImage = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath(statue.ChildText("Image"))
            );

            var statueGlow = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath(statue.ChildText("Glow"))
            );

            statueInfo = new()
            {
                Image = statueImage,
                Glow = statueGlow
            };
        }
        else
        {
            statueInfo = original.Statue;
        }

        GemInfo gemInfo;

        if (element.HasChild("Gems"))
        {
            var gems = element["Gems"];

            var gemMenu = registry.Sprites.GetMenuSpriteEntry<string>(gems.ChildText("Menu"));
            var gemGameplay = registry.Sprites.GetSpriteEntry<int>(gems.ChildText("Gameplay"));

            gemInfo = new()
            {
                Menu = gemMenu!,
                Gameplay = gemGameplay!
            };
        }
        else
        {
            gemInfo = original.Gems;
        }

        SpriteInfo spriteInfo;

        if (element.HasChild("Sprites"))
        {
            var sprites = element["Sprites"];

            var bodyText = sprites.ChildText("Body");
            var body = registry.Sprites.GetSpriteEntry<string>(bodyText)
                ?? throw new Exception($"[{content.Metadata.Name}] {bodyText} on SpriteData cannot be found.");

            var headNormalText = sprites.ChildText("HeadNormal");
            var headNormal = registry.Sprites.GetSpriteEntry<string>(headNormalText)
                ?? throw new Exception($"[{content.Metadata.Name}] {headNormalText} on SpriteData cannot be found.");

            var headNoHatText = sprites.ChildText("HeadNoHat");
            var headNoHat = registry.Sprites.GetSpriteEntry<string>(headNoHatText)
                ?? throw new Exception($"[{content.Metadata.Name}] {headNoHatText} on SpriteData cannot be found.");

            var headCrownText = sprites.ChildText("HeadCrown");
            var headCrown = registry.Sprites.GetSpriteEntry<string>(headCrownText)
                ?? throw new Exception($"[{content.Metadata.Name}] {headCrownText} on SpriteData cannot be found.");

            var bowText = sprites.ChildText("Bow");
            var bow = registry.Sprites.GetSpriteEntry<string>(bowText)
                ?? throw new Exception($"[{content.Metadata.Name}] {bowText} on SpriteData cannot be found.");

            var headBackText = sprites.ChildText("HeadBack", "");
            var headBack = string.IsNullOrEmpty(headBackText) ? null : registry.Sprites.GetSpriteEntry<string>(headBackText);

            spriteInfo = new()
            {
                Body = body,
                HeadNoHat = headNoHat,
                HeadBack = headBack,
                HeadCrown = headCrown,
                HeadNormal = headNormal,
                Bow = bow
            };
        }
        else
        {
            spriteInfo = original.Sprites;
        }

        string victoryMusic;

        if (element.HasChild("VictoryMusic"))
        {
            var entry = registry.Musics.GetMusic(element.ChildText("VictoryMusic"));
            // another step if the user takes the whole path instead
            if (entry is null)
            {
                entry = registry.Musics.RegisterMusic(
                    element.ChildText("VictoryMusic"),
                    content.Root.GetRelativePath(element.ChildText("VictoryMusic")));
            }

            victoryMusic = entry.Name;
        }
        else
        {
            victoryMusic = original.VictoryMusic!;
        }

        return new()
        {
            TopName = name0,
            BottomName = name1,
            ColorA = colorA,
            ColorB = colorB,
            VictoryMusic = victoryMusic,
            LightbarColor = lightBarColor,
            Aimer = aimer,
            CorpseSprite = (ICorpseSpriteContainerEntry)corpse!,
            StartNoHat = startNoHat,
            Gender = gender,
            Hair = hairInfo,
            SFX = sfxID,
            Statue = statueInfo,
            Gems = gemInfo,
            Portraits = portraitInfo,
            Sprites = spriteInfo
        };
    }

    private static ArcherConfiguration CreateArcherConfiguration(string id, XmlElement element, IModRegistry registry, IModContent content)
    {
        string name0 = element.ChildText("Name0").Trim();
        string name1 = element.ChildText("Name1").Trim();
        Color colorA = element.ChildHexColor("ColorA", Color.White);
        Color colorB = element.ChildHexColor("ColorB", Color.White);
        Color lightBarColor = element.ChildHexColor("LightbarColor");
        ISubtextureEntry aimer = registry.Subtextures.RegisterTexture(
            content.Root.GetRelativePath(element.ChildText("Aimer").Trim())
        );

        IBaseSpriteContainerEntry? corpse = registry.Sprites.GetCorpseSpriteEntry<string>(element.ChildText("Corpse"));
        if (corpse is null)
        {
            corpse = registry.Sprites.GetSpriteEntry<string>(element.ChildText("Corpse"));
        }

        TFGame.Genders gender = element.ChildEnum<TFGame.Genders>("Genders", TFGame.Genders.Male);
        bool startNoHat = element.ChildBool("StartNoHat", false);

        Option<HairInfo> hairInfo = Option<HairInfo>.None();

        if (element.HasChild("Hair"))
        {
            var hairElm = element["Hair"]!;
            Vector2 offset = new Vector2(0, 0);
            if (hairElm.HasChild("Offset"))
            {
                offset.X = hairElm["Offset"].AttrInt("x");
                offset.Y = hairElm["Offset"].AttrInt("y");
            }

            Vector2 duckingOffset = new Vector2(0, 0);
            if (hairElm.HasChild("DuckingOffset"))
            {
                offset.X = hairElm["DuckingOffset"].AttrInt("x");
                offset.Y = hairElm["DuckingOffset"].AttrInt("y");
            }

            hairInfo = new HairInfo()
            {
                Offset = offset,
                DuckingOffset = duckingOffset,
                Color = hairElm.ChildHexColor("Color", Color.White),
                OutlineColor = hairElm.ChildHexColor("OutlineColor", Color.Black),
            };
        }
        int sfxID = 0;

        string sfx = element.ChildText("SFX", null);
        if (!string.IsNullOrEmpty(sfx))
        {
            sfxID = registry.CharacterSounds.RegisterCharacterSounds(id + "_Char_Sounds", new()
            {
                Ready = LoadSFXVariedQuick(id, "READY", sfx, registry, content),
                AimCancel = LoadSFXQuick(id, "AIM_CANCEL", sfx, registry, content),
                AimDir = LoadSFXQuick(id, "AIM_DIR", sfx, registry, content),
                Aim = LoadSFXQuick(id, "AIM", sfx, registry, content),
                ArrowGrab = LoadSFXQuick(id, "ARROW_GRAB", sfx, registry, content),
                ArrowRecover = LoadSFXQuick(id, "ARROW_RECOVER", sfx, registry, content),
                ArrowSteal = LoadSFXQuick(id, "ARROW_STEAL", sfx, registry, content),
                Deselect = LoadSFXQuick(id, "DESELECT", sfx, registry, content),
                DieBomb = LoadSFXQuick(id, "DIE_BOMB", sfx, registry, content),
                DieLaser = LoadSFXQuick(id, "DIE_LASER", sfx, registry, content),
                DieStomp = LoadSFXQuick(id, "DIE_STOMP", sfx, registry, content),
                DieEnv = LoadSFXQuick(id, "DIE_ENV", sfx, registry, content),
                Die = LoadSFXQuick(id, "DIE", sfx, registry, content),
                Duck = LoadSFXQuick(id, "DUCK", sfx, registry, content),
                FireArrow = LoadSFXQuick(id, "FIRE_ARROW", sfx, registry, content),
                Grab = LoadSFXQuick(id, "GRAB", sfx, registry, content),
                Jump = LoadSFXQuick(id, "JUMP", sfx, registry, content),
                Land = LoadSFXQuick(id, "LAND", sfx, registry, content),
                NoFire = LoadSFXQuick(id, "NOFIRE", sfx, registry, content),
                Revive = LoadSFXQuick(id, "REVIVE", sfx, registry, content),
                WallSlide = LoadSFXLoopedQuick(id, "WALLSLIDE_LOOP", sfx, registry, content)!,
                Sleep = LoadSFXLoopedQuick(id, "SLEEP", sfx, registry, content)
            }).SFXID;
        }

        var portraits = element["Portraits"] ??
            throw new Exception($"[{content.Metadata.Name}] Missing Portraits element.");

        var joined = registry.Subtextures.RegisterTexture(
            content.Root.GetRelativePath(portraits.ChildText("Joined"))
        );

        var notJoined = registry.Subtextures.RegisterTexture(
            content.Root.GetRelativePath(portraits.ChildText("NotJoined"))
        );

        var win = registry.Subtextures.RegisterTexture(
            content.Root.GetRelativePath(portraits.ChildText("Win"))
        );

        var lose = registry.Subtextures.RegisterTexture(
            content.Root.GetRelativePath(portraits.ChildText("Lose"))
        );

        var statue = element["Statue"] ??
            throw new Exception($"[{content.Metadata.Name}] Missing Statue element.");

        var statueImage = registry.Subtextures.RegisterTexture(
            content.Root.GetRelativePath(statue.ChildText("Image"))
        );

        var statueGlow = registry.Subtextures.RegisterTexture(
            content.Root.GetRelativePath(statue.ChildText("Glow"))
        );

        var gems = element["Gems"] ??
            throw new Exception($"[{content.Metadata.Name}] Missing Gems element.");

        var gemMenu = registry.Sprites.GetMenuSpriteEntry<string>(gems.ChildText("Menu"));
        var gemGameplay = registry.Sprites.GetSpriteEntry<int>(gems.ChildText("Gameplay"));

        var sprites = element["Sprites"] ??
            throw new Exception($"[{content.Metadata.Name}] Missing Sprites element.");

        var bodyText = sprites.ChildText("Body");
        var body = registry.Sprites.GetSpriteEntry<string>(bodyText)
            ?? throw new Exception($"[{content.Metadata.Name}] {bodyText} on SpriteData cannot be found.");

        var headNormalText = sprites.ChildText("HeadNormal");
        var headNormal = registry.Sprites.GetSpriteEntry<string>(headNormalText)
            ?? throw new Exception($"[{content.Metadata.Name}] {headNormalText} on SpriteData cannot be found.");

        var headNoHatText = sprites.ChildText("HeadNoHat");
        var headNoHat = registry.Sprites.GetSpriteEntry<string>(headNoHatText)
            ?? throw new Exception($"[{content.Metadata.Name}] {headNoHatText} on SpriteData cannot be found.");

        var headCrownText = sprites.ChildText("HeadCrown");
        var headCrown = registry.Sprites.GetSpriteEntry<string>(headCrownText)
            ?? throw new Exception($"[{content.Metadata.Name}] {headCrownText} on SpriteData cannot be found.");

        var bowText = sprites.ChildText("Bow");
        var bow = registry.Sprites.GetSpriteEntry<string>(bowText)
            ?? throw new Exception($"[{content.Metadata.Name}] {bowText} on SpriteData cannot be found.");

        var headBackText = sprites.ChildText("HeadBack", "");
        var headBack = string.IsNullOrEmpty(headBackText) ? null : registry.Sprites.GetSpriteEntry<string>(headBackText);

        string victoryMusic = "Team";

        if (element.HasChild("VictoryMusic"))
        {
            var entry = registry.Musics.GetMusic(element.ChildText("VictoryMusic"));
            // another step if the user takes the whole path instead
            if (entry is null)
            {
                entry = registry.Musics.RegisterMusic(
                    element.ChildText("VictoryMusic"),
                    content.Root.GetRelativePath(element.ChildText("VictoryMusic")));
            }

            victoryMusic = entry.Name;
        }


        return new()
        {
            TopName = name0,
            BottomName = name1,
            ColorA = colorA,
            ColorB = colorB,
            LightbarColor = lightBarColor,
            Aimer = aimer,
            CorpseSprite = (ICorpseSpriteContainerEntry)corpse!,
            StartNoHat = startNoHat,
            Gender = gender,
            Hair = hairInfo,
            SFX = sfxID,
            VictoryMusic = victoryMusic,
            Statue = new()
            {
                Image = statueImage,
                Glow = statueGlow
            },
            Gems = new()
            {
                Gameplay = gemGameplay!,
                Menu = gemMenu!
            },
            Portraits = new()
            {
                Win = win,
                Lose = lose,
                Joined = joined,
                NotJoined = notJoined
            },
            Sprites = new()
            {
                HeadBack = headBack,
                HeadCrown = headCrown!,
                HeadNoHat = headNoHat!,
                HeadNormal = headNormal!,
                Body = body!,
                Bow = bow!
            },
        };
    }

    private static ISFXLoopedEntry? LoadSFXLoopedQuick(string id, string type, string sfxPath, IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath(sfxPath.Replace("{action}", type), out var res))
        {
            return null;
        }
        return registry.SFXs.RegisterSFXLooped(id + "_" + type + "_SFX", res);
    }

    private static ISFXVariedEntry LoadSFXVariedQuick(string id, string type, string sfxPath, IModRegistry registry, IModContent content)
    {
        var act = sfxPath.Replace("{action}", type);

        var path = Path.ChangeExtension(act, null);
        int num = 0;

        List<IResourceInfo> validSFX = [];
        while (content.Root.TryGetRelativePath(path + VariedSuffix(num) + ".wav", out var info))
        {
            validSFX.Add(info);
            num += 1;
        }


        return registry.SFXs.RegisterSFXVaried(id + "_" + type + "_SFX", [.. validSFX]);
    }

    private static string VariedSuffix(int num)
    {
        num++;
        string text;
        if (num < 10)
        {
            text = "_0" + num;
        }
        else
        {
            text = "_" + num;
        }
        return text;
    }

    private static ISFXEntry LoadSFXQuick(string id, string type, string sfxPath, IModRegistry registry, IModContent content)
    {
        return registry.SFXs.RegisterSFX(id + "_" + type + "_SFX", content.Root.GetRelativePath(sfxPath.Replace("{action}", type)));
    }
}
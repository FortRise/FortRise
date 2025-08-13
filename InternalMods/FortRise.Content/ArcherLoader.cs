using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Extensions.Logging;
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

            registry.Archers.RegisterArcher(id, CreateArcherConfigurationWithDefaults(id, element, registry, content, default));
        }
    }

    private static ArcherConfiguration CreateArcherConfigurationWithDefaults(string id, XmlElement element, IModRegistry registry, IModContent content, in ArcherConfiguration original)
    {
        string name0 = element.ChildText("Name0", original.TopName).Trim();
        string name1 = element.ChildText("Name1", original.BottomName).Trim();

        if (name0 == null)
        {
            throw new Exception($"Archer: '{id}' is missing Name0 field.");
        }

        if (name1 == null)
        {
            throw new Exception($"Archer: '{id}' is missing Name1 field.");
        }

        Color colorA = element.ChildHexColor("ColorA", original.ColorA);
        Color colorB = element.ChildHexColor("ColorB", original.ColorB);
        Color lightBarColor = element.ChildHexColor("LightbarColor", original.LightbarColor);

        ISubtextureEntry aimer;

        if (element.HasChild("Aimer"))
        {
            aimer = content.LoadTexture(registry, element.ChildText("Aimer").Trim(), SubtextureAtlasDestination.Atlas);
        }
        else
        {
            aimer = original.Aimer;
            if (aimer == null)
            {
                ContentModule.Instance.Logger.LogWarning($"Archer: '{id}' is missing Aimer field. Falling back to Green's Aimer");
                aimer = registry.Subtextures.RegisterTexture(() => TFGame.Atlas["aimers/green"]);
            }
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

            if (corpse == null)
            {
                throw new Exception($"Archer: '{id}' is missing Aimer field. Falling back to Green's Corpse");
            }
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

        int sfxIDFallback = element.ChildInt("SFXFallback", 0);
        string sfx = element.ChildText("SFX", null);
        if (!string.IsNullOrEmpty(sfx))
        {
            if (int.TryParse(sfx, out int s))
            {
                sfxID = s;
            }
            else 
            {
                sfxID = registry.CharacterSounds.RegisterCharacterSounds(id + "_Char_Sounds", new()
                {
                    Ready = LoadSFXVariedQuick(id, "READY", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].Ready),
                    AimCancel = LoadSFXQuick(id, "AIM_CANCEL", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].AimCancel),
                    AimDir = LoadSFXQuick(id, "AIM_DIR", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].AimDir),
                    Aim = LoadSFXQuick(id, "AIM", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].Aim),
                    ArrowGrab = LoadSFXQuick(id, "ARROW_GRAB", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].ArrowGrab),
                    ArrowRecover = LoadSFXQuick(id, "ARROW_RECOVER", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].ArrowRecover),
                    ArrowSteal = LoadSFXQuick(id, "ARROW_STEAL", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].ArrowSteal),
                    Deselect = LoadSFXQuick(id, "DESELECT", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].Deselect),
                    DieBomb = LoadSFXQuick(id, "DIE_BOMB", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].DieBomb),
                    DieLaser = LoadSFXQuick(id, "DIE_LASER", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].DieLaser),
                    DieStomp = LoadSFXQuick(id, "DIE_STOMP", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].DieStomp),
                    DieEnv = LoadSFXQuick(id, "DIE_ENV", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].DieEnv),
                    Die = LoadSFXQuick(id, "DIE", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].Die),
                    Duck = LoadSFXQuick(id, "DUCK", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].Duck),
                    FireArrow = LoadSFXQuick(id, "FIRE_ARROW", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].FireArrow),
                    Grab = LoadSFXQuick(id, "GRAB", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].Grab),
                    Jump = LoadSFXQuick(id, "JUMP", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].Jump),
                    Land = LoadSFXQuick(id, "LAND", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].Land),
                    NoFire = LoadSFXQuick(id, "NOFIRE", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].NoFire),
                    Revive = LoadSFXQuick(id, "REVIVE", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].Revive),
                    WallSlide = LoadSFXLoopedQuick(id, "WALLSLIDE_LOOP", sfx, registry, content, () => Sounds.Characters[sfxIDFallback].WallSlide)!,
                    Sleep = LoadSFXLoopedQuick(id, "SLEEP", sfx, registry, content, null)
                }).SFXID;
            }
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
            string music = element.ChildText("VictoryMusic");
            var entry = registry.Musics.GetMusic(music);
            // another step if the user takes the whole path instead
            if (entry is null)
            {
                if (content.Root.ExistsRelativePath(music))
                {
                    entry = registry.Musics.RegisterMusic(
                        element.ChildText("VictoryMusic"),
                        content.Root.GetRelativePath(music));

                    victoryMusic = entry.Name;
                }
                else 
                {
                    victoryMusic = music;
                }
            }
            else 
            {
                victoryMusic = entry.Name;
            }
        }
        else
        {
            victoryMusic = original.VictoryMusic ?? "Team";
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

    private static ISFXLoopedEntry? LoadSFXLoopedQuick(string id, string type, string sfxPath, IModRegistry registry, IModContent content, Func<SFXLooped>? fallback)
    {
        if (!content.Root.TryGetRelativePath(sfxPath.Replace("{action}", type), out var res))
        {
            if (fallback is { } f)
            {
                return registry.SFXs.RegisterSFXLooped(id + "_" + type + "_SFX", f);
            }
            return null;
        }
        return registry.SFXs.RegisterSFXLooped(id + "_" + type + "_SFX", res);
    }

    private static ISFXVariedEntry LoadSFXVariedQuick(string id, string type, string sfxPath, IModRegistry registry, IModContent content, Func<SFXVaried> fallback)
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

        if (num == 0)
        {
            return registry.SFXs.RegisterSFXVaried(id + "_" + type + "_SFX", fallback);
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

    private static ISFXEntry LoadSFXQuick(string id, string type, string sfxPath, IModRegistry registry, IModContent content, Func<SFX> sfxFallback)
    {
        if (content.Root.TryGetRelativePath((sfxPath.Replace("{action}", type)), out var res))
        {
            return registry.SFXs.RegisterSFX(id + "_" + type + "_SFX", res);
        }

        return registry.SFXs.RegisterSFX(id + "_" + type + "_SFX", sfxFallback);
    }
}

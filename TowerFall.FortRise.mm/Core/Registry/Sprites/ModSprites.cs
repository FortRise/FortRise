#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public interface IBaseSpriteContainerEntry
{
    public ISpriteEntry Entry { get; init; }
    public ContainerSpriteType Type { get; init; }
}

public interface ISpriteContainerEntry : IBaseSpriteContainerEntry;
public interface IMenuSpriteContainerEntry : IBaseSpriteContainerEntry;
public interface ICorpseSpriteContainerEntry : IBaseSpriteContainerEntry;
public interface IBGSpriteContainerEntry : IBaseSpriteContainerEntry;
public interface IBossSpriteContainerEntry : IBaseSpriteContainerEntry;

public enum ContainerSpriteType { Main, Menu, Corpse, BG, Boss }
internal sealed class SpriteContainerEntry :
    ISpriteContainerEntry,
    IMenuSpriteContainerEntry,
    ICorpseSpriteContainerEntry,
    IBGSpriteContainerEntry,
    IBossSpriteContainerEntry
{
    public ISpriteEntry Entry { get; init; }
    public ContainerSpriteType Type { get; init; }

    public SpriteContainerEntry(ISpriteEntry entry, ContainerSpriteType type)
    {
        Entry = entry;
        Type = type;
    }
}

public interface ISpriteEntry
{
    /// <summary>
    /// A sprite identity for SpriteData.
    /// </summary>
    public string ID { get; init; }
    public XmlElement Xml { get; }
}

public interface ISpriteEntry<T> : ISpriteEntry
{
    public Sprite<T>? Sprite { get; }
    public SpriteConfiguration<T> Configuration { get; init; }
}

internal class SpriteEntry<T> : ISpriteEntry<T>
{
    public string ID { get; init; }
    public SpriteConfiguration<T> Configuration { get; init; }
    public Sprite<T>? Sprite => GetActualSprite();

    public XmlElement Xml => xmlCallback();
    private XmlElement? cache;
    private Func<XmlElement> xmlCallback;

    public SpriteEntry(string id, SpriteConfiguration<T> configuration)
    {
        ID = id;
        Configuration = configuration;
        xmlCallback = GetActualXml;
    }

    public SpriteEntry(string id, Func<XmlElement> callback)
    {
        ID = id;
        xmlCallback = callback;
    }

    private XmlElement GetActualXml()
    {
        if (cache != null)
        {
            return cache;
        }
        var document = new XmlDocument();
        var type = typeof(T);
        string spriteType;


        if (type == typeof(int))
        {
            spriteType = "sprite_int";
        }
        else if (type == typeof(string))
        {
            spriteType = "sprite_string";
        }
        else
        {
            throw new Exception($"[SpriteEntry] Unsupported type: {typeof(T).Name}");
        }
        var sprite = document.CreateElement(spriteType);
        sprite.SetAttribute("id", ID);
        CreateProperty(document, sprite, "Texture", Configuration.Texture.ID);
        CreateProperty(document, sprite, "FrameWidth", Configuration.FrameWidth);
        CreateProperty(document, sprite, "FrameHeight", Configuration.FrameHeight);
        CreateProperty(document, sprite, "OriginX", Configuration.OriginX);
        CreateProperty(document, sprite, "OriginY", Configuration.OriginY);
        CreateProperty(document, sprite, "X", Configuration.X);
        CreateProperty(document, sprite, "Y", Configuration.Y);

        var animations = document.CreateElement("Animations");
        for (int i = 0; i < Configuration.Animations.Length; i++)
        {
            var anim = Configuration.Animations[i];
            var elm = document.CreateElement("Anim");
            elm.SetAttribute("id", anim.ID!.ToString());

            string frames = string.Join(',', anim.Frames);
            elm.SetAttribute("frames", frames);
            elm.SetAttribute("delay", anim.Delay.ToString());
            elm.SetAttribute("loop", anim.Loop ? "True" : "False");
            animations.AppendChild(elm);
        }

        if (Configuration.AdditionalData != null)
        {
            foreach (var additional in Configuration.AdditionalData)
            {
                CreateProperty(document, sprite, additional.Key, additional.Value);
            }
        }

        sprite.AppendChild(animations);

        if (!Directory.Exists("DUMP"))
        {
            Directory.CreateDirectory("DUMP");
        }

        return sprite;
    }

    private XmlElement CreateProperty<T1>(XmlDocument document, XmlElement parent, string propertyName, T1 value)
    where T1: notnull
    {
        XmlElement element = document.CreateElement(propertyName);
        element.InnerText = value.ToString()!;
        parent.AppendChild(element);
        return element;
    }

    private Sprite<T> GetActualSprite()
    {
        if (Configuration.Texture == null)
        {
            throw new Exception($"[SpriteEntry] The Sprite that has been used might have been a Vanilla Sprite. Use 'TFGame.SpriteData' or any of its variant instead.");
        }

        if (typeof(T) == typeof(string))
        {
            XmlElement xml = Xml;

            Sprite<string> sprite = new Sprite<string>(Configuration.Texture.Subtexture, xml.ChildInt("FrameWidth"), xml.ChildInt("FrameHeight"), 0);
            sprite.Origin = new Vector2(xml.ChildFloat("OriginX", 0f), xml.ChildFloat("OriginY", 0f));
            sprite.Position = new Vector2(xml.ChildFloat("X", 0f), xml.ChildFloat("Y", 0f));
            sprite.Color = xml.ChildHexColor("Color", Color.White);

            XmlElement animationXml = xml["Animations"]!;
            if (animationXml != null)
            {
                foreach (XmlElement animXml in animationXml.GetElementsByTagName("Anim"))
                {
                    sprite.Add(
                        animXml.Attr("id"),
                        animXml.AttrFloat("delay", 0f),
                        animXml.AttrBool("loop", true),
                        Calc.ReadCSVInt(animXml.Attr("frames"))
                    );
                }
            }
            return (Sprite<T>)(object)sprite;
        }
        else if (typeof(T) == typeof(int))
        {
            XmlElement xml = Xml;

            Sprite<int> sprite = new Sprite<int>(Configuration.Texture.Subtexture, xml.ChildInt("FrameWidth"), xml.ChildInt("FrameHeight"), 0);
            sprite.Origin = new Vector2(xml.ChildFloat("OriginX", 0f), xml.ChildFloat("OriginY", 0f));
            sprite.Position = new Vector2(xml.ChildFloat("X", 0f), xml.ChildFloat("Y", 0f));
            sprite.Color = xml.ChildHexColor("Color", Color.White);

            XmlElement animationXml = xml["Animations"]!;
            if (animationXml != null)
            {
                foreach (XmlElement animXml in animationXml.GetElementsByTagName("Anim"))
                {
                    sprite.Add(
                        animXml.AttrInt("id"),
                        animXml.AttrFloat("delay", 0f),
                        animXml.AttrBool("loop", true),
                        Calc.ReadCSVInt(animXml.Attr("frames"))
                    );
                }
            }
            return (Sprite<T>)(object)sprite;
        }
        else
        {
            throw new Exception($"[SpriteEntry] Unsupported type: {typeof(T).Name}");
        }
    }
}

public readonly struct SpriteConfiguration<T>
{
    public required ISubtextureEntry Texture { get; init; }
    public required int FrameWidth { get; init; }
    public required int FrameHeight { get; init; }
    public int OriginX { get; init; }
    public int OriginY { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public Dictionary<string, object>? AdditionalData { get; init; }
    public required Animation<T>[] Animations { get; init; }
}

public readonly struct Animation<T>
{
    public required T ID { get; init; }
    public required int[] Frames { get; init; }
    public float Delay { get; init; }
    public bool Loop { get; init; }
}

public class ModSprites
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<IBaseSpriteContainerEntry> spriteQueue;
    private readonly Dictionary<string, IBaseSpriteContainerEntry> spriteEntries = new();

    internal ModSprites(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        spriteQueue = manager.CreateQueue<IBaseSpriteContainerEntry>(Invoke);
    }

    public ISpriteContainerEntry RegisterSprite<T>(string id, SpriteConfiguration<T> configuration)
    {
        var name = $"{metadata.Name}/{id}";
        ISpriteEntry entry = new SpriteEntry<T>(name, configuration);
        var actualEntry = new SpriteContainerEntry(entry, ContainerSpriteType.Main);
        spriteEntries.Add(name, actualEntry);
        spriteQueue.AddOrInvoke(actualEntry);
        return actualEntry;
    }

    public IMenuSpriteContainerEntry RegisterMenuSprite<T>(string id, SpriteConfiguration<T> configuration)
    {
        var name = $"{metadata.Name}/{id}";
        ISpriteEntry entry = new SpriteEntry<T>(name, configuration);
        var actualEntry = new SpriteContainerEntry(entry, ContainerSpriteType.Menu);
        spriteEntries.Add(name, actualEntry);
        spriteQueue.AddOrInvoke(actualEntry);
        return actualEntry;
    }

    public IBGSpriteContainerEntry RegisterBGSprite<T>(string id, SpriteConfiguration<T> configuration)
    {
        var name = $"{metadata.Name}/{id}";
        ISpriteEntry entry = new SpriteEntry<T>(name, configuration);
        var actualEntry = new SpriteContainerEntry(entry, ContainerSpriteType.BG);
        spriteEntries.Add(name, actualEntry);
        spriteQueue.AddOrInvoke(actualEntry);
        return actualEntry;
    }

    public ICorpseSpriteContainerEntry RegisterCorpseSprite<T>(string id, SpriteConfiguration<T> configuration)
    {
        var name = $"{metadata.Name}/{id}";
        ISpriteEntry entry = new SpriteEntry<T>(name, configuration);
        var actualEntry = new SpriteContainerEntry(entry, ContainerSpriteType.Corpse);
        spriteEntries.Add(name, actualEntry);
        spriteQueue.AddOrInvoke(actualEntry);
        return actualEntry;
    }

    public IBossSpriteContainerEntry RegisterBossSprite<T>(string id, SpriteConfiguration<T> configuration)
    {
        var name = $"{metadata.Name}/{id}";
        ISpriteEntry entry = new SpriteEntry<T>(name, configuration);
        var actualEntry = new SpriteContainerEntry(entry, ContainerSpriteType.Boss);
        spriteEntries.Add(name, actualEntry);
        spriteQueue.AddOrInvoke(actualEntry);
        return actualEntry;
    }

    public ISpriteContainerEntry? GetSpriteEntry<T>(string id)
    {
        string name = $"{metadata.Name}/{id}";
        if (spriteEntries.TryGetValue(name, out IBaseSpriteContainerEntry? value))
        {
            return (ISpriteContainerEntry?)value;
        }

        return (ISpriteContainerEntry?)CreateVanillaEntry<T>(name, ContainerSpriteType.Main);
    }

    public IMenuSpriteContainerEntry? GetMenuSpriteEntry<T>(string id)
    {
        string name = $"{metadata.Name}/{id}";
        if (spriteEntries.TryGetValue(name, out IBaseSpriteContainerEntry? value))
        {
            return (IMenuSpriteContainerEntry?)value;
        }

        return (IMenuSpriteContainerEntry?)CreateVanillaEntry<T>(name, ContainerSpriteType.Menu);
    }

    public ICorpseSpriteContainerEntry? GetCorpseSpriteEntry<T>(string id)
    {
        string name = $"{metadata.Name}/{id}";
        if (spriteEntries.TryGetValue(name, out IBaseSpriteContainerEntry? value))
        {
            return (ICorpseSpriteContainerEntry?)value;
        }

        return (ICorpseSpriteContainerEntry?)CreateVanillaEntry<T>(name, ContainerSpriteType.Corpse);
    }

    public IBGSpriteContainerEntry? GetBGSpriteEntry<T>(string id)
    {
        string name = $"{metadata.Name}/{id}";
        if (spriteEntries.TryGetValue(name, out IBaseSpriteContainerEntry? value))
        {
            return (IBGSpriteContainerEntry?)value;
        }

        return (IBGSpriteContainerEntry?)CreateVanillaEntry<T>(name, ContainerSpriteType.BG);
    }

    public IBossSpriteContainerEntry? GetBossSpriteEntry<T>(string id)
    {
        string name = $"{metadata.Name}/{id}";
        if (spriteEntries.TryGetValue(name, out IBaseSpriteContainerEntry? value))
        {
            return (IBossSpriteContainerEntry?)value;
        }

        return (IBossSpriteContainerEntry?)CreateVanillaEntry<T>(name, ContainerSpriteType.Boss);
    }


    private static Dictionary<string, IBaseSpriteContainerEntry> vanillaCaches = new Dictionary<string, IBaseSpriteContainerEntry>();
    private static IBaseSpriteContainerEntry CreateVanillaEntry<T>(string id, ContainerSpriteType spriteType)
    {
        ref var vanilla = ref CollectionsMarshal.GetValueRefOrAddDefault(vanillaCaches, id, out bool exists);
        if (exists)
        {
            return vanilla!;
        }

        IBaseSpriteContainerEntry entry;

        switch (spriteType)
        {
            case ContainerSpriteType.Boss:
                entry = new SpriteContainerEntry(new SpriteEntry<T>(id, () => TFGame.BossSpriteData.GetXML(id)), spriteType);
                break;
            case ContainerSpriteType.BG:
                entry = new SpriteContainerEntry(new SpriteEntry<T>(id, () => TFGame.BGSpriteData.GetXML(id)), spriteType);
                break;
            case ContainerSpriteType.Menu:
                entry = new SpriteContainerEntry(new SpriteEntry<T>(id, () => TFGame.MenuSpriteData.GetXML(id)), spriteType);
                break;
            case ContainerSpriteType.Corpse:
                entry = new SpriteContainerEntry(new SpriteEntry<T>(id, () => TFGame.CorpseSpriteData.GetXML(id)), spriteType);
                break;
            default:
                entry = new SpriteContainerEntry(new SpriteEntry<T>(id, () => TFGame.SpriteData.GetXML(id)), spriteType);
                break;
        }

        vanillaCaches[id] = entry;
        return entry;
    }

    private void Invoke(IBaseSpriteContainerEntry entry)
    {
        Dictionary<string, XmlElement> xmlElements = entry.Type switch
        {
            ContainerSpriteType.Menu => TFGame.MenuSpriteData.GetSprites(),
            ContainerSpriteType.Boss => TFGame.BossSpriteData.GetSprites(),
            ContainerSpriteType.BG => TFGame.BGSpriteData.GetSprites(),
            ContainerSpriteType.Corpse => TFGame.CorpseSpriteData.GetSprites(),
            _ => TFGame.SpriteData.GetSprites()
        };

        xmlElements[entry.Entry.ID] = entry.Entry.Xml;
    }
}
#nullable enable
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise;


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

    /// <summary>
    /// Field only for archers
    /// </summary>
    public int[]? HeadYOrigins { get; init; }
    /// <summary>
    /// Field only for archers
    /// </summary>
    public ISubtextureEntry? RedTexture { get; init; }
    /// <summary>
    /// Field only for archers
    /// </summary>
    public ISubtextureEntry? BlueTexture { get; init; }
    /// <summary>
    /// Field only for archers
    /// </summary>
    public ISubtextureEntry? RedTeam { get; init; }
    /// <summary>
    /// Field only for archers
    /// </summary>
    public ISubtextureEntry? BlueTeam { get; init; }
    /// <summary>
    /// Field only for archers
    /// </summary>
    public ISubtextureEntry? Flash { get; init; }
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
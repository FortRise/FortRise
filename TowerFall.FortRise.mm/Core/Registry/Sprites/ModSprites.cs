#nullable enable
using System.Collections.Generic;
using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise;

public interface IModSprites
{
    ISpriteContainerEntry RegisterSprite<T>(string id, SpriteConfiguration<T> configuration);
    IMenuSpriteContainerEntry RegisterMenuSprite<T>(string id, SpriteConfiguration<T> configuration);
    IBGSpriteContainerEntry RegisterBGSprite<T>(string id, SpriteConfiguration<T> configuration);
    ICorpseSpriteContainerEntry RegisterCorpseSprite<T>(string id, SpriteConfiguration<T> configuration);
    IBossSpriteContainerEntry RegisterBossSprite<T>(string id, SpriteConfiguration<T> configuration);

    ISpriteContainerEntry? GetSpriteEntry<T>(string id);
    IMenuSpriteContainerEntry? GetMenuSpriteEntry<T>(string id);
    ICorpseSpriteContainerEntry? GetCorpseSpriteEntry<T>(string id);
    IBGSpriteContainerEntry? GetBGSpriteEntry<T>(string id);
    IBossSpriteContainerEntry? GetBossSpriteEntry<T>(string id);
}

internal sealed class ModSprites : IModSprites
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<IBaseSpriteContainerEntry> spriteQueue;

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
        SpriteRegistry.AddSprite(actualEntry);
        spriteQueue.AddOrInvoke(actualEntry);
        return actualEntry;
    }

    public IMenuSpriteContainerEntry RegisterMenuSprite<T>(string id, SpriteConfiguration<T> configuration)
    {
        var name = $"{metadata.Name}/{id}";
        ISpriteEntry entry = new SpriteEntry<T>(name, configuration);
        var actualEntry = new SpriteContainerEntry(entry, ContainerSpriteType.Menu);
        SpriteRegistry.AddSprite(actualEntry);
        spriteQueue.AddOrInvoke(actualEntry);
        return actualEntry;
    }

    public IBGSpriteContainerEntry RegisterBGSprite<T>(string id, SpriteConfiguration<T> configuration)
    {
        var name = $"{metadata.Name}/{id}";
        ISpriteEntry entry = new SpriteEntry<T>(name, configuration);
        var actualEntry = new SpriteContainerEntry(entry, ContainerSpriteType.BG);
        SpriteRegistry.AddSprite(actualEntry);
        spriteQueue.AddOrInvoke(actualEntry);
        return actualEntry;
    }

    public ICorpseSpriteContainerEntry RegisterCorpseSprite<T>(string id, SpriteConfiguration<T> configuration)
    {
        var name = $"{metadata.Name}/{id}";
        ISpriteEntry entry = new SpriteEntry<T>(name, configuration);
        var actualEntry = new SpriteContainerEntry(entry, ContainerSpriteType.Corpse);
        SpriteRegistry.AddSprite(actualEntry);
        spriteQueue.AddOrInvoke(actualEntry);
        return actualEntry;
    }

    public IBossSpriteContainerEntry RegisterBossSprite<T>(string id, SpriteConfiguration<T> configuration)
    {
        var name = $"{metadata.Name}/{id}";
        ISpriteEntry entry = new SpriteEntry<T>(name, configuration);
        var actualEntry = new SpriteContainerEntry(entry, ContainerSpriteType.Boss);
        SpriteRegistry.AddSprite(actualEntry);
        spriteQueue.AddOrInvoke(actualEntry);
        return actualEntry;
    }

    public ISpriteContainerEntry? GetSpriteEntry<T>(string id)
    {
        return SpriteRegistry.GetSpriteEntry<T>(id);
    }

    public IMenuSpriteContainerEntry? GetMenuSpriteEntry<T>(string id)
    {
        return SpriteRegistry.GetMenuSpriteEntry<T>(id);
    }

    public ICorpseSpriteContainerEntry? GetCorpseSpriteEntry<T>(string id)
    {
        return SpriteRegistry.GetCorpseSpriteEntry<T>(id);
    }

    public IBGSpriteContainerEntry? GetBGSpriteEntry<T>(string id)
    {
        return SpriteRegistry.GetBGSpriteEntry<T>(id);
    }

    public IBossSpriteContainerEntry? GetBossSpriteEntry<T>(string id)
    {
        return SpriteRegistry.GetBossSpriteEntry<T>(id);
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

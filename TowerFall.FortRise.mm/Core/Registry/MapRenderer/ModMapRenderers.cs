#nullable enable
using System;

namespace FortRise;

public interface IModMapRenderers
{
    IMapRendererEntry RegisterMapRenderer(string id, MapRendererConfiguration configuration);
    IMapRendererEntry? GetMapRenderer(string name);
    [Obsolete("Use IModMapRenderers.GetMapRendererFromTowerSet instead")]
    IMapRendererEntry? GetMapRendererFromLevelSet(string levelSet) => GetMapRendererFromTowerSet(levelSet);
    IMapRendererEntry? GetMapRendererFromTowerSet(string towerSet);
}

internal sealed class ModMapRenderers : IModMapRenderers
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<IMapRendererEntry> registryQueue;

    internal ModMapRenderers(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IMapRendererEntry>(metadata, static _ => {}, Detach);
    }

    private void Detach(IMapRendererEntry entry)
    {
        MapRendererRegistry.RemoveEntry(entry);
    }

    public IMapRendererEntry RegisterMapRenderer(string id, MapRendererConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        IMapRendererEntry entry = new MapRendererEntry(name, configuration);

        MapRendererRegistry.AddEntry(entry);
        registryQueue.AddOrInvoke(entry);
        return entry;
    }

    public IMapRendererEntry? GetMapRenderer(string name)
    {
        return MapRendererRegistry.GetEntry(name);
    }

    public IMapRendererEntry? GetMapRendererFromTowerSet(string towerSet)
    {
        return MapRendererRegistry.GetEntryFromTowerSet(towerSet);
    }
}

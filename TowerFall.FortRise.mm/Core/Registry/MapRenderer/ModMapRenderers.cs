#nullable enable
using System.Collections.Generic;

namespace FortRise;

public interface IModMapRenderers
{
    IMapRendererEntry RegisterMapRenderer(string id, MapRendererConfiguration configuration);
    IMapRendererEntry? GetMapRenderer(string name);
    IMapRendererEntry? GetMapRendererFromLevelSet(string levelSet);
}

internal sealed class ModMapRenderers : IModMapRenderers
{
    private readonly RegistryQueue<IMapRendererEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModMapRenderers(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IMapRendererEntry>(Invoke);
    }

    public IMapRendererEntry RegisterMapRenderer(string id, MapRendererConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        IMapRendererEntry entry = new MapRendererEntry(name, configuration);

        registryQueue.AddOrInvoke(entry);
        MapRendererRegistry.AddEntry(entry);
        return entry;
    }

    public IMapRendererEntry? GetMapRenderer(string name)
    {
        return MapRendererRegistry.GetEntry(name);
    }

    public IMapRendererEntry? GetMapRendererFromLevelSet(string name)
    {
        return MapRendererRegistry.GetEntry(name);
    }

    internal void Invoke(IMapRendererEntry entry)
    {

    }
}

internal static class MapRendererRegistry 
{
    private static readonly Dictionary<string, IMapRendererEntry> mapEntries = [];
    private static readonly Dictionary<string, IMapRendererEntry> levelSetToMapEntries = [];

    public static void AddEntry(IMapRendererEntry entry)
    {
        mapEntries[entry.Name] = entry;
        levelSetToMapEntries[entry.Configuration.LevelSet] = entry;
    }

    public static IMapRendererEntry? GetEntry(string name)
    {
        mapEntries.TryGetValue(name, out var map);
        return map;
    }

    public static IMapRendererEntry? GetEntryFromLevelSet(string levelSet)
    {
        levelSetToMapEntries.TryGetValue(levelSet, out var map);
        return map;
    }
}

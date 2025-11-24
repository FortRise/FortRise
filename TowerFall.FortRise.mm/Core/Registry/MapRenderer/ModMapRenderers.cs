#nullable enable
namespace FortRise;

public interface IModMapRenderers
{
    IMapRendererEntry RegisterMapRenderer(string id, MapRendererConfiguration configuration);
    IMapRendererEntry? GetMapRenderer(string name);
    IMapRendererEntry? GetMapRendererFromLevelSet(string levelSet);
}

internal sealed class ModMapRenderers : IModMapRenderers
{
    private readonly ModuleMetadata metadata;

    internal ModMapRenderers(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
    }

    public IMapRendererEntry RegisterMapRenderer(string id, MapRendererConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        IMapRendererEntry entry = new MapRendererEntry(name, configuration);

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
}

#nullable enable
using TowerFall;

namespace FortRise;

public interface IModArrows
{
    IArrowEntry? GetArrow(string id);
    IArrowEntry RegisterArrows(string id, in ArrowConfiguration configuration);
}

internal sealed class ModArrows : IModArrows
{
    private readonly RegistryQueue<IArrowEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModArrows(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IArrowEntry>(Invoke);
    }

    public IArrowEntry RegisterArrows(string id, in ArrowConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";

        IArrowEntry arrow = new ArrowEntry(name, EnumPool.Obtain<ArrowTypes>(), configuration);
        ArrowsRegistry.AddArrow(arrow);
        registryQueue.AddOrInvoke(arrow);
        return arrow;
    }

    public IArrowEntry? GetArrow(string id)
    {
        return ArrowsRegistry.GetArrow(id);
    }

    internal void Invoke(IArrowEntry entry)
    {
        ArrowsRegistry.Register(entry.Name, entry.ArrowTypes, entry.Configuration);
    }
}

#nullable enable
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public interface IModArrows
{
    IReadOnlyDictionary<string, IArrowEntry> RegisteredArrows { get; }

    IArrowEntry? GetArrow(string id);
    IArrowEntry RegisterArrows(string id, in ArrowConfiguration configuration);
}

internal sealed class ModArrows : IModArrows
{
    private readonly RegistryQueue<IArrowEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    public IReadOnlyDictionary<string, IArrowEntry> RegisteredArrows => ArrowsRegistry.GetArrowEntries();

    internal ModArrows(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IArrowEntry>(metadata, Invoke, Detach);
    }

    public IArrowEntry RegisterArrows(string id, in ArrowConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";

        IArrowEntry arrow = new ArrowEntry(name, IndexReservationPool.ClaimOrObtain<ArrowTypes>(metadata, name), configuration);
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

    internal void Detach(IArrowEntry entry)
    {
        ArrowsRegistry.Unregister(entry);
        IndexReservationPool.FreeOrReserve(metadata, entry.Name, entry.ArrowTypes);
    }
}

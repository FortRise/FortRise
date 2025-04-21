#nullable enable
using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public class ModArrows 
{
    private readonly Dictionary<string, IArrowEntry> entries = new Dictionary<string, IArrowEntry>();
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
        entries.Add(name, arrow);
        registryQueue.AddOrInvoke(arrow);
        return arrow;
    }

    public IArrowEntry? GetArrow(string id) 
    {
        ReadOnlySpan<char> name = $"{metadata.Name}/{id}";
        var alternate = entries.GetAlternateLookup<ReadOnlySpan<char>>();
        alternate.TryGetValue(name, out IArrowEntry? value);
        return value;
    }

    internal void Invoke(IArrowEntry entry)
    {
        ArrowsRegistry.Register(entry.Name, entry.ArrowTypes, entry.Configuration);
    }
}

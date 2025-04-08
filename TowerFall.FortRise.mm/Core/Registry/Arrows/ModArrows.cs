#nullable enable
using System;
using System.Collections.Generic;

namespace FortRise;

public class ModArrows 
{
    private readonly Dictionary<string, IArrow> entries = new Dictionary<string, IArrow>();
    private readonly RegistryQueue<IArrow> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModArrows(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IArrow>(Invoke);
    }

    public IArrow RegisterArrows(string id, ArrowConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";

        IArrow arrow = new ArrowMetadata(name, configuration);
        entries.Add(name, arrow);
        registryQueue.AddOrInvoke(arrow);
        return arrow;
    }

    public IArrow? GetArrow(string id) 
    {
        ReadOnlySpan<char> name = $"{metadata.Name}/{id}";
        var alternate = entries.GetAlternateLookup<ReadOnlySpan<char>>();
        alternate.TryGetValue(name, out IArrow? value);
        return value;
    }

    internal void Invoke(IArrow entry)
    {
        ArrowsRegistry.Register(entry.Name, entry.Configuration);
    }
}

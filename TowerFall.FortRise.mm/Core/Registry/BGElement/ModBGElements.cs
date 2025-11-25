#nullable enable
using System.Collections.Generic;

namespace FortRise;

public interface IModBGElements
{
    IBGElementEntry? GetBGElement(string id);
    IBGElementEntry RegisterBGElement(string id, in BGElementConfiguration configuration);
}

internal sealed class ModBGElements : IModBGElements
{
    private readonly RegistryQueue<IBGElementEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModBGElements(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IBGElementEntry>(Invoke);
    }

    public IBGElementEntry RegisterBGElement(string id, in BGElementConfiguration configuration)
    {
        string name = $"{metadata.Name}.{id}";
        IBGElementEntry bgElement = new BGElementEntry(name, configuration);
        BGElementsRegistry.AddBGElement(bgElement);
        registryQueue.AddOrInvoke(bgElement);
        return bgElement;
    }

    public IBGElementEntry? GetBGElement(string id)
    {
        return BGElementsRegistry.GetBGElement(id);
    }

    internal void Invoke(IBGElementEntry entry)
    {
        BGElementsRegistry.Register(entry.Name, entry.Configuration);
    }
}
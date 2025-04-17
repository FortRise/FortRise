#nullable enable
using System.Collections.Generic;

namespace FortRise;

public class ModPickups
{
    private readonly Dictionary<string, IPickupEntry> entries = new Dictionary<string, IPickupEntry>();
    private readonly RegistryQueue<IPickupEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModPickups(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IPickupEntry>(Invoke);
    }

    public IPickupEntry RegisterPickups(string id, in PickupConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";       
        IPickupEntry pickup = new PickupEntry(name, configuration);
        entries.Add(name, pickup);
        registryQueue.AddOrInvoke(pickup);
        return pickup;
    }

    internal void Invoke(IPickupEntry entry)
    {
        PickupsRegistry.Register(entry.Name, entry.Configuration);
    }
}

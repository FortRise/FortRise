#nullable enable
using System.Collections.Generic;

namespace FortRise;

public class ModPickups
{
    private readonly Dictionary<string, IPickup> entries = new Dictionary<string, IPickup>();
    private readonly RegistryQueue<IPickup> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModPickups(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IPickup>(Invoke);
    }

    public IPickup RegisterPickups(string id, PickupConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";       
        IPickup pickup = new PickupMetadata(name, configuration);
        entries.Add(name, pickup);
        registryQueue.AddOrInvoke(pickup);
        return pickup;
    }

    internal void Invoke(IPickup entry)
    {
        PickupsRegistry.Register(entry.Name, entry.Configuration);
    }
}

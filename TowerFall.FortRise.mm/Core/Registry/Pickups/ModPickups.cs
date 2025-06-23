#nullable enable
using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public interface IModPickups
{
    IPickupEntry RegisterPickups(string id, in PickupConfiguration configuration);
    IPickupEntry? GetPickup(string id);
}

internal sealed class ModPickups : IModPickups
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
        IPickupEntry pickup = new PickupEntry(name, EnumPool.Obtain<Pickups>(), configuration);
        entries.Add(name, pickup);
        registryQueue.AddOrInvoke(pickup);
        return pickup;
    }

    public IPickupEntry? GetPickup(string id)
    {
        ReadOnlySpan<char> name = $"{metadata.Name}/{id}";
        var alternate = entries.GetAlternateLookup<ReadOnlySpan<char>>();
        alternate.TryGetValue(name, out IPickupEntry? value);
        return value;
    }

    internal void Invoke(IPickupEntry entry)
    {
        PickupsRegistry.Register(entry.Name, entry.Pickups, entry.Configuration);
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public interface IModPickups
{
    IReadOnlyDictionary<string, IPickupEntry> RegisteredPickups { get; }
    [Obsolete("Use 'RegisterPickup' instead")]
    IPickupEntry RegisterPickups(string id, in PickupConfiguration configuration) => RegisterPickup(id, configuration);
    IPickupEntry RegisterPickup(string id, in PickupConfiguration configuration);
    IPickupEntry RegisterArrowPickup(string id, IArrowEntry entry);
    IPickupEntry RegisterArrowPickup(string id, IArrowEntry entry, in PickupConfiguration configuration);
    IPickupEntry? GetPickup(string id);
}

internal sealed class ModPickups : IModPickups
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<IPickupEntry> registryQueue;

    public IReadOnlyDictionary<string, IPickupEntry> RegisteredPickups => PickupsRegistry.GetAllPickups();

    internal ModPickups(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IPickupEntry>(metadata, static _ => {}, Detach);
    }

    private void Detach(IPickupEntry entry)
    {
        PickupsRegistry.RemovePickup(entry);
        EnumPool.Free(entry.Pickups);
    }

    public IPickupEntry RegisterPickup(string id, in PickupConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        IPickupEntry pickup = new PickupEntry(name, EnumPool.Obtain<Pickups>(), configuration);
        PickupsRegistry.AddPickup(pickup);
        registryQueue.AddOrInvoke(pickup);
        return pickup;
    }

    public IPickupEntry? GetPickup(string id)
    {
        return PickupsRegistry.GetPickup(id);
    }

    public IPickupEntry RegisterArrowPickup(string id, IArrowEntry entry, in PickupConfiguration configuration)
    {
        var realConf = new PickupConfiguration()
        {
            CreatePickup = configuration.CreatePickup,
            ArrowType = configuration.ArrowType.HasValue ? configuration.ArrowType : entry.ArrowTypes,
            Chance = configuration.Chance
        };
    
        string name = $"{metadata.Name}/{id}";
        IPickupEntry pickup = new PickupEntry(name, EnumPool.Obtain<Pickups>(), realConf);
        PickupsRegistry.AddPickup(pickup);
        registryQueue.AddOrInvoke(pickup);

        return pickup;
    }

    public IPickupEntry RegisterArrowPickup(string id, IArrowEntry entry)
    {
        string name = $"{metadata.Name}/{id}";
        IPickupEntry pickup = new PickupEntry(name, EnumPool.Obtain<Pickups>(), new()
        {
            CreatePickup = (e) => new ArrowTypePickup(e.Position, e.TargetPosition, entry.ArrowTypes),
            ArrowType = entry.ArrowTypes,
            Chance = 1f
        });
        PickupsRegistry.AddPickup(pickup);
        registryQueue.AddOrInvoke(pickup);

        return pickup;
    }
}

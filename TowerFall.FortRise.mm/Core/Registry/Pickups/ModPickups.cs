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
    private readonly ModuleMetadata metadata;

    internal ModPickups(ModuleMetadata metadata)
    {
        this.metadata = metadata;
    }

    public IPickupEntry RegisterPickups(string id, in PickupConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        IPickupEntry pickup = new PickupEntry(name, EnumPool.Obtain<Pickups>(), configuration);
        PickupsRegistry.AddPickup(pickup);
        return pickup;
    }

    public IPickupEntry? GetPickup(string id)
    {
        return PickupsRegistry.GetPickup(id);
    }
}

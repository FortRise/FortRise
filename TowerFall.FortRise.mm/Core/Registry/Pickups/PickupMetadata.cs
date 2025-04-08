#nullable enable
using TowerFall;

namespace FortRise;

internal class PickupMetadata : IPickup
{
    public string Name { get; init; }
    public PickupConfiguration Configuration { get; init; }


    public PickupMetadata(string name, PickupConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }

    public Pickups ToPickupType()
    {
        return ModRegisters.PickupType(Name);
    }
}

#nullable enable
using TowerFall;

namespace FortRise;

internal class PickupEntry : IPickupEntry
{
    public string Name { get; init; }
    public PickupConfiguration Configuration { get; init; }


    public PickupEntry(string name, PickupConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }

    public Pickups ToPickupType()
    {
        return ModRegisters.PickupType(Name);
    }
}

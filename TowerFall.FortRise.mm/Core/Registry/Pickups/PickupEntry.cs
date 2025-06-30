#nullable enable
using TowerFall;

namespace FortRise;

internal class PickupEntry : IPickupEntry
{
    public string Name { get; init; }
    public PickupConfiguration Configuration { get; init; }
    public Pickups Pickups { get; init; }


    public PickupEntry(string name, Pickups pickups, PickupConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
        Pickups = pickups;
    }
}

#nullable enable
using TowerFall;

namespace FortRise;

public interface IPickup
{
    string Name { get; init; }
    public PickupConfiguration Configuration { get; init; }
    
    public Pickups ToPickupType();
}

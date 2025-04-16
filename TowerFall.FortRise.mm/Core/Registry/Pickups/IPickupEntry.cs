#nullable enable
using TowerFall;

namespace FortRise;

public interface IPickupEntry
{
    string Name { get; init; }
    public PickupConfiguration Configuration { get; init; }
    
    public Pickups ToPickupType();
}

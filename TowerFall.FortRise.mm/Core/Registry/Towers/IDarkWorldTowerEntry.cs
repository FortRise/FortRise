#nullable enable
using TowerFall;

namespace FortRise;

public interface IDarkWorldTowerEntry : ITowerEntry
{
    public DarkWorldTowerConfiguration Configuration { get; init; }    
    public DarkWorldTowerData DarkWorldTowerData { get; }
}

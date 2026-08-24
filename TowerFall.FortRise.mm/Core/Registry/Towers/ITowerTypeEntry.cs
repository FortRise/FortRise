#nullable enable
using TowerFall;

namespace FortRise;

public interface ITowerTypeEntry
{
    public string Name { get; init; }
    public MapButton.TowerType TowerType { get; init; }
    public TowerTypeConfiguration Configuration { get; init; }
}


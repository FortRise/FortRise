#nullable enable
using TowerFall;

namespace FortRise;

internal sealed class TowerTypeEntry(string name, in MapButton.TowerType towerType, in TowerTypeConfiguration configuration) : ITowerTypeEntry
{
    public string Name { get; init; } = name;
    public MapButton.TowerType TowerType { get; init; } = towerType;
    public TowerTypeConfiguration Configuration { get; init; } = configuration;
}


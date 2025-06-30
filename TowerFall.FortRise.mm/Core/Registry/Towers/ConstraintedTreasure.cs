#nullable enable
using TowerFall;

namespace FortRise;

public readonly struct ConstraintedTreasure()
{
    public required Pickups Pickups { get; init; }
    public Option<int> MinPlayer { get; init; }
    public Option<int> MaxPlayer { get; init; }
}

#nullable enable
using TowerFall;

namespace FortRise;

public readonly struct ConstraintedTreasure()
{
    public required Pickups Pickups { get; init; }
    public int MinPlayer { get; init; } = 1;
    public int MaxPlayer { get; init; } = 4;
}

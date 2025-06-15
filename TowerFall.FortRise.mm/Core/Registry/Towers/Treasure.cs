#nullable enable
using TowerFall;

namespace FortRise;

public readonly struct Treasure
{
    /// <summary>
    /// An enum pickup to use.
    /// </summary>
    public required Pickups Pickup { get; init; }
    /// <summary>
    /// Override the chance of a pickup.
    /// </summary>
    public Option<float> Chance { get; init; }
    /// <summary>
    /// Override the rate of a pickup.
    /// </summary>
    public Option<int> Rates { get; init; }
}

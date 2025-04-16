#nullable enable
namespace FortRise;

public readonly struct EnemyConfiguration
{
    public required string Name { get; init; }
    public required EnemyLoader Loader { get; init; }
}

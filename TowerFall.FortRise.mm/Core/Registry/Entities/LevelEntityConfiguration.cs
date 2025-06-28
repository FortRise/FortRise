#nullable enable
namespace FortRise;

public readonly struct LevelEntityConfiguration
{
    public required string Name { get; init; }
    public required LevelEntityLoader Loader { get; init; }
}

#nullable enable
namespace FortRise;

public readonly struct VersusTowerConfiguration
{
    public required string Theme { get; init; }
    public required IResourceInfo[] Levels { get; init; }
    public Treasure[]? Treasure { get; init; }
    public string? Author { get; init; }
    public bool ArrowShuffle { get; init; }
    public bool Procedural { get; init; }
    public float SpecialArrowRate { get; init; }
}

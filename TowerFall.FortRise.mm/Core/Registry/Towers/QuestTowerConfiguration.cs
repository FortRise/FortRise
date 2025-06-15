#nullable enable
namespace FortRise;

public readonly struct QuestTowerConfiguration
{
    public required IResourceInfo Level { get; init; }
    public required IResourceInfo Data { get; init; }
    public required string Theme { get; init; }
    public string? Author { get; init; }
}

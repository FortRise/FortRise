#nullable enable
using Monocle;
using TowerFall;

namespace FortRise;

public readonly struct VariantConfiguration 
{
    public required string Title { get; init; }
    public required Subtexture Icon { get; init; }
    public string? Header { get; init; }
    public string? Description { get; init; }
    public CustomVariantFlags Flags { get; init; }
    public Pickups[]? Exclusions { get; init; }
    public IVariantEntry[]? Links { get; init; }
    public IArrowEntry? StartWith { get; init; }
}

#nullable enable
using Monocle;

namespace FortRise;

public readonly struct PresetConfiguration 
{
    public required string Name { get; init; }
    public required Subtexture Icon { get; init; }
    public required IVariantEntry[] Variants { get; init; }
    public string? Description { get; init; }
}

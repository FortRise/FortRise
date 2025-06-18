#nullable enable
namespace FortRise;

public readonly struct StatueInfo
{
    public required ISubtextureEntry Image { get; init; }
    public required ISubtextureEntry Glow { get; init; }
}

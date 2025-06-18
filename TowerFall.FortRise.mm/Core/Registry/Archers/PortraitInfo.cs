#nullable enable
namespace FortRise;

public readonly struct PortraitInfo
{
    public required ISubtextureEntry NotJoined { get; init; }
    public required ISubtextureEntry Joined { get; init; }
    public required ISubtextureEntry Win { get; init; }
    public required ISubtextureEntry Lose { get; init; }
}
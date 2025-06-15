#nullable enable
namespace FortRise;

public readonly struct TrialsTier()
{
    public required IResourceInfo Level { get; init; }
    public required string Theme { get; init; }
    public required double DevTime { get; init; }
    public required double DiamondTime { get; init; }
    public required double GoldTime { get; init; }
    public int Arrows { get; init; } = 3;
    public int SwitchBlockTimer { get; init; } = 200;
}

#nullable enable
namespace FortRise;

public readonly struct TrialsTowerConfiguration
{
    public string? Author { get; init; }
    public required TrialsTier Tier1 { get; init; }
    public required TrialsTier Tier2 { get; init; }
    public required TrialsTier Tier3 { get; init; }
}

#nullable enable
namespace FortRise;

public readonly struct QuestEventConfiguration
{
    public required QuestEventAction Appear { get; init; }
    public required QuestEventAction Disappear { get; init; }
}

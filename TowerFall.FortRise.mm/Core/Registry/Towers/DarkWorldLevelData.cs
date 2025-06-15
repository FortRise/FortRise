#nullable enable
namespace FortRise;

public readonly struct DarkWorldLevelData()
{
    public required int LevelIndex { get; init; }
    public int Waves { get; init; } = 3;
    public int Difficulty { get; init; }
    public string? EnemySet { get; init; }
    public Option<int> BossID { get; init; }
    public ConstraintedTreasure[]? Treasures { get; init; }
    public string[]? Variants { get; init; }
    public float DelayMultiplier { get; init; } = 1f;
}
#nullable enable
using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public readonly struct DarkWorldTowerConfiguration()
{
    public required string Theme { get; init; }
    public required IResourceInfo[] Levels { get; init; }
    public required Dictionary<string, List<DarkWorldTowerData.EnemyData>> EnemySets { get; init; }
    public required DarkWorldLevelData[] Normal { get; init; }
    public required DarkWorldLevelData[] Hardcore { get; init; }
    public required DarkWorldLevelData[] Legendary { get; init; }
    public string? Author { get; init; }

    public int TimeBase { get; init; } = 300;
    public int TimeAdd { get; init; } = 40;

    public int StartingLives { get; init; } = -1;
    public int[] MaxContinues { get; init; } = [-1, -1, -1];

    public Func<IDarkWorldTowerEntry, bool>? ShowLocked { get; init; }
    public Func<IDarkWorldTowerEntry, bool>? IsHidden { get; init; }
}

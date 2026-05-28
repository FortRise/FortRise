#nullable enable
using System;
using Microsoft.Xna.Framework;

namespace FortRise;

public interface ITowerEntry
{
    public string ID { get; init; }
    [Obsolete("Use ITowerEntry.TowerSet instead")]
    public string LevelSet => TowerSet;
    public string TowerSet { get; init; }
    public Point LevelIndex { get; internal set; }
}

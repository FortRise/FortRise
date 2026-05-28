#nullable enable
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

internal sealed class DarkWorldTowerEntry : IDarkWorldTowerEntry
{
    public string ID { get; init; }
    public DarkWorldTowerConfiguration Configuration { get; init; }
    public DarkWorldTowerData DarkWorldTowerData => GetDarkWorldTowerData();
    public Point LevelIndex { get; set; }
    public string TowerSet { get; init; }

    public DarkWorldTowerEntry(string id, string towerSet, DarkWorldTowerConfiguration configuration)
    {
        ID = id;
        TowerSet = towerSet;
        Configuration = configuration;
    }

    private DarkWorldTowerData GetDarkWorldTowerData()
    {
        return GameData.DarkWorldTowers[LevelIndex.X];
    }
}

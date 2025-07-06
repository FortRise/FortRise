#nullable enable
using TowerFall;

namespace FortRise;

internal sealed class DarkWorldTowerEntry : IDarkWorldTowerEntry
{
    public string ID { get; init; }
    public string LevelSet { get; init; }
    public DarkWorldTowerConfiguration Configuration { get; init; }
    public DarkWorldTowerData DarkWorldTowerData => GetDarkWorldTowerData();

    public DarkWorldTowerEntry(string id, string levelSet, DarkWorldTowerConfiguration configuration)
    {
        ID = id;
        LevelSet = levelSet;
        Configuration = configuration;
    }

    private DarkWorldTowerData GetDarkWorldTowerData()
    {
        return TowerRegistry.DarkWorldGet(LevelSet, ID);
    }
}

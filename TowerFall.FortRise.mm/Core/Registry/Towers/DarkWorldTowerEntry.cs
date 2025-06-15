#nullable enable
namespace FortRise;

internal sealed class DarkWorldTowerEntry : IDarkWorldTowerEntry
{
    public string ID { get; init; }
    public string LevelSet { get; init; }
    public DarkWorldTowerConfiguration Configuration { get; init; }

    public DarkWorldTowerEntry(string id, string levelSet, DarkWorldTowerConfiguration configuration)
    {
        ID = id;
        LevelSet = levelSet;
        Configuration = configuration;
    }
}

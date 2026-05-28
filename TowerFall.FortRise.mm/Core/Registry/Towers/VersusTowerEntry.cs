#nullable enable
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

internal sealed class VersusTowerEntry : IVersusTowerEntry
{
    public VersusTowerConfiguration Configuration { get; init; }
    public string ID { get; init; }
    public VersusTowerData? VersusTowerData => GameData.VersusTowers[LevelIndex.X];
    public Point LevelIndex { get; set; }
    public string TowerSet { get; init; }

    public VersusTowerEntry(string id, string towerSet, VersusTowerConfiguration configuration)
    {
        ID = id;
        TowerSet = towerSet;
        Configuration = configuration;
    }
}

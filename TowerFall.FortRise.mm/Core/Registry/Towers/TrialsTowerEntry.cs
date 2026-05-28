#nullable enable
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

internal sealed class TrialsTowerEntry : ITrialsTowerEntry
{
    public string ID { get; init; }
    public Point LevelIndex { get; set; }
    public TrialsTowerConfiguration Configuration { get; init; }
    public TrialsLevelData? TrialsLevelDataTier1 => GetTrialTower(0);
    public TrialsLevelData? TrialsLevelDataTier2 => GetTrialTower(1);
    public TrialsLevelData? TrialsLevelDataTier3 => GetTrialTower(2);

    public TrialsLevelData? TrialsLevelData => GameData.TrialsLevels[LevelIndex.X, LevelIndex.Y];

    public string TowerSet { get; init; }

    public TrialsTowerEntry(string id, string towerSet, TrialsTowerConfiguration configuration)
    {
        ID = id;
        TowerSet = towerSet;
        Configuration = configuration;
    }

    private TrialsLevelData GetTrialTower(int tier)
    {
        return GameData.TrialsLevels[LevelIndex.X, tier];
    }
}

#nullable enable
using TowerFall;

namespace FortRise;

internal sealed class TrialsTowerEntry : ITrialsTowerEntry
{
    public string ID { get; init; }
    public string LevelSet { get; init; }
    public TrialsTowerConfiguration Configuration { get; init; }
    public TrialsLevelData? TrialsLevelDataTier1 => GetTrialTower(0);
    public TrialsLevelData? TrialsLevelDataTier2 => GetTrialTower(1);
    public TrialsLevelData? TrialsLevelDataTier3 => GetTrialTower(2);

    public TrialsTowerEntry(string id, string levelSet, TrialsTowerConfiguration configuration)
    {
        ID = id;
        LevelSet = levelSet;
        Configuration = configuration;
    }

    private TrialsLevelData GetTrialTower(int tier)
    {
        return TowerRegistry.TrialsGet(LevelSet, tier, ID + "-" + (tier + 1));
    }
}

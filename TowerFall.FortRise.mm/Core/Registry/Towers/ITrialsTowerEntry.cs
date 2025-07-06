#nullable enable
using TowerFall;

namespace FortRise;

public interface ITrialsTowerEntry : ITowerEntry
{
    public TrialsTowerConfiguration Configuration { get; init; }
    public TrialsLevelData? TrialsLevelDataTier1 { get; }
    public TrialsLevelData? TrialsLevelDataTier2 { get; }
    public TrialsLevelData? TrialsLevelDataTier3 { get; }
}

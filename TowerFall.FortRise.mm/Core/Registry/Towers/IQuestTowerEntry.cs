#nullable enable
using TowerFall;

namespace FortRise;

public interface IQuestTowerEntry : ITowerEntry
{
    public QuestTowerConfiguration Configuration { get; init; }
    public QuestLevelData? QuestLevelData { get; }
}

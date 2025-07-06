#nullable enable
using TowerFall;

namespace FortRise;

internal sealed class QuestTowerEntry : IQuestTowerEntry
{
    public QuestTowerConfiguration Configuration { get; init; }
    public string ID { get; init; }
    public string LevelSet { get; init; }
    public QuestLevelData? QuestLevelData => GetQuestLevelData();

    public QuestTowerEntry(string id, string levelSet, QuestTowerConfiguration configuration)
    {
        ID = id;
        LevelSet = levelSet;
        Configuration = configuration;
    }

    private QuestLevelData GetQuestLevelData()
    {
        return TowerRegistry.QuestGet(LevelSet, ID);
    }
}

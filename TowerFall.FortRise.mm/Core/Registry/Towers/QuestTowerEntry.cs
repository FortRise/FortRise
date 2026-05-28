#nullable enable
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

internal sealed class QuestTowerEntry : IQuestTowerEntry
{
    public QuestTowerConfiguration Configuration { get; init; }
    public string ID { get; init; }
    public Point LevelIndex { get; set; }
    public QuestLevelData? QuestLevelData => GetQuestLevelData();

    public string TowerSet { get; init; }

    public QuestTowerEntry(string id, string towerSet, QuestTowerConfiguration configuration)
    {
        ID = id;
        TowerSet = towerSet;
        Configuration = configuration;
    }

    private QuestLevelData GetQuestLevelData()
    {
        return GameData.QuestLevels[LevelIndex.X];
    }
}

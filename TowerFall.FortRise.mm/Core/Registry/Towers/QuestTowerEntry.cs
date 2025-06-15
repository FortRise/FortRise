#nullable enable
namespace FortRise;

internal sealed class QuestTowerEntry : IQuestTowerEntry
{
    public QuestTowerConfiguration Configuration { get; init; }
    public string ID { get; init; }
    public string LevelSet { get; init; }

    public QuestTowerEntry(string id, string levelSet, QuestTowerConfiguration configuration)
    {
        ID = id;
        LevelSet = levelSet;
        Configuration = configuration;
    }
}

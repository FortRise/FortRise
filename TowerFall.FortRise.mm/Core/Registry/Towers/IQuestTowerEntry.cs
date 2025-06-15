#nullable enable
namespace FortRise;

public interface IQuestTowerEntry : ITowerEntry
{
    public QuestTowerConfiguration Configuration { get; init; }
}

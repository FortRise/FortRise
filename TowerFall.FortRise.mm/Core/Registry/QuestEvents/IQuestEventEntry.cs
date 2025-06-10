#nullable enable
namespace FortRise;

public interface IQuestEventEntry 
{
    string Name { get; }
    QuestEventConfiguration Configuration { get; }
}

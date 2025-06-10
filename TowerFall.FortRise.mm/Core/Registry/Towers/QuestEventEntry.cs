#nullable enable
namespace FortRise;

internal class QuestEventEntry(string name, QuestEventConfiguration configuration) : IQuestEventEntry
{
    public string Name { get; init; } = name;
    public QuestEventConfiguration Configuration { get; init; } = configuration;
}

#nullable enable
namespace FortRise;

internal sealed class LevelEntityEntry : ILevelEntityEntry
{
    public string ID { get; init; }
    public LevelEntityConfiguration Configuration { get; init; }

    public LevelEntityEntry(string name, LevelEntityConfiguration configuration)
    {
        ID = name;
        Configuration = configuration;
    }
}

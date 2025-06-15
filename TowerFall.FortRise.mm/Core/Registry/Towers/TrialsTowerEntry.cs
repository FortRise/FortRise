#nullable enable
namespace FortRise;

internal sealed class TrialsTowerEntry : ITrialsTowerEntry
{
    public string ID { get; init; }
    public string LevelSet { get; init; }
    public TrialsTowerConfiguration Configuration { get; init; }

    public TrialsTowerEntry(string id, string levelSet, TrialsTowerConfiguration configuration)
    {
        ID = id;
        LevelSet = levelSet;
        Configuration = configuration;
    }
}

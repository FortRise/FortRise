#nullable enable
namespace FortRise;

internal sealed class VersusTowerEntry : IVersusTowerEntry
{
    public VersusTowerConfiguration Configuration { get; init; }
    public string ID { get; init; }
    public string LevelSet { get; init; }

    public VersusTowerEntry(string id, string levelSet, VersusTowerConfiguration configuration)
    {
        ID = id;
        LevelSet = levelSet;
        Configuration = configuration;
    }
}

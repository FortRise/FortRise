#nullable enable
namespace FortRise;

internal class DarkWorldBossEntry : IDarkWorldBossEntry
{
    public string Name { get; init; }
    public DarkWorldBossConfiguration Configuration { get; init; }


    public DarkWorldBossEntry(string name, DarkWorldBossConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }
}

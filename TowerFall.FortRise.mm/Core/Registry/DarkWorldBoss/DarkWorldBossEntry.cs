#nullable enable
namespace FortRise;

internal class DarkWorldBossEntry : IDarkWorldBossEntry
{
    public string Name { get; init; }
    public int BossID { get; init; }
    public DarkWorldBossConfiguration Configuration { get; init; }

    public DarkWorldBossEntry(string name, int bossID, DarkWorldBossConfiguration configuration)
    {
        Name = name;
        BossID = bossID;
        Configuration = configuration;
    }
}

#nullable enable
namespace FortRise;

public interface IDarkWorldBossEntry
{
    public string Name { get; init; }
    public int BossID { get; init; }
    public DarkWorldBossConfiguration Configuration { get; init; }
}

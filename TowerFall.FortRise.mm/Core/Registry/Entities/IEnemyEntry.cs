#nullable enable
namespace FortRise;

public interface IEnemyEntry
{
    public string ID { get; init; }
    public EnemyConfiguration Configuration { get; init; }
}

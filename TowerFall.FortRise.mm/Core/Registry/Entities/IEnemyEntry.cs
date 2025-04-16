#nullable enable
namespace FortRise;

public interface IEnemyEntry
{
    public string Name { get; init; }
    public EnemyConfiguration Configuration { get; init; }
}

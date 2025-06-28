#nullable enable
namespace FortRise;

internal class EnemyEntry : IEnemyEntry
{
    public string ID { get; init; }
    public EnemyConfiguration Configuration { get; init; }


    public EnemyEntry(string name, EnemyConfiguration configuration)
    {
        ID = name;
        Configuration = configuration;
    }
}

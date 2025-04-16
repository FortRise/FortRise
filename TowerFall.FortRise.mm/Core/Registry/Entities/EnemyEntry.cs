#nullable enable
namespace FortRise;

internal class EnemyEntry : IEnemyEntry
{
    public string Name { get; init; }
    public EnemyConfiguration Configuration { get; init; }


    public EnemyEntry(string name, EnemyConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }
}

using System.Collections.Generic;

namespace FortRise;

/// <summary>
/// A class that registers custom entity from types.
/// </summary>
public static class EntityRegistry 
{
    public static Dictionary<string, IEnemyEntry> EnemyEntries = [];
    public static Dictionary<string, ILevelEntityEntry> EntityEntries = [];
    public static Dictionary<string, EnemyLoader> EnemyLoader = new();
    public static Dictionary<string, LevelEntityLoader> LevelEntityLoader = new();

    public static void AddEnemy(IEnemyEntry enemyEntry)
    {
        EnemyEntries[enemyEntry.ID] = enemyEntry;
    }

    public static void AddLevelEntity(ILevelEntityEntry entityEntry)
    {
        EntityEntries[entityEntry.ID] = entityEntry;
    }


#nullable enable
    public static IEnemyEntry? GetEnemy(string id)
    {
        EnemyEntries.TryGetValue(id, out var entry);
        return entry;
    }

    public static ILevelEntityEntry? GetLevelEntity(string id)
    {
        EntityEntries.TryGetValue(id, out var entry);
        return entry;
    }
#nullable disable

    public static void AddEnemy(string id, EnemyConfiguration configuration)
    {
        EnemyLoader[id] = configuration.Loader;
    }

    public static void AddLevelEntity(string id, LevelEntityConfiguration configuration)
    {
        LevelEntityLoader[id] = configuration.Loader;
    }
}

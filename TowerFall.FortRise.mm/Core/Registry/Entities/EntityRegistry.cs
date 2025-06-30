#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using TowerFall;

namespace FortRise;

/// <summary>
/// A class that registers custom entity from types.
/// </summary>
public static class EntityRegistry 
{
    private static Dictionary<string, IEnemyEntry> enemyEntries = [];
    private static Dictionary<string, ILevelEntityEntry> entityEntries = [];
    public static Dictionary<string, EnemyLoader> EnemyLoader = new();
    public static Dictionary<string, LevelEntityLoader> LevelEntityLoader = new();

    public static void AddEnemy(IEnemyEntry enemyEntry)
    {
        enemyEntries[enemyEntry.ID] = enemyEntry;
    }

    public static void AddLevelEntity(ILevelEntityEntry entityEntry)
    {
        entityEntries[entityEntry.ID] = entityEntry;
    }


#nullable enable
    public static IEnemyEntry? GetEnemy(string id)
    {
        enemyEntries.TryGetValue(id, out var entry);
        return entry;
    }

    public static ILevelEntityEntry? GetLevelEntity(string id)
    {
        entityEntries.TryGetValue(id, out var entry);
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
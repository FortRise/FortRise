#nullable enable
using System;
using System.Collections.Generic;

namespace FortRise;

public class ModEnemies
{
    private readonly Dictionary<string, IEnemyEntry> entries = new Dictionary<string, IEnemyEntry>();
    private readonly RegistryQueue<IEnemyEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModEnemies(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IEnemyEntry>(Invoke);
    }

    public IEnemyEntry RegisterEnemy(string id, EnemyConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";

        IEnemyEntry enemy = new EnemyEntry(name, configuration);
        entries.Add(name, enemy);
        registryQueue.AddOrInvoke(enemy);
        return enemy;
    }

    public IEnemyEntry? GetEnemy(string id) 
    {
        ReadOnlySpan<char> name = $"{metadata.Name}/{id}";
        var alternate = entries.GetAlternateLookup<ReadOnlySpan<char>>();
        alternate.TryGetValue(name, out IEnemyEntry? value);
        return value;
    }

    internal void Invoke(IEnemyEntry entry)
    {
        EntityRegistry.AddEnemy(entry.Name, entry.Configuration);
    }
}

#nullable enable
using System;
using System.Collections.Generic;

namespace FortRise;

public interface IModEnemies
{
    IEnemyEntry? GetEnemy(string id);
    IEnemyEntry RegisterEnemy(string id, in EnemyConfiguration configuration);
}

internal sealed class ModEnemies : IModEnemies
{
    private readonly Dictionary<string, IEnemyEntry> entries = new Dictionary<string, IEnemyEntry>();
    private readonly RegistryQueue<IEnemyEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModEnemies(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IEnemyEntry>(Invoke);
    }

    public IEnemyEntry RegisterEnemy(string id, in EnemyConfiguration configuration)
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

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
        EntityRegistry.AddEnemy(enemy);
        registryQueue.AddOrInvoke(enemy);
        return enemy;
    }

    public IEnemyEntry? GetEnemy(string id)
    {
        return EntityRegistry.GetEnemy(id);
    }

    internal void Invoke(IEnemyEntry entry)
    {
        EntityRegistry.AddEnemy(entry.Name, entry.Configuration);
    }
}

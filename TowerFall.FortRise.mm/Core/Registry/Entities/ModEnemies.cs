#nullable enable
using System;
using System.Collections.Generic;

namespace FortRise;

public interface IEnemy
{
    public string Name { get; init; }
    public EnemyConfiguration Configuration { get; init; }
}

public readonly struct EnemyConfiguration
{
    public required string Name { get; init; }
    public required EnemyLoader Loader { get; init; }
}

internal class EnemyMetadata : IEnemy
{
    public string Name { get; init; }
    public EnemyConfiguration Configuration { get; init; }


    public EnemyMetadata(string name, EnemyConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }
}

public class ModEnemies
{
    private readonly Dictionary<string, IEnemy> entries = new Dictionary<string, IEnemy>();
    private readonly RegistryQueue<IEnemy> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModEnemies(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IEnemy>(Invoke);
    }

    public IEnemy RegisterEnemy(string id, EnemyConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";

        IEnemy enemy = new EnemyMetadata(name, configuration);
        entries.Add(name, enemy);
        registryQueue.AddOrInvoke(enemy);
        return enemy;
    }

    public IEnemy? GetEnemy(string id) 
    {
        ReadOnlySpan<char> name = $"{metadata.Name}/{id}";
        var alternate = entries.GetAlternateLookup<ReadOnlySpan<char>>();
        alternate.TryGetValue(name, out IEnemy? value);
        return value;
    }

    internal void Invoke(IEnemy entry)
    {
        EntityRegistry.AddEnemy(entry.Name, entry.Configuration);
    }
}

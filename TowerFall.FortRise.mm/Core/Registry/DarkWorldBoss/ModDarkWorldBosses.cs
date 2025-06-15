#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using TowerFall;

namespace FortRise;

public class ModDarkWorldBosses
{
    private readonly Dictionary<string, IDarkWorldBossEntry> entries = new Dictionary<string, IDarkWorldBossEntry>();
    private readonly RegistryQueue<IDarkWorldBossEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModDarkWorldBosses(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IDarkWorldBossEntry>(Invoke);
    }

    public IDarkWorldBossEntry RegisterDarkWorldBoss(string id, in DarkWorldBossConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";

        IDarkWorldBossEntry enemy = new DarkWorldBossEntry(name, IDPool.Obtain("boss"), configuration);
        entries.Add(name, enemy);
        registryQueue.AddOrInvoke(enemy);
        return enemy;
    }

    public IDarkWorldBossEntry? GetDarkWorldBoss(string id) 
    {
        ReadOnlySpan<char> name = $"{metadata.Name}/{id}";
        var alternate = entries.GetAlternateLookup<ReadOnlySpan<char>>();
        alternate.TryGetValue(name, out IDarkWorldBossEntry? value);
        return value;
    }

    internal void Invoke(IDarkWorldBossEntry entry)
    {
        var bossName = entry.Name;

        ConstructorInfo? ctor;
        DarkWorldBossLoader? loader = null;
        ctor = entry.Configuration.DarkWorldBossType.GetConstructor([typeof(int)]);
        if (ctor != null)
        {
            loader = diff =>
            {
                var invoked = (DarkWorldBoss)ctor.Invoke([diff]);
                return invoked;
            };
            goto Loaded;
        }

        Loaded:
        DarkWorldBossRegistry.DarkWorldBosses[bossName] = entry.BossID;
        DarkWorldBossRegistry.DarkWorldBossLoader[entry.BossID] = loader;
    }
}
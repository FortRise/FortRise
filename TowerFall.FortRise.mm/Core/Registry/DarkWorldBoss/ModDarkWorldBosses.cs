#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using TowerFall;

namespace FortRise;

public interface IModDarkWorldBosses
{
    IDarkWorldBossEntry? GetDarkWorldBoss(string id);
    IDarkWorldBossEntry RegisterDarkWorldBoss(string id, in DarkWorldBossConfiguration configuration);
}

internal class ModDarkWorldBosses : IModDarkWorldBosses
{
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

        IDarkWorldBossEntry boss = new DarkWorldBossEntry(name, IDPool.Obtain("boss"), configuration);
        DarkWorldBossRegistry.AddDarkWorldBoss(boss);
        registryQueue.AddOrInvoke(boss);
        return boss;
    }

    public IDarkWorldBossEntry? GetDarkWorldBoss(string id)
    {
        return DarkWorldBossRegistry.GetDarkWorldBoss(id);
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
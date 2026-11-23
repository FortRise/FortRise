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
        registryQueue = manager.CreateQueue<IDarkWorldBossEntry>(metadata, Invoke, Detach);
    }

    public IDarkWorldBossEntry RegisterDarkWorldBoss(string id, in DarkWorldBossConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";

        IDarkWorldBossEntry boss = new DarkWorldBossEntry(name, ClaimOrObtain(name), configuration);
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

    private int ClaimOrObtain(string name)
    {
        return IndexReservationPool.For(metadata) is { } session && session.TryClaim("boss", name, out int id)
            ? id
            : IDPool.Obtain("boss");
    }

    internal void Detach(IDarkWorldBossEntry entry)
    {
        DarkWorldBossRegistry.RemoveDarkWorldBoss(entry);

        int id = entry.BossID;
        if (IndexReservationPool.For(metadata) is { } session)
        {
            session.Reserve("boss", entry.Name, id, () => IDPool.Free("boss", id));
            return;
        }

        IDPool.Free("boss", id);
    }
}
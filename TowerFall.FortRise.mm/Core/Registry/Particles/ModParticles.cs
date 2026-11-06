#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Monocle;
using TowerFall;

namespace FortRise;

public interface IModParticles
{
    IReadOnlyDictionary<string, IParticleEntry> RegisteredParticles { get; }

    IParticleEntry RegisterParticle(string id, in ParticleConfiguration configuration);

    IParticleEntry? GetParticle(string id);
}

internal sealed class ModParticles : IModParticles
{
    private readonly ModuleMetadata metadata;
    private readonly Dictionary<string, IParticleEntry> particleEntries = [];
    private static Dictionary<string, IParticleEntry> cachedVanillaEntry = [];
    private readonly RegistryQueue<IParticleEntry> registryQueue;

    public IReadOnlyDictionary<string, IParticleEntry> RegisteredParticles => particleEntries;

    internal ModParticles(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IParticleEntry>(Invoke);
    }

    public IParticleEntry RegisterParticle(string id, in ParticleConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        IParticleEntry entry = new ParticleEntry(name, configuration);
        particleEntries.Add(name, entry);
        registryQueue.AddOrInvoke(entry);
        return entry;
    }

    public IParticleEntry? GetParticle(string id)
    {
        if (!particleEntries.TryGetValue(id, out var particle))
        {
            particle = CreateVanillaEntry(id);
            if (Unsafe.IsNullRef(ref particle))
            {
                return null;
            }
        }

        return particle;
    }


    public static IParticleEntry? CreateVanillaEntry(string id)
    {
        ref var cached = ref CollectionsMarshal.GetValueRefOrAddDefault(cachedVanillaEntry, id, out bool exists);

        if (exists)
        {
            return cached;
        }

        var particle = typeof(Particles).GetField(id);

        if (particle is null)
        {
            return default;
        }

        var type = particle.FieldType;

        if (type == typeof(ParticleType[]))
        {
            RiseCore.logger.LogError($"Particle Type Name: {id} cannot be obtain because it is a multiple particle type.");
        }

        IParticleEntry entry = new ParticleEntry(id, () => (ParticleType)particle.GetValue(null)!, new() { Source = null! });
        cachedVanillaEntry[id] = entry;
        return entry;
    }

    private void Invoke(IParticleEntry entry)
    {
        TowerFall.Patching.Particles.ParticleList[entry.Name] = entry.ParticleType;
    }
}

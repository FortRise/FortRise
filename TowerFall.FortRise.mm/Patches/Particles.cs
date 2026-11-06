using System.Collections.Generic;
using FortRise;
using Microsoft.Extensions.Logging;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.Particles")]
public static class Particles 
{
    public static Dictionary<string, ParticleType> ParticleList { get; set; } = [];

    public static ParticleType GetParticle(string name)
    {
        if (ParticleList.TryGetValue(name, out var type))
        {
            return type;
        }

        var particle = typeof(Particles).GetField(name);

        if (particle is null)
        {
            return default;
        }

        var t = particle.FieldType;

        if (t == typeof(ParticleType[]))
        {
            RiseCore.logger.LogError($"Particle Type Name: {name} cannot be obtain because it is a multiple particle type.");
        }

        return (ParticleType)particle.GetValue(null)!;
    }
}

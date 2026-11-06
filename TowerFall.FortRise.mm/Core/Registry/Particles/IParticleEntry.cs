#nullable enable
using Monocle;

namespace FortRise;

public interface IParticleEntry 
{
    string Name { get; init; }
    ParticleConfiguration Configuration { get; init; }
    public ParticleType? ParticleType { get; }
}


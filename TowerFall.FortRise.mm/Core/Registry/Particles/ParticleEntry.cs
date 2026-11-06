#nullable enable
using System;
using Monocle;

namespace FortRise;

internal sealed class ParticleEntry : IParticleEntry
{
    public string Name { get; init; }
    public ParticleConfiguration Configuration { get; init; }
    public ParticleType? ParticleType => GetActualParticleType();

    private ParticleType? cache;
    private readonly Func<ParticleType>? particleCallback;

    public ParticleEntry(string name, in ParticleConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }

    public ParticleEntry(string name, Func<ParticleType> particleCallback, in ParticleConfiguration configuration)
    {
        Name = name;
        this.particleCallback = particleCallback;
        Configuration = configuration;
    }

    private ParticleType? GetActualParticleType()
    {
        if (cache != null)
        {
            return cache;
        }

        if (particleCallback != null)
        {
            return cache = particleCallback();
        }

        return cache = new ParticleType() 
        {
            Source = Configuration.Source.Subtexture,
            Color = Configuration.Color,
            Color2 = Configuration.Color2,
            ColorSwitch = Configuration.ColorSwitch,
            ColorSwitchLoop = Configuration.ColorSwitchLoop,
            Speed = Configuration.Speed,
            SpeedRange = Configuration.SpeedRange,
            SpeedMultiplier = Configuration.SpeedMultiplier,
            Acceleration = Configuration.Acceleration,
            Direction = Configuration.Direction,
            DirectionRange = Configuration.DirectionRange,
            Life = Configuration.Life,
            LifeRange = Configuration.LifeRange,
            Size = Configuration.Size,
            SizeRange = Configuration.SizeRange,
            Rotated  = Configuration.Rotated,
            RandomRotate = Configuration.RandomRotate,
            ScaleOut = Configuration.ScaleOut
        };
    }
}


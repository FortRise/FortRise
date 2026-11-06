#nullable enable
using Microsoft.Xna.Framework;

namespace FortRise;

public readonly struct ParticleConfiguration()
{
    public required ISubtextureEntry Source { get; init; }
    public Color Color { get; init; } = Color.White;
    public Color Color2 { get; init; } = Color.White;
    public int ColorSwitch { get; init; }
    public bool ColorSwitchLoop { get; init; } = true;
    public float Speed { get; init; }
    public float SpeedRange { get; init; }
    public float SpeedMultiplier { get; init; } = 1f;
    public Vector2 Acceleration { get; init; }
    public float Direction { get; init; }
    public float DirectionRange { get; init; }
    public int Life { get; init; }
    public int LifeRange { get; init; }
    public float Size { get; init; } = 2f;
    public float SizeRange { get; init; }
    public bool Rotated { get; init; } = true;
    public bool RandomRotate { get; init; }
    public bool ScaleOut { get; init; }
}


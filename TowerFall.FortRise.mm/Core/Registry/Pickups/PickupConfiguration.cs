#nullable enable
using System;
using Microsoft.Xna.Framework;

namespace FortRise;

public readonly struct PickupConfiguration
{
    public required string Name { get; init; }
    public required Type PickupType { get; init; }
    public Option<Color> Color { get; init; }
    public Option<Color> ColorB { get; init; }
    public Type? ArrowType { get; init; }
    public float Chance { get; init; }
}

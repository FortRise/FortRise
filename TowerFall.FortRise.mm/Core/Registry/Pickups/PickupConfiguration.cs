#nullable enable
using System;
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

public readonly struct PickupConfiguration()
{
    public required Func<CreatePickupArgs, Pickup> CreatePickup { get; init; }
    public float Chance { get; init; } = 1f;
    public Option<ArrowTypes> ArrowType { get; init; }

    [Obsolete("Use 'ArrowConfiguration.ArrowPickupName' on an arrow instead.")]
    public string? Name { get; init; }
    [Obsolete("Use 'CreatePickup' instead.")]
    public Type? PickupType { get; init; }
    [Obsolete("Use 'ArrowConfiguration.ArrowPickupColor' on an arrow instead.")]
    public Option<Color> Color { get; init; }
    [Obsolete("Use 'ArrowConfiguration.ArrowPickupColorB' on an arrow instead.")]
    public Option<Color> ColorB { get; init; }

    public struct CreatePickupArgs()
    {
        public required Vector2 Position { get; init; }
        public required Vector2 TargetPosition { get; init; } 
        public required int PlayerIndex { get; init; }
    }
}

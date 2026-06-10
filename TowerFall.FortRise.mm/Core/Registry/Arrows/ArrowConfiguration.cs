#nullable enable
using System;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public readonly struct ArrowConfiguration()
{
    public required string ArrowPickupName { get; init; }
    public required Func<patch_Arrow> CreateArrow { get; init; }

    [Obsolete("Use 'CreateArrow' instead")]
    public Type? ArrowType { get; init; }
    public ISubtextureEntry? HUD { get; init; }
    public bool LowPriority { get; init; }
    public Func<SpritePickupArgs, GraphicsComponent>? CreateArrowPickupSprite { get; init; }
    public Color ArrowPickupColor { get; init; } = Color.White;
    public Color ArrowPickupColorB { get; init; } = Color.White;
    public ISFXEntry? ArrowPickupSFX { get; init; }

    public struct SpritePickupArgs()
    {
        public required Vector2 Position;
        public required Vector2 TargetPosition; 
        public required ArrowTypes ArrowTypes;
    }
}

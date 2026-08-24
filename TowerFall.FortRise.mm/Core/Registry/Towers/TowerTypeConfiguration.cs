#nullable enable
using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace FortRise;

public readonly struct TowerTypeConfiguration()
{
    public required ISubtextureEntry BlockTexture { get; init; }
    public required ISubtextureEntry SmallBlockTexture { get; init; }
    public required IBaseSFXEntry TowerSound { get; init; }
    public Func<int, Subtexture>? NumeralTexture { get; init; }

    public Color Tint { get; init; } = Color.White;
}


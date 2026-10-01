#nullable enable
using Microsoft.Xna.Framework;

namespace FortRise;

public readonly struct HairInfo()
{
    public Color Color { get; init; } = Color.White;
    public Option<Color> RedColor { get; init; }
    public Option<Color> BlueColor { get; init; }
    public Color OutlineColor { get; init; } = Color.Black;
    public Vector2 Offset { get; init; }
    public Vector2 DuckingOffset { get; init; }
    public int AddLinks { get; init; }
    public float AddLinkDistance { get; init; }
    public bool ShowOnHat { get; init; }
    public bool ShowOnCrown { get; init; }
    public ISubtextureEntry? Texture { get; init; }
    public ISubtextureEntry? TextureEnd { get; init; }
}

#nullable enable
using Microsoft.Xna.Framework;

namespace FortRise;

public readonly struct BackgroundConfiguration()
{
    public Color BackgroundColor { get; init; } = Color.White;

    public BGLayer[]? Background { get; init; }
    public BGLayer[]? Foreground { get; init; }
}

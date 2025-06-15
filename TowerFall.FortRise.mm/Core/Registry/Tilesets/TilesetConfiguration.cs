#nullable enable
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public readonly struct TilesetConfiguration
{
    public required ISubtextureEntry Texture { get; init; }
    public required AutotileData AutotileData { get; init; }
}
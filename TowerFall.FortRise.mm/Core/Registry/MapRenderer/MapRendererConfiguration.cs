using Microsoft.Xna.Framework;

namespace FortRise;
#nullable enable
public readonly struct MapRendererConfiguration 
{
    public required string LevelSet { get; init; }

    public ISubtextureEntry? Water { get; init; }
    public ISubtextureEntry? Land { get; init; }

    public MapElement[] Elements { get; init; } 

    public Option<int> Width { get; init; }
    public Option<int> Height { get; init; }
}

public readonly struct MapElement
{
    public required Vector2 Position { get; init; }
    public required Either<ISubtextureEntry, AnimatedTowerConfiguration> Sprite { get; init; }
}

public readonly struct AnimatedTowerConfiguration
{
    public required string In { get; init; }
    public required string Out { get; init; }
    public required string Selected { get; init; }
    public required string NotSelected { get; init; }
    public required IMenuSpriteContainerEntry Sprite { get; init; }
    public required string TowerID { get; init; }
}

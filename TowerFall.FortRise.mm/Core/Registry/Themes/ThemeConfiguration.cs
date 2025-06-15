#nullable enable
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public readonly struct ThemeConfiguration
{
    public ThemeConfiguration() {}

    public required string Name { get; init; }
    public ISubtextureEntry? Icon { get; init; }
    public string? Tileset { get; init; }
    public string? BGTileset { get; init; }
    public string? BackgroundID { get; init; }
    public string? Music { get; init; }
    public MapButton.TowerType TowerType { get; init; }
    public Vector2 MapPosition { get; init; }
    public Color DarknessColor { get; init; } = Color.Black;
    public float DarknessOpacity { get; init; } = 0.2f;
    public int Wind { get; init; }
    public TowerTheme.LanternTypes Lanterns { get; init; }
    public TowerTheme.Worlds World { get; init; }
    public Color DrillParticleColor { get; init; } = Color.Red;
    public bool Cold { get; init; }
    public Color CrackedBlockColor { get; init; } = new Color(78, 177, 233);
    public bool Raining { get; init; }
    public bool Cataclysm { get; init; }
    public float[]? InvisibleOpacities { get; init; }
}
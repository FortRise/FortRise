#nullable enable
using System.Collections.Generic;

namespace FortRise;

public readonly struct SpriteConfiguration<T>
{
    public required ISubtextureEntry Texture { get; init; }
    public required int FrameWidth { get; init; }
    public required int FrameHeight { get; init; }
    public int OriginX { get; init; }
    public int OriginY { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public Dictionary<string, object>? AdditionalData { get; init; }
    public required Animation<T>[] Animations { get; init; }

    /// <summary>
    /// Field only for archers
    /// </summary>
    public int[]? HeadXOrigins { get; init; }
    /// <summary>
    /// Field only for archers
    /// </summary>
    public int[]? HeadYOrigins { get; init; }
    /// <summary>
    /// Field only for archers
    /// </summary>
    public ISubtextureEntry? RedTexture { get; init; }
    /// <summary>
    /// Field only for archers
    /// </summary>
    public ISubtextureEntry? BlueTexture { get; init; }
    /// <summary>
    /// Field only for archers
    /// </summary>
    public ISubtextureEntry? RedTeam { get; init; }
    /// <summary>
    /// Field only for archers
    /// </summary>
    public ISubtextureEntry? BlueTeam { get; init; }
    /// <summary>
    /// Field only for archers
    /// </summary>
    public ISubtextureEntry? Flash { get; init; }
}

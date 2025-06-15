#nullable enable
using System;
using Monocle;

namespace FortRise;

public readonly struct ArrowConfiguration 
{
    public required Type ArrowType { get; init; }
    public ISubtextureEntry? HUD { get; init; }
    public bool LowPriority { get; init; }
}

#nullable enable
using System;

namespace FortRise;

public readonly struct EffectConfiguration
{
    public required IResourceInfo EffectFile { get; init; }
    public required Type EffectResourceType { get; init; }
    public required string PassName { get; init; }
}

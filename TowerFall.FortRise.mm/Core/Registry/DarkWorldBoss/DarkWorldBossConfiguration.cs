#nullable enable
using System;

namespace FortRise;

public readonly struct DarkWorldBossConfiguration
{
    public required string Name { get; init; }
    public required Type DarkWorldBossType { get; init; }
}

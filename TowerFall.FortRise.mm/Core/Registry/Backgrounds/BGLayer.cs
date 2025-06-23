#nullable enable
using System.Collections.Generic;

namespace FortRise;

public readonly struct BGLayer
{
    public required string Name { get; init; }
    public Dictionary<string, object>? Data { get; init; }
    public string? SingleChildren { get; init; }
    public BGLayer[]? Childrens { get; init; }
}

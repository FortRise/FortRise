#nullable enable
using System;

namespace FortRise;

public readonly struct CommandConfiguration 
{
    public required Action<string[]> Callback { get; init; }
}

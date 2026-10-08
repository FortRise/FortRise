#nullable enable
namespace FortRise;

internal sealed class ModFlags(
    bool isWindows,
    bool isSteam
) : IModFlags
{
    public bool IsWindows { get; init; } = isWindows;
    public bool IsSteam { get; init; } = isSteam;
}


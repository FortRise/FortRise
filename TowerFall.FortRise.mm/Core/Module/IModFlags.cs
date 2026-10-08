#nullable enable
namespace FortRise;

public interface IModFlags
{
    bool IsWindows { get; init; }
    bool IsSteam { get; init; }
}


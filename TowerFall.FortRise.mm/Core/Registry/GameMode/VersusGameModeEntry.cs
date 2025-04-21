using TowerFall;

namespace FortRise;

internal class VersusGameModeEntry : IVersusGameModeEntry
{
    public string Name { get; init; }

    public IVersusGameMode VersusGameMode { get; init; }

    public Modes Modes { get; init; }

    public VersusGameModeEntry(string name, Modes modes, IVersusGameMode versusGameMode)
    {
        Name = name;
        Modes = modes;
        VersusGameMode = versusGameMode;
    }
}
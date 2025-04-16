namespace FortRise;

internal class VersusGameModeEntry : IVersusGameModeEntry
{
    public string Name { get; init; }

    public IVersusGameMode GameMode { get; init; }

    public VersusGameModeEntry(string name, IVersusGameMode gameMode)
    {
        Name = name;
        GameMode = gameMode;
    }
}
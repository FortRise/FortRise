using TowerFall;

namespace FortRise;

internal sealed class CoOpGameModeEntry : ICoOpGameModeEntry
{
    public string Name { get; init; }
    public ICoOpGameMode GameMode { get; init; }
    public Modes Modes { get; init; }

    public CoOpGameModeEntry(string name, ICoOpGameMode coOpGameMode, Modes modes)
    {
        Name = name;
        GameMode = coOpGameMode;
        Modes = modes;
    }
}

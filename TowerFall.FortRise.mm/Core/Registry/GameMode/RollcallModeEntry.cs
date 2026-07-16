using TowerFall;

namespace FortRise;

public sealed class RollcallModeEntry : IRollcallModesEntry
{
    public string Name { get; init; }
    public MainMenu.RollcallModes RollcallMode { get; init; }

    public RollcallModeEntry(string name, MainMenu.RollcallModes rollcallMode)
    {
        Name = name;
        RollcallMode = rollcallMode;
    }
}


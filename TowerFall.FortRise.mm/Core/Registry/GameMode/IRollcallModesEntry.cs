using TowerFall;

namespace FortRise;

public interface IRollcallModesEntry
{
    public string Name { get; init; }
    public MainMenu.RollcallModes RollcallMode { get; init; }
}


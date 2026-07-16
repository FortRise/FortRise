#nullable enable
using TowerFall;

namespace FortRise;

public interface ICoOpGameMode
{
    string Name { get; }
    MainMenu.RollcallModes RollcallMode { get; }

    RoundLogic CreateRoundLogic(Session session);
    CoOpButtonDisplay CreateButtonDisplay(MainMenu Menu);
}


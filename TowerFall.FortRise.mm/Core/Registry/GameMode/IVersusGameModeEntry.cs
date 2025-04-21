using TowerFall;

namespace FortRise;

public interface IVersusGameModeEntry 
{
    string Name { get; }
    IVersusGameMode VersusGameMode { get; }
    Modes Modes { get; }
}

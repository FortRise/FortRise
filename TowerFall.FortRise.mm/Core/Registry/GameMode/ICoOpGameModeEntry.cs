using TowerFall;

namespace FortRise;

public interface ICoOpGameModeEntry 
{
    public string Name { get; init; }
    public ICoOpGameMode GameMode { get; init; }
    public Modes Modes { get; init; }
}

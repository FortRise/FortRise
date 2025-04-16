namespace FortRise;

public interface IVersusGameModeEntry 
{
    string Name { get; }
    IVersusGameMode GameMode { get; }
}

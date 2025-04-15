#nullable enable
namespace FortRise;

public interface IModRegistry
{
    ModArrows Arrows { get; }   
    ModPickups Pickups { get; }
    ModVariants Variants { get; }
    ModCommands Commands { get; }
    ModEnemies Enemies { get; }
    ModGameModes GameModes { get; }
}

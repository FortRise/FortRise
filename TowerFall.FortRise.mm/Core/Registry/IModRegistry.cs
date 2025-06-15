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
    ModBackdrops Backdrops { get; }
    ModMenuStates MenuStates { get; }
    ModDarkWorldBosses DarkWorldBosses { get; }

    ModTilesets Tilesets { get; }
    ModTowers Towers { get; }
    ModTowerHooks TowerHooks { get; }
    ModQuestEvents QuestEvents { get; }
    ModThemes Themes { get; }
}

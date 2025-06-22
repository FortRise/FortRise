#nullable enable
namespace FortRise;

public interface IModRegistry
{
    ModSubtextures Subtextures { get; }
    ModSprites Sprites { get; }
    ModSFXs SFXs { get; }
    ModMusics Musics { get; }
    ModCharacterSounds CharacterSounds { get; }
    ModArchers Archers { get; }
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

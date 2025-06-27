#nullable enable
namespace FortRise;

public interface IModRegistry
{
    IModSubtextures Subtextures { get; }
    IModSprites Sprites { get; }
    IModSFXs SFXs { get; }
    IModMusics Musics { get; }
    IModCharacterSounds CharacterSounds { get; }
    IModArchers Archers { get; }
    IModArrows Arrows { get; }   
    IModPickups Pickups { get; }
    IModVariants Variants { get; }
    IModEffects Effects { get; }
    IModEnemies Enemies { get; }
    IModGameModes GameModes { get; }
    IModBackdrops Backdrops { get; }
    IModMenuStates MenuStates { get; }
    IModDarkWorldBosses DarkWorldBosses { get; }

    IModTilesets Tilesets { get; }
    IModTowers Towers { get; }
    IModTowerHooks TowerHooks { get; }
    IModQuestEvents QuestEvents { get; }
    IModThemes Themes { get; }
    IModCommands Commands { get; }
}

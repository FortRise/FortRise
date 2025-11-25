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
    IModLevelEntities LevelEntities { get; }
    IModGameModes GameModes { get; }
    IModBGElements BGElements { get; }
    IModBackgrounds Backgrounds { get; }
    IModMenuStates MenuStates { get; }
    IModDarkWorldBosses DarkWorldBosses { get; }

    IModTilesets Tilesets { get; }
    IModTowers Towers { get; }
    IModTowerHooks TowerHooks { get; }
    IModQuestEvents QuestEvents { get; }
    IModThemes Themes { get; }
    IModMapRenderers MapRenderers  { get; }
    IModCommands Commands { get; }
}

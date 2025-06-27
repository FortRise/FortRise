#nullable enable
namespace FortRise;

internal class ModRegistry : IModRegistry
{
    public IModSubtextures Subtextures { get; }
    public IModSprites Sprites { get; }
    public IModSFXs SFXs { get; }
    public IModMusics Musics { get; }
    public IModCharacterSounds CharacterSounds { get; }
    public IModArchers Archers { get; }
    public IModArrows Arrows { get; }   
    public IModPickups Pickups { get; }   
    public IModVariants Variants { get; }
    public IModEffects Effects { get; }
    public IModEnemies Enemies { get; }
    public IModGameModes GameModes { get; }
    public IModBackdrops Backdrops { get; }
    public IModBackgrounds Backgrounds { get; }
    public IModMenuStates MenuStates { get; }
    public IModDarkWorldBosses DarkWorldBosses { get; }
    // Pre-loaded Content
    public IModTilesets Tilesets { get; }
    public IModThemes Themes { get; }

    // Loaded Content
    public IModTowers Towers { get; }

    // Post-loaded Content
    public IModTowerHooks TowerHooks { get; }
    public IModQuestEvents QuestEvents { get; }
    public IModCommands Commands { get; }


    internal ModRegistry(ModuleMetadata metadata, ModuleManager manager)
    {
        Subtextures = new ModSubtextures(metadata, manager);
        Sprites = new ModSprites(metadata, manager);
        SFXs = new ModSFXs(metadata, manager);
        Musics = new ModMusics(metadata, manager);
        CharacterSounds = new ModCharacterSounds(metadata, manager);
        Archers = new ModArchers(metadata, manager);
        Arrows = new ModArrows(metadata, manager);
        Pickups = new ModPickups(metadata, manager);
        Variants = new ModVariants(metadata, manager);
        Effects = new ModEffects(metadata, manager);
        Enemies = new ModEnemies(metadata, manager);
        GameModes = new ModGameModes(metadata, manager);
        Backdrops = new ModBackdrops(metadata, manager);
        Backgrounds = new ModBackgrounds(metadata, manager);
        MenuStates = new ModMenuStates(metadata, manager);

        DarkWorldBosses = new ModDarkWorldBosses(metadata, manager);

        Tilesets = new ModTilesets(metadata, manager);
        Themes = new ModThemes(metadata, manager);
        Towers = new ModTowers(metadata, manager);

        TowerHooks = new ModTowerHooks(metadata, manager);
        QuestEvents = new ModQuestEvents(metadata, manager);
        Commands = new ModCommands(metadata, manager);
    }
}

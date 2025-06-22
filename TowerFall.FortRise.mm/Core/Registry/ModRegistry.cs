#nullable enable
namespace FortRise;

internal class ModRegistry : IModRegistry
{
    public ModSubtextures Subtextures { get; }
    public ModSprites Sprites { get; }
    public ModSFXs SFXs { get; }
    public ModMusics Musics { get; }
    public ModCharacterSounds CharacterSounds { get; }
    public ModArchers Archers { get; }
    public ModArrows Arrows { get; }   
    public ModPickups Pickups { get; }   
    public ModVariants Variants { get; }
    public ModCommands Commands { get; }
    public ModEnemies Enemies { get; }
    public ModGameModes GameModes { get; }
    public ModBackdrops Backdrops { get; }
    public ModMenuStates MenuStates { get; }
    public ModDarkWorldBosses DarkWorldBosses { get; }
    // Pre-loaded Content
    public ModTilesets Tilesets { get; }
    public ModThemes Themes { get; }

    // Loaded Content
    public ModTowers Towers { get; }

    // Post-loaded Content
    public ModTowerHooks TowerHooks { get; }
    public ModQuestEvents QuestEvents { get; }


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
        Commands = new ModCommands(metadata, manager);
        Enemies = new ModEnemies(metadata, manager);
        GameModes = new ModGameModes(metadata, manager);
        Backdrops = new ModBackdrops(metadata, manager);
        MenuStates = new ModMenuStates(metadata, manager);

        DarkWorldBosses = new ModDarkWorldBosses(metadata, manager);

        Tilesets = new ModTilesets(metadata, manager);
        Themes = new ModThemes(metadata, manager);
        Towers = new ModTowers(metadata, manager);

        TowerHooks = new ModTowerHooks(metadata, manager);
        QuestEvents = new ModQuestEvents(metadata, manager);
    }
}

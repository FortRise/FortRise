#nullable enable
namespace FortRise;

internal class ModRegistry : IModRegistry
{
    public ModArrows Arrows { get; }   
    public ModPickups Pickups { get; }   
    public ModVariants Variants { get; }
    public ModCommands Commands { get; }
    public ModEnemies Enemies { get; }
    public ModGameModes GameModes { get; }
    public ModBackdrops Backdrops { get; }
    public ModMenuStates MenuStates { get; }

    internal ModRegistry(ModuleMetadata metadata, ModuleManager manager)
    {
        Arrows = new ModArrows(metadata, manager);
        Pickups = new ModPickups(metadata, manager);
        Variants = new ModVariants(metadata, manager);
        Commands = new ModCommands(metadata, manager);
        Enemies = new ModEnemies(metadata, manager);
        GameModes = new ModGameModes(metadata, manager);
        Backdrops = new ModBackdrops(metadata, manager);
        MenuStates = new ModMenuStates(metadata, manager);
    }
}

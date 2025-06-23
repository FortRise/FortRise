using TowerFall;

namespace FortRise;
#nullable enable
internal sealed class ModEventsManager
{
    public SafeModEventHandler<ModuleMetadata> OnModInitialize;
    public SafeModEventHandler<LoadState> OnModLoadStateFinished;
    public SafeModEventHandler<RoundLogic> OnLevelLoaded;

    public ModEventsManager()
    {
        OnModInitialize = new();
        OnModLoadStateFinished = new();
        OnLevelLoaded = new();
    }
}

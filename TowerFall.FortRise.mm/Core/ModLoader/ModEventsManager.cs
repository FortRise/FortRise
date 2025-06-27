using TowerFall;

namespace FortRise;
#nullable enable
internal sealed class ModEventsManager
{
    public SafeModEventHandler<ModuleMetadata> OnModInitialize;
    public SafeModEventHandler<BeforeModInstantiationEventArgs> OnBeforeModInstantiation;
    public SafeModEventHandler<LoadState> OnModLoadStateFinished;
    public SafeModEventHandler<RoundLogic> OnLevelLoaded;

    public ModEventsManager()
    {
        OnModInitialize = new();
        OnBeforeModInstantiation = new();
        OnModLoadStateFinished = new();
        OnLevelLoaded = new();
    }
}

public class BeforeModInstantiationEventArgs(IModContent content, IModuleContext context)
{
    public IModContent ModContent { get; private set; } = content;
    public IModuleContext Context { get; private set; } = context;
}

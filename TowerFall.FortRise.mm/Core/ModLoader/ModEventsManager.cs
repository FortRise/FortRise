using System.Collections.Generic;
using TowerFall;

namespace FortRise;
#nullable enable
internal sealed class ModEventsManager
{
    internal static ModEventsManager Instance { get; private set; } = null!;
    public SafeModEventHandler<ModuleMetadata> OnModInitialize;
    public SafeModEventHandler<BeforeModInstantiationEventArgs> OnBeforeModInstantiation;
    public SafeModEventHandler<LoadState> OnModLoadStateFinished;
    public SafeModEventHandler<RoundLogic> OnLevelLoaded;
    public SafeModEventHandler<SlotVariantCreatedEventArgs> OnSlotVariantCreated;
    public SafeModEventHandler<MenuLoadedEventArgs> OnMenuLoaded;
    public SafeModEventHandler<Level> OnLevelExited;
    public SafeModEventHandler<TFGame> OnGameInitialized;

    public ModEventsManager()
    {
        Instance = this;
        OnModInitialize = new();
        OnBeforeModInstantiation = new();
        OnModLoadStateFinished = new();
        OnLevelLoaded = new();
        OnSlotVariantCreated = new();
        OnMenuLoaded = new();
        OnLevelExited = new();
        OnGameInitialized = new();
    }

    public void Dispose() 
    {
        OnModInitialize.RemoveAll();
        OnBeforeModInstantiation.RemoveAll();
        OnModLoadStateFinished.RemoveAll();
        OnLevelLoaded.RemoveAll();
        OnSlotVariantCreated.RemoveAll();
        OnMenuLoaded.RemoveAll();
        OnLevelExited.RemoveAll();
        OnGameInitialized.RemoveAll();
    }
}

public record MenuLoadedEventArgs(MainMenu Menu, bool NewDataCreated);
public record BeforeModInstantiationEventArgs(IModContent ModContent, IModuleContext Context);
public record SlotVariantCreatedEventArgs(MatchVariants MatchVariants, List<List<VariantItem>> VariantSlots);


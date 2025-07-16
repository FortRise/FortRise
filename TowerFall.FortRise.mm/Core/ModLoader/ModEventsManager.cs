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

    public ModEventsManager()
    {
        Instance = this;
        OnModInitialize = new();
        OnBeforeModInstantiation = new();
        OnModLoadStateFinished = new();
        OnLevelLoaded = new();
        OnSlotVariantCreated = new();
        OnMenuLoaded = new();
    }
}

public record MenuLoadedEventArgs(MainMenu Menu, bool NewDataCreated);
public record BeforeModInstantiationEventArgs(IModContent ModContent, IModuleContext Context);
public record SlotVariantCreatedEventArgs(MatchVariants MatchVariants, List<List<VariantItem>> VariantSlots);


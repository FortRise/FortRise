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
    public SafeModEventHandler<OnDataLoadEventArgs> OnBeforeDataLoad;
    public SafeModEventHandler<OnDataLoadEventArgs> OnAfterDataLoad;
    public SafeModEventHandler<OnSessionQuitEventArgs> OnSessionQuit;

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
        OnBeforeDataLoad = new();
        OnAfterDataLoad = new();
        OnSessionQuit = new();
    }

    public void RemoveByMod(Mod mod)
    {
        OnModInitialize.RemoveAllWithMetadata(mod.Meta);
        OnBeforeModInstantiation.RemoveAllWithMetadata(mod.Meta);
        OnModLoadStateFinished.RemoveAllWithMetadata(mod.Meta);
        OnLevelLoaded.RemoveAllWithMetadata(mod.Meta);
        OnSlotVariantCreated.RemoveAllWithMetadata(mod.Meta);
        OnMenuLoaded.RemoveAllWithMetadata(mod.Meta);
        OnLevelExited.RemoveAllWithMetadata(mod.Meta);
        OnGameInitialized.RemoveAllWithMetadata(mod.Meta);     
        OnBeforeDataLoad.RemoveAllWithMetadata(mod.Meta);
        OnAfterDataLoad.RemoveAllWithMetadata(mod.Meta);
        OnSessionQuit.RemoveAllWithMetadata(mod.Meta);
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
        OnBeforeDataLoad.RemoveAll();
        OnAfterDataLoad.RemoveAll();
        OnSessionQuit.RemoveAll();
    }
}

public record MenuLoadedEventArgs(MainMenu Menu, bool NewDataCreated);
public record BeforeModInstantiationEventArgs(IModContent ModContent, IModuleContext Context);
public record SlotVariantCreatedEventArgs(MatchVariants MatchVariants, List<List<VariantItem>> VariantSlots);
public record OnDataLoadEventArgs(bool WillRestart);
public record OnSessionQuitEventArgs(Session Session, PauseMenu.MenuType PauseMenuType);


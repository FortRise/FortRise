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
    public SafeModEventHandler<DataLoadEventArgs> OnBeforeDataLoad;
    public SafeModEventHandler<DataLoadEventArgs> OnAfterDataLoad;
    public SafeModEventHandler<SessionQuitEventArgs> OnSessionQuit;
    public SafeModEventHandler<LevelSetsCreatedEventArgs> OnLevelSetsCreated;
    public SafeModEventHandler<BeforeSaveSaveDataEventArgs> OnBeforeSaveSaveData;
    public SafeModEventHandler<AfterSaveSaveDataEventArgs> OnAfterSaveSaveData;

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
        OnLevelSetsCreated = new();
        OnBeforeSaveSaveData = new();
        OnAfterSaveSaveData = new();
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
        OnLevelSetsCreated.RemoveAllWithMetadata(mod.Meta);
        OnBeforeSaveSaveData.RemoveAllWithMetadata(mod.Meta);
        OnAfterSaveSaveData.RemoveAllWithMetadata(mod.Meta);
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
        OnLevelSetsCreated.RemoveAll();
        OnBeforeSaveSaveData.RemoveAll();
        OnAfterSaveSaveData.RemoveAll();
    }
}

public record MenuLoadedEventArgs(MainMenu Menu, bool NewDataCreated);
public record BeforeModInstantiationEventArgs(IModContent ModContent, IModuleContext Context);
public record SlotVariantCreatedEventArgs(MatchVariants MatchVariants, List<List<VariantItem>> VariantSlots);
public record DataLoadEventArgs(bool WillRestart);
public record SessionQuitEventArgs(Session Session, PauseMenu.MenuType PauseMenuType);
public record LevelSetsCreatedEventArgs(
    MapScene Map, 
    MainMenu.RollcallModes RollcallModes, 
    List<string> LevelSets
);

public record BeforeSaveSaveDataEventArgs();
public record AfterSaveSaveDataEventArgs(string Result);

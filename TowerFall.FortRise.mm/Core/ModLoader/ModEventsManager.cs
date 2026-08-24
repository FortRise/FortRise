using System.Collections.Generic;
using TowerFall;

namespace FortRise;

#nullable enable
internal sealed class ModEventsManager
{
    internal static ModEventsManager Instance { get; private set; } = null!;
    public SafeModEventHandler<ModuleMetadata> ModInitialize;
    public SafeModEventHandler<BeforeModInstantiationEventArgs> ModBeforeInstantiation;
    public SafeModEventHandler<LoadState> ModLoadStateFinished;
    public SafeModEventHandler<RoundLogic> RoundLogicLevelLoadFinish;
    public SafeModEventHandler<SlotVariantCreatedEventArgs> MatchVariantsSlotVariantCreated;
    public SafeModEventHandler<MenuLoadedEventArgs> GameLoaded;
    public SafeModEventHandler<Level> LevelEntered;
    public SafeModEventHandler<Level> LevelExited;
    public SafeModEventHandler<TFGame> GameInitialized;
    public SafeModEventHandler<DataLoadEventArgs> OnBeforeDataLoad;
    public SafeModEventHandler<DataLoadEventArgs> OnAfterDataLoad;
    public SafeModEventHandler<SessionQuitEventArgs> SessionQuit;
    public SafeModEventHandler<LevelSetsCreatedEventArgs> MapSceneLevelSetsCreated;
    public SafeModEventHandler<BeforeSaveSaveDataEventArgs> BeforeSaveData;
    public SafeModEventHandler<AfterSaveSaveDataEventArgs> AfterSaveData;

    public ModEventsManager()
    {
        Instance = this;
        ModInitialize = new();
        ModBeforeInstantiation = new();
        ModLoadStateFinished = new();
        RoundLogicLevelLoadFinish = new();
        MatchVariantsSlotVariantCreated = new();
        GameLoaded = new();
        LevelEntered = new();
        LevelExited = new();
        GameInitialized = new();
        OnBeforeDataLoad = new();
        OnAfterDataLoad = new();
        SessionQuit = new();
        MapSceneLevelSetsCreated = new();
        BeforeSaveData = new();
        AfterSaveData = new();
    }

    public void RemoveByMod(Mod mod)
    {
        ModInitialize.RemoveAllWithMetadata(mod.Meta);
        ModBeforeInstantiation.RemoveAllWithMetadata(mod.Meta);
        ModLoadStateFinished.RemoveAllWithMetadata(mod.Meta);
        RoundLogicLevelLoadFinish.RemoveAllWithMetadata(mod.Meta);
        MatchVariantsSlotVariantCreated.RemoveAllWithMetadata(mod.Meta);
        GameLoaded.RemoveAllWithMetadata(mod.Meta);
        LevelEntered.RemoveAllWithMetadata(mod.Meta);
        LevelExited.RemoveAllWithMetadata(mod.Meta);
        GameInitialized.RemoveAllWithMetadata(mod.Meta);     
        OnBeforeDataLoad.RemoveAllWithMetadata(mod.Meta);
        OnAfterDataLoad.RemoveAllWithMetadata(mod.Meta);
        SessionQuit.RemoveAllWithMetadata(mod.Meta);
        MapSceneLevelSetsCreated.RemoveAllWithMetadata(mod.Meta);
        BeforeSaveData.RemoveAllWithMetadata(mod.Meta);
        AfterSaveData.RemoveAllWithMetadata(mod.Meta);
    }

    public void Dispose() 
    {
        ModInitialize.RemoveAll();
        ModBeforeInstantiation.RemoveAll();
        ModLoadStateFinished.RemoveAll();
        RoundLogicLevelLoadFinish.RemoveAll();
        MatchVariantsSlotVariantCreated.RemoveAll();
        GameLoaded.RemoveAll();
        LevelEntered.RemoveAll();
        LevelExited.RemoveAll();
        GameInitialized.RemoveAll();
        OnBeforeDataLoad.RemoveAll();
        OnAfterDataLoad.RemoveAll();
        SessionQuit.RemoveAll();
        MapSceneLevelSetsCreated.RemoveAll();
        BeforeSaveData.RemoveAll();
        AfterSaveData.RemoveAll();
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

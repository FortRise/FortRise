using System;
using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise;

#nullable enable
internal sealed class ModEventsManager
{
    internal static ModEventsManager Instance { get; private set; } = null!;
    public SafeModEventHandler<RequestingSubtextureEventArgs> RequestingSubtexture;
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
    public SafeModEventHandler<ArrowCreatedEventArgs> ArrowCreated;
    public SafeModEventHandler<ArrowHitTargetEventArgs> ArrowHitTarget;
    public SafeModEventHandler<ArrowStuckEventArgs> ArrowStuck;
    public SafeModEventHandler<ArrowBuriedEventArgs> ArrowBuried;
    public SafeModEventHandler<Arrow> ArrowStartedFalling;
    public SafeModEventHandler<ArrowCaughtEventArgs> ArrowCaught;
    public SafeModEventHandler<ArrowCollectedEventArgs> ArrowCollected;
    public SafeModEventHandler<ArrowHitLavaEventArgs> ArrowTouchedLava;
    public SafeModEventHandler<Arrow> ArrowRemoved;

    public ModEventsManager()
    {
        Instance = this;
        RequestingSubtexture = new();
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
        ArrowCreated = new();
        ArrowHitTarget = new();
        ArrowStuck = new();
        ArrowBuried = new();
        ArrowStartedFalling = new();
        ArrowCaught = new();
        ArrowCollected = new();
        ArrowTouchedLava = new();
        ArrowRemoved = new();
    }

    public void RemoveByMetadata(ModuleMetadata metadata)
    {
        RequestingSubtexture.RemoveAllWithMetadata(metadata);
        ModInitialize.RemoveAllWithMetadata(metadata);
        ModBeforeInstantiation.RemoveAllWithMetadata(metadata);
        ModLoadStateFinished.RemoveAllWithMetadata(metadata);
        RoundLogicLevelLoadFinish.RemoveAllWithMetadata(metadata);
        MatchVariantsSlotVariantCreated.RemoveAllWithMetadata(metadata);
        GameLoaded.RemoveAllWithMetadata(metadata);
        LevelEntered.RemoveAllWithMetadata(metadata);
        LevelExited.RemoveAllWithMetadata(metadata);
        GameInitialized.RemoveAllWithMetadata(metadata);
        OnBeforeDataLoad.RemoveAllWithMetadata(metadata);
        OnAfterDataLoad.RemoveAllWithMetadata(metadata);
        SessionQuit.RemoveAllWithMetadata(metadata);
        MapSceneLevelSetsCreated.RemoveAllWithMetadata(metadata);
        BeforeSaveData.RemoveAllWithMetadata(metadata);
        AfterSaveData.RemoveAllWithMetadata(metadata);
        ArrowCreated.RemoveAllWithMetadata(metadata);
        ArrowHitTarget.RemoveAllWithMetadata(metadata);
        ArrowStuck.RemoveAllWithMetadata(metadata);
        ArrowBuried.RemoveAllWithMetadata(metadata);
        ArrowStartedFalling.RemoveAllWithMetadata(metadata);
        ArrowCaught.RemoveAllWithMetadata(metadata);
        ArrowCollected.RemoveAllWithMetadata(metadata);
        ArrowTouchedLava.RemoveAllWithMetadata(metadata);
        ArrowRemoved.RemoveAllWithMetadata(metadata);
    }

    public void Dispose()
    {
        RequestingSubtexture.RemoveAll();
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
        ArrowCreated.RemoveAll();
        ArrowHitTarget.RemoveAll();
        ArrowStuck.RemoveAll();
        ArrowBuried.RemoveAll();
        ArrowStartedFalling.RemoveAll();
        ArrowCaught.RemoveAll();
        ArrowCollected.RemoveAll();
        ArrowTouchedLava.RemoveAll();
        ArrowRemoved.RemoveAll();
    }
}

public record RequestingSubtextureEventArgs(string Name)
{
    private Func<Subtexture>? replacementCallback;

    public void Edit(Func<Subtexture> editSubtextureCallback)
    {
       replacementCallback = editSubtextureCallback;
    }

    internal Subtexture? ApplyEdit()
    {
        return replacementCallback?.Invoke();
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

public record ArrowCreatedEventArgs(Arrow Arrow, LevelEntity Owner);
public record ArrowHitTargetEventArgs(Arrow Arrow, LevelEntity Target);
public record ArrowStuckEventArgs(Arrow Arrow, Platform Platform);
public record ArrowBuriedEventArgs(Arrow Arrow, ArrowCushion Cushion);
public record ArrowCaughtEventArgs(Arrow Arrow, Player Player);
public record ArrowCollectedEventArgs(Arrow Arrow, Player Player);
public record ArrowHitLavaEventArgs(Arrow Arrow, Lava Lava);

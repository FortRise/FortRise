using System;
using TowerFall;

namespace FortRise;


internal partial class ModEvents 
{
    public event EventHandler<AfterSaveSaveDataEventArgs> OnAfterSaveSaveData
    {
        add => manager.AfterSaveData.Add(metadata, value);
        remove => manager.AfterSaveData.Remove(metadata, value);
    }

    public event EventHandler<BeforeSaveSaveDataEventArgs> OnBeforeSaveSaveData
    {
        add => manager.BeforeSaveData.Add(metadata, value);
        remove => manager.BeforeSaveData.Remove(metadata, value);
    }

    public event EventHandler<ModuleMetadata> OnModInitialize
    {
        add => manager.ModInitialize.Add(metadata, value);
        remove => manager.ModInitialize.Remove(metadata, value);
    }

    public event EventHandler<BeforeModInstantiationEventArgs> OnBeforeModInstantiation
    {
        add => manager.ModBeforeInstantiation.Add(metadata, value);
        remove => manager.ModBeforeInstantiation.Remove(metadata, value);
    }

    public event EventHandler<LoadState> OnModLoadStateFinished
    {
        add => manager.ModLoadStateFinished.Add(metadata, value);
        remove => manager.ModLoadStateFinished.Remove(metadata, value);
    }

    public event EventHandler<RoundLogic> OnLevelLoaded
    {
        add => manager.RoundLogicLevelLoadFinish.Add(metadata, value);
        remove => manager.RoundLogicLevelLoadFinish.Remove(metadata, value);
    }

    public event EventHandler<SlotVariantCreatedEventArgs> OnSlotVariantCreated
    {
        add => manager.MatchVariantsSlotVariantCreated.Add(metadata, value);
        remove => manager.MatchVariantsSlotVariantCreated.Remove(metadata, value);
    }

    public event EventHandler<MenuLoadedEventArgs> OnMenuLoaded
    {
        add => manager.GameLoaded.Add(metadata, value);
        remove => manager.GameLoaded.Remove(metadata, value);
    }

    public event EventHandler<Level> OnLevelExited 
    {
        add => manager.LevelExited.Add(metadata, value);
        remove => manager.LevelExited.Remove(metadata, value);
    }

    public event EventHandler<TFGame> OnGameInitialized
    {
        add => manager.GameInitialized.Add(metadata, value);
        remove => manager.GameInitialized.Remove(metadata, value);
    }

    public event EventHandler<DataLoadEventArgs> OnBeforeDataLoad
    {
        add => manager.OnBeforeDataLoad.Add(metadata, value);
        remove => manager.OnBeforeDataLoad.Remove(metadata, value);
    }

    public event EventHandler<DataLoadEventArgs> OnAfterDataLoad
    {
        add => manager.OnAfterDataLoad.Add(metadata, value);
        remove => manager.OnAfterDataLoad.Remove(metadata, value);
    }

    public event EventHandler<SessionQuitEventArgs> OnSessionQuit
    {
        add => manager.SessionQuit.Add(metadata, value);
        remove => manager.SessionQuit.Remove(metadata, value);
    }
    public event EventHandler<LevelSetsCreatedEventArgs> OnLevelSetsCreated
    {
        add => manager.MapSceneLevelSetsCreated.Add(metadata, value);
        remove => manager.MapSceneLevelSetsCreated.Remove(metadata, value);
    }
}


public partial interface IModEvents
{
    [Obsolete("Use Mods.Initialize")]
    event EventHandler<ModuleMetadata> OnModInitialize;
    [Obsolete("Use Mods.BeforeInstantiation")]
    event EventHandler<BeforeModInstantiationEventArgs> OnBeforeModInstantiation;
    [Obsolete("Use Mods.LoadStateFinished")]
    event EventHandler<LoadState> OnModLoadStateFinished;

    [Obsolete("Use RoundLogic.LevelLoadFinish")]
    event EventHandler<RoundLogic> OnLevelLoaded;
    [Obsolete("Use MatchVariants.SlotVariantCreated")]
    event EventHandler<SlotVariantCreatedEventArgs> OnSlotVariantCreated;

    [Obsolete("Use Game.GameLoaded")]
    event EventHandler<MenuLoadedEventArgs> OnMenuLoaded;

    [Obsolete("Use Level.LevelExited")]
    event EventHandler<Level> OnLevelExited;

    [Obsolete("Use Game.GameLoaded")]
    event EventHandler<TFGame> OnGameInitialized;

    [Obsolete("Use GameData.BeforeLoad")]
    event EventHandler<DataLoadEventArgs> OnBeforeDataLoad;

    [Obsolete("Use GameData.AfterLoad")]
    event EventHandler<DataLoadEventArgs> OnAfterDataLoad;

    [Obsolete("Use Session.Quit")]
    event EventHandler<SessionQuitEventArgs> OnSessionQuit;

    [Obsolete("Use MapScene.LevelSetsCreated")]
    event EventHandler<LevelSetsCreatedEventArgs> OnLevelSetsCreated;

    [Obsolete("Use SaveData.BeforeSave")]
    event EventHandler<BeforeSaveSaveDataEventArgs> OnBeforeSaveSaveData;

    [Obsolete("Use SaveData.AfterSave")]
    event EventHandler<AfterSaveSaveDataEventArgs> OnAfterSaveSaveData;
}

#nullable enable
using System;
using TowerFall;

namespace FortRise;

internal sealed partial class ModEvents(ModuleMetadata metadata, ModEventsManager manager) : IModEvents
{
    public sealed class FortRiseModEvents(ModuleMetadata metadata, ModEventsManager manager) : IModEvents.IFortRiseModEvents
    {
        public event EventHandler<ModuleMetadata> Initialize 
        {
            add => manager.ModInitialize.Add(metadata, value);
            remove => manager.ModInitialize.Remove(metadata, value);
        }

        public event EventHandler<BeforeModInstantiationEventArgs> BeforeInstantiation 
        {
            add => manager.ModBeforeInstantiation.Add(metadata, value);
            remove => manager.ModBeforeInstantiation.Remove(metadata, value);
        }
        public event EventHandler<LoadState> LoadStateFinished 
        {
            add => manager.ModLoadStateFinished.Add(metadata, value);
            remove => manager.ModLoadStateFinished.Remove(metadata, value);
        }
    }

    public sealed class MatchVariantEvents(ModuleMetadata metadata, ModEventsManager manager) : IModEvents.IMatchVariantsEvents
    {
        public event EventHandler<SlotVariantCreatedEventArgs> SlotVariantCreated 
        {
            add => manager.MatchVariantsSlotVariantCreated.Add(metadata, value);
            remove => manager.MatchVariantsSlotVariantCreated.Remove(metadata, value);
        }
    }

    public sealed class RoundLogicEvents(ModuleMetadata metadata, ModEventsManager manager) : IModEvents.IRoundLogicEvents
    {
        public event EventHandler<RoundLogic> LevelLoadFinish 
        {
            add => manager.RoundLogicLevelLoadFinish.Add(metadata, value);
            remove => manager.RoundLogicLevelLoadFinish.Remove(metadata, value);           
        }
    }

    public sealed class LevelEvents(ModuleMetadata metadata, ModEventsManager manager) : IModEvents.ILevelEvents
    {
        public event EventHandler<Level> LevelEntered
        {
            add => manager.LevelEntered.Add(metadata, value);
            remove => manager.LevelEntered.Remove(metadata, value);           
        }

        public event EventHandler<Level> LevelExited
        {
            add => manager.LevelExited.Add(metadata, value);
            remove => manager.LevelExited.Remove(metadata, value);           
        }
    }

    public sealed class GameEvents(ModuleMetadata metadata, ModEventsManager manager) : IModEvents.IGameEvents
    {
        public event EventHandler<TFGame> GameInitialized
        {
            add => manager.GameInitialized.Add(metadata, value);
            remove => manager.GameInitialized.Remove(metadata, value);           
        }

        public event EventHandler<MenuLoadedEventArgs> GameLoaded
        {
            add => manager.GameLoaded.Add(metadata, value);
            remove => manager.GameLoaded.Remove(metadata, value);           
        }
    }

    public sealed class MapSceneEvents(ModuleMetadata metadata, ModEventsManager manager) : IModEvents.IMapSceneEvents
    {
        public event EventHandler<LevelSetsCreatedEventArgs> LevelSetsCreated
        {
            add => manager.MapSceneLevelSetsCreated.Add(metadata, value);
            remove => manager.MapSceneLevelSetsCreated.Remove(metadata, value);           
        }
    }

    public sealed class SaveDataEvents(ModuleMetadata metadata, ModEventsManager manager) : IModEvents.ISaveDataEvents
    {
        public event EventHandler<BeforeSaveSaveDataEventArgs> BeforeSave
        {
            add => manager.BeforeSaveData.Add(metadata, value);
            remove => manager.BeforeSaveData.Remove(metadata, value);
        }

        public event EventHandler<AfterSaveSaveDataEventArgs> AfterSave
        {
            add => manager.AfterSaveData.Add(metadata, value);
            remove => manager.AfterSaveData.Remove(metadata, value);
        }
    }

    public sealed class SessionEvents(ModuleMetadata metadata, ModEventsManager manager) : IModEvents.ISessionEvents
    {
        public event EventHandler<SessionQuitEventArgs> Quit
        {
            add => manager.SessionQuit.Add(metadata, value);
            remove => manager.SessionQuit.Remove(metadata, value);
        }

    }

    public sealed class GameDataEvents(ModuleMetadata metadata, ModEventsManager manager) : IModEvents.IGameDataEvents
    {
        public event EventHandler<DataLoadEventArgs> BeforeLoad
        {
            add => manager.OnBeforeDataLoad.Add(metadata, value);
            remove => manager.OnBeforeDataLoad.Remove(metadata, value);
        }

        public event EventHandler<DataLoadEventArgs> AfterLoad
        {
            add => manager.OnAfterDataLoad.Add(metadata, value);
            remove => manager.OnAfterDataLoad.Remove(metadata, value);
        }
    }

    public IModEvents.IFortRiseModEvents Mods { get; } = new FortRiseModEvents(metadata, manager);
    public IModEvents.IMatchVariantsEvents MatchVariants { get; } = new MatchVariantEvents(metadata, manager);
    public IModEvents.IRoundLogicEvents RoundLogic { get; } = new RoundLogicEvents(metadata, manager);
    public IModEvents.ILevelEvents Level { get; } = new LevelEvents(metadata, manager);
    public IModEvents.IGameEvents Game { get; } = new GameEvents(metadata, manager);
    public IModEvents.IMapSceneEvents MapScene { get; } = new MapSceneEvents(metadata, manager);
    public IModEvents.ISaveDataEvents SaveData { get; } = new SaveDataEvents(metadata, manager);
    public IModEvents.ISessionEvents Session { get; } = new SessionEvents(metadata, manager);
    public IModEvents.IGameDataEvents GameData { get; } = new GameDataEvents(metadata, manager);
}

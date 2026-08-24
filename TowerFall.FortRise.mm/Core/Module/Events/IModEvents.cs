#nullable enable
using System;
using TowerFall;

namespace FortRise;

public partial interface IModEvents
{
    public interface IFortRiseModEvents 
    {
        event EventHandler<ModuleMetadata> Initialize;
        event EventHandler<BeforeModInstantiationEventArgs> BeforeInstantiation;
        event EventHandler<LoadState> LoadStateFinished;
    }

    public interface IMatchVariantsEvents 
    {
        /// <summary>
        /// Called after all presets and variants are created but not yet placed for modification.
        /// </summary>
        event EventHandler<SlotVariantCreatedEventArgs> SlotVariantCreated;
    }

    public interface IRoundLogicEvents 
    {
        event EventHandler<RoundLogic> LevelLoadFinish;
    }

    public interface ILevelEvents 
    {
        event EventHandler<Level> LevelEntered;
        event EventHandler<Level> LevelExited;
    }

    public interface IGameEvents
    {
        /// <summary>
        /// Called when the game is initialized. This is different from mod initialization and it is run before that happens.
        /// </summary>
        event EventHandler<TFGame> GameInitialized;

        /// <summary>
        /// Called after the game menu load is finished.
        /// </summary>
        event EventHandler<MenuLoadedEventArgs> GameLoaded;
    }

    public interface IMapSceneEvents 
    {
        /// <summary>
        /// Called after creating level sets before it being rendered. Usually used for modifying and filtering level sets.
        /// </summary>
        event EventHandler<LevelSetsCreatedEventArgs> LevelSetsCreated;
    }

    public interface ISaveDataEvents 
    {
        /// <summary>
        /// Called before saving a save data.
        /// </summary>
        event EventHandler<BeforeSaveSaveDataEventArgs> BeforeSave;

        /// <summary>
        /// Called after saving a save data.
        /// </summary>
        event EventHandler<AfterSaveSaveDataEventArgs> AfterSave;
    }

    public interface ISessionEvents 
    {
        /// <summary>
        /// Called after quiting a level within a session.
        /// </summary>
        event EventHandler<SessionQuitEventArgs> Quit;
    }

    public interface IGameDataEvents 
    {
        /// <summary>
        /// Called before the game data is loaded.
        /// </summary>
        event EventHandler<DataLoadEventArgs> BeforeLoad;

        /// <summary>
        /// Called after the game data is loaded, such as xml assets.
        /// </summary>
        event EventHandler<DataLoadEventArgs> AfterLoad;
    }

    IFortRiseModEvents Mods { get; }
    IMatchVariantsEvents MatchVariants { get; }
    IRoundLogicEvents RoundLogic { get; }
    ILevelEvents Level { get; }
    IGameEvents Game { get; }
    IMapSceneEvents MapScene { get; }
    ISaveDataEvents SaveData { get; }
    ISessionEvents Session { get; }
    IGameDataEvents GameData { get; }
}

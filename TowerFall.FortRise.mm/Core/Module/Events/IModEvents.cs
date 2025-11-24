#nullable enable
using System;
using TowerFall;

namespace FortRise;

public interface IModEvents
{
    event EventHandler<ModuleMetadata> OnModInitialize;
    event EventHandler<BeforeModInstantiationEventArgs> OnBeforeModInstantiation;
    event EventHandler<LoadState> OnModLoadStateFinished;
    event EventHandler<RoundLogic> OnLevelLoaded;
    /// <summary>
    /// Called after all presets and variants are created but not yet placed for modification.
    /// </summary>
    event EventHandler<SlotVariantCreatedEventArgs> OnSlotVariantCreated;
    /// <summary>
    /// Called after the game initialization state.
    /// </summary>
    event EventHandler<MenuLoadedEventArgs> OnMenuLoaded;
    event EventHandler<Level> OnLevelExited;
    /// <summary>
    /// Called when the game is initialized. This is different from mod initialization and it is run before that happens.
    /// </summary>
    event EventHandler<TFGame> OnGameInitialized;
    /// <summary>
    /// Called before the game data is loaded, such as xml assets.
    /// </summary>
    event EventHandler<DataLoadEventArgs> OnBeforeDataLoad;
    /// <summary>
    /// Called after the game data is loaded, such as xml assets.
    /// </summary>
    event EventHandler<DataLoadEventArgs> OnAfterDataLoad;
    /// <summary>
    /// Called after quiting a level within a session.
    /// </summary>
    event EventHandler<SessionQuitEventArgs> OnSessionQuit;
    /// <summary>
    /// Called after creating level sets before it being rendered. Usually used for modifying and filtering level sets.
    /// </summary>
    event EventHandler<LevelSetsCreatedEventArgs> OnLevelSetsCreated;
}

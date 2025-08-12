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
}

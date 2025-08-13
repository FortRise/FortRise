#nullable enable
using System;
using TowerFall;

namespace FortRise;

internal class ModEvents(ModuleMetadata metadata, ModEventsManager manager) : IModEvents
{
    public event EventHandler<ModuleMetadata> OnModInitialize
    {
        add => manager.OnModInitialize.Add(metadata, value);
        remove => manager.OnModInitialize.Remove(metadata, value);
    }

    public event EventHandler<BeforeModInstantiationEventArgs> OnBeforeModInstantiation
    {
        add => manager.OnBeforeModInstantiation.Add(metadata, value);
        remove => manager.OnBeforeModInstantiation.Remove(metadata, value);
    }

    public event EventHandler<LoadState> OnModLoadStateFinished
    {
        add => manager.OnModLoadStateFinished.Add(metadata, value);
        remove => manager.OnModLoadStateFinished.Remove(metadata, value);
    }

    public event EventHandler<RoundLogic> OnLevelLoaded
    {
        add => manager.OnLevelLoaded.Add(metadata, value);
        remove => manager.OnLevelLoaded.Remove(metadata, value);
    }

    public event EventHandler<SlotVariantCreatedEventArgs> OnSlotVariantCreated
    {
        add => manager.OnSlotVariantCreated.Add(metadata, value);
        remove => manager.OnSlotVariantCreated.Remove(metadata, value);
    }

    public event EventHandler<MenuLoadedEventArgs> OnMenuLoaded
    {
        add => manager.OnMenuLoaded.Add(metadata, value);
        remove => manager.OnMenuLoaded.Remove(metadata, value);
    }

    public event EventHandler<Level> OnLevelExited 
    {
        add => manager.OnLevelExited.Add(metadata, value);
        remove => manager.OnLevelExited.Remove(metadata, value);
    }

    public event EventHandler<TFGame> OnGameInitialized
    {
        add => manager.OnGameInitialized.Add(metadata, value);
        remove => manager.OnGameInitialized.Remove(metadata, value);
    }
}

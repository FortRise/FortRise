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
}
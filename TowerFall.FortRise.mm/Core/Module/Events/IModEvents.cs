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
}

#nullable enable
using System;
using TowerFall;

namespace FortRise;

public interface IModEvents
{
    public event EventHandler<ModuleMetadata> OnModInitialize;
    public event EventHandler<LoadState> OnModLoadStateFinished;
    public event EventHandler<RoundLogic> OnLevelLoaded;
}

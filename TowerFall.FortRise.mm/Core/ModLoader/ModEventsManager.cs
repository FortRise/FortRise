using System;

namespace FortRise;
#nullable enable
internal sealed class ModEventsManager
{
    public event EventHandler<ModuleMetadata>? OnModInitialize;
    public event EventHandler? OnModLoadingFinished;
    public event EventHandler? OnModInitializingFinished;

    public ModEventsManager() { }

    internal void OnModInitializeInvoke(ModuleMetadata moduleMetadata)
    {
        OnModInitialize?.Invoke(null, moduleMetadata);
    }

    internal void OnModLoadingFinishedInvoke()
    {
        OnModLoadingFinished?.Invoke(null, null!);
    }

    internal void OnModInitializingFinishedInvoke()
    {
        OnModInitializingFinished?.Invoke(null, null!);
    }
}
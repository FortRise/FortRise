using System;

namespace FortRise.ContentPatcher;

internal sealed class PatchHandle<T>(ModuleMetadata owner, T patch, string id, Action<PatchHandle<T>> unregister)
    : IFortRiseContentPatcherAPI.IPatchHandle
{
    public ModuleMetadata Owner { get; } = owner;
    public T Patch { get; } = patch;
    public string Id { get; } = id;
    public bool Enabled { get; set; } = true;

    public void Unregister() => unregister(this);
}

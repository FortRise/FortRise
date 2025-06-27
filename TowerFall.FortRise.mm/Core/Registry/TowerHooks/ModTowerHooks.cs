#nullable enable
using System.Collections.Generic;

namespace FortRise;

public interface IModTowerHooks
{
    ITowerHookEntry RegisterTowerHook(string id, ITowerHook hook);
    ITowerHookEntry? GetTowerHook(string id);
}

internal sealed class ModTowerHooks : IModTowerHooks
{
    private readonly ModuleMetadata metadata;

    internal ModTowerHooks(ModuleMetadata metadata)
    {
        this.metadata = metadata;
    }

    public ITowerHookEntry RegisterTowerHook(string id, ITowerHook hook)
    {
        string name = $"{metadata.Name}/{id}";
        ITowerHookEntry entry = new TowerHookEntry(name, hook);
        TowerPatchRegistry.Hooks.Add(entry.Name, entry);
        return entry;
    }

    public ITowerHookEntry? GetTowerHook(string id)
    {
        TowerPatchRegistry.Hooks.TryGetValue(id, out ITowerHookEntry? entry);
        return entry;
    }
}

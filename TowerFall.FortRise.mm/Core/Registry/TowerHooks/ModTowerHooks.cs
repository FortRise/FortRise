#nullable enable

namespace FortRise;

public interface IModTowerHooks
{
    ITowerHookEntry RegisterTowerHook(string id, ITowerHook hook);
    ITowerHookEntry? GetTowerHook(string id);
}

internal sealed class ModTowerHooks : IModTowerHooks
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<ITowerHookEntry> registryQueue;

    internal ModTowerHooks(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<ITowerHookEntry>(metadata, static _ => {}, Detach);
    }

    private void Detach(ITowerHookEntry entry)
    {
        TowerPatchRegistry.Hooks.Remove(entry.Name);
    }

    public ITowerHookEntry RegisterTowerHook(string id, ITowerHook hook)
    {
        string name = $"{metadata.Name}/{id}";
        ITowerHookEntry entry = new TowerHookEntry(name, hook);
        TowerPatchRegistry.Hooks.Add(entry.Name, entry);
        registryQueue.AddOrInvoke(entry);
        return entry;
    }

    public ITowerHookEntry? GetTowerHook(string id)
    {
        TowerPatchRegistry.Hooks.TryGetValue(id, out ITowerHookEntry? entry);
        return entry;
    }
}

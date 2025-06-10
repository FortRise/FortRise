#nullable enable
using System.Collections.Generic;

namespace FortRise;

public class ModTowerHooks
{
    private readonly Dictionary<string, ITowerHookEntry> entries = new Dictionary<string, ITowerHookEntry>();
    private readonly RegistryQueue<ITowerHookEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModTowerHooks(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<ITowerHookEntry>(Invoke);
    }

    public ITowerHookEntry RegisterTowerHook(string id, ITowerHook hook)
    {
        string name = $"{metadata.Name}/{id}";       
        ITowerHookEntry entry = new TowerHookEntry(name, hook);
        entries.Add(name, entry);
        registryQueue.AddOrInvoke(entry);
        return entry;
    }

    internal void Invoke(ITowerHookEntry entry)
    {
        TowerPatchRegistry.Register(entry.Name, entry.Hook);
    }
}

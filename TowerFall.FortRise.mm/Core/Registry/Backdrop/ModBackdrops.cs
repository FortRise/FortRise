using System.Collections.Generic;

namespace FortRise;
#nullable enable
public class ModBackdrops 
{
    private readonly Dictionary<string, IBackdropEntry> entries = new Dictionary<string, IBackdropEntry>();
    private readonly RegistryQueue<IBackdropEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModBackdrops(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IBackdropEntry>(Invoke);
    }

    public IBackdropEntry RegisterBackdrop(string id, in BackdropConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        IBackdropEntry command = new BackdropEntry(name, configuration);
        entries.Add(name, command);
        registryQueue.AddOrInvoke(command);
        return command;
    }

    public IBackdropEntry? GetBackdrop(string id)
    {
        string name = $"{metadata.Name}/{id}";
        entries.TryGetValue(name, out IBackdropEntry? backdrop);
        return backdrop;
    }

    internal void Invoke(IBackdropEntry entry)
    {
        BackdropRegistry.Register(entry.Name, entry.Configuration);
    }
}
#nullable enable
using System.Collections.Generic;

namespace FortRise;

public interface IModBackdrops
{
    IBackdropEntry? GetBackdrop(string id);
    IBackdropEntry RegisterBackdrop(string id, in BackdropConfiguration configuration);
}

internal sealed class ModBackdrops : IModBackdrops
{
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
        IBackdropEntry backdrop = new BackdropEntry(name, configuration);
        BackdropRegistry.AddBackdrop(backdrop);
        registryQueue.AddOrInvoke(backdrop);
        return backdrop;
    }

    public IBackdropEntry? GetBackdrop(string id)
    {
        return BackdropRegistry.GetBackdrop(id);
    }

    internal void Invoke(IBackdropEntry entry)
    {
        BackdropRegistry.Register(entry.Name, entry.Configuration);
    }
}
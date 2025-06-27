#nullable enable
using TowerFall;

namespace FortRise;

public interface IModBackgrounds
{
    IBackgroundEntry? GetBackground(string id);
    IBackgroundEntry RegisterBackground(string id, in BackgroundConfiguration configuration);
}

internal sealed class ModBackgrounds : IModBackgrounds
{
    private readonly RegistryQueue<IBackgroundEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModBackgrounds(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IBackgroundEntry>(Invoke);
    }

    public IBackgroundEntry RegisterBackground(string id, in BackgroundConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        IBackgroundEntry background = new BackgroundEntry(name, configuration);
        BackgroundRegistry.AddBackground(background);
        registryQueue.AddOrInvoke(background);
        return background;
    }

    public IBackgroundEntry? GetBackground(string id)
    {
        return BackgroundRegistry.GetBackground(id);
    }

    internal void Invoke(IBackgroundEntry entry)
    {
        GameData.BGs[entry.Name] = entry.Xml;
    }
}

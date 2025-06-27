#nullable enable
using System.Collections.Generic;

namespace FortRise;

public interface IModQuestEvents
{
    IQuestEventEntry RegisterQuestEvent(string id, in QuestEventConfiguration configuration);

    IQuestEventEntry? GetQuestEvent(string id);
}

internal sealed class ModQuestEvents : IModQuestEvents
{
    private readonly RegistryQueue<IQuestEventEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModQuestEvents(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IQuestEventEntry>(Invoke);
    }

    public IQuestEventEntry RegisterQuestEvent(string id, in QuestEventConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        IQuestEventEntry entry = new QuestEventEntry(name, configuration);
        QuestEventRegistry.AddQuestEvent(entry);
        registryQueue.AddOrInvoke(entry);
        return entry;
    }

    public IQuestEventEntry? GetQuestEvent(string id)
    {
        return QuestEventRegistry.GetQuestEvent(id);
    }

    internal void Invoke(IQuestEventEntry entry)
    {
        QuestEventRegistry.Register(entry.Name, entry.Configuration.Appear, entry.Configuration.Disappear);
    }
}
#nullable enable
using System.Collections.Generic;

namespace FortRise;

public class ModQuestEvents
{
    private readonly Dictionary<string, IQuestEventEntry> entries = new Dictionary<string, IQuestEventEntry>();
    private readonly RegistryQueue<IQuestEventEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModQuestEvents(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IQuestEventEntry>(Invoke);
    }

    public IQuestEventEntry RegisterTowerHook(string id, in QuestEventConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";       
        IQuestEventEntry entry = new QuestEventEntry(name, configuration);
        entries.Add(name, entry);
        registryQueue.AddOrInvoke(entry);
        return entry;
    }

    internal void Invoke(IQuestEventEntry entry)
    {
        QuestEventRegistry.Register(entry.Name, entry.Configuration.Appear, entry.Configuration.Disappear);
    }
}

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

public readonly struct QuestEventConfiguration
{
    public required QuestEventAction Appear { get; init; }
    public required QuestEventAction Disappear { get; init; }
}

public interface IQuestEventEntry 
{
    string Name { get; }
    QuestEventConfiguration Configuration { get; }
}

internal class QuestEventEntry(string name, QuestEventConfiguration configuration) : IQuestEventEntry
{
    public string Name { get; init; } = name;
    public QuestEventConfiguration Configuration { get; init; } = configuration;
}

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
#nullable enable
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public class ModGameModes
{
    private readonly Dictionary<string, IVersusGameModeEntry> entries = new Dictionary<string, IVersusGameModeEntry>();
    private readonly RegistryQueue<IVersusGameModeEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModGameModes(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IVersusGameModeEntry>(Invoke);
    }

    public IVersusGameModeEntry RegisterVersusGameMode(IVersusGameMode gameMode)
    {
        string id = $"{metadata.Name}/{gameMode.Name}";
        VersusGameModeEntry entry;
        entries.Add(id, entry = new VersusGameModeEntry(id, EnumPool.Obtain<Modes>(), gameMode));
        registryQueue.AddOrInvoke(entry);
        return entry;
    }

    public IVersusGameModeEntry? GetVersusGameMode(string name)
    {
        string id = $"{metadata.Name}/{name}";
        entries.TryGetValue(id, out IVersusGameModeEntry? gameMode);
        return gameMode;
    }

    internal void Invoke(IVersusGameModeEntry entry)
    {
        GameModeRegistry.Register(entry);
    }
}

#nullable enable
using System.Collections.Generic;

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
        entries.Add(id, entry = new VersusGameModeEntry(id, gameMode));
        registryQueue.AddOrInvoke(entry);
        return entry;
    }

    public IVersusGameModeEntry? GetVersusGameMode(string name)
    {
        entries.TryGetValue(name, out IVersusGameModeEntry? gameMode);
        return gameMode;
    }

    internal void Invoke(IVersusGameModeEntry entry)
    {
        GameModeRegistry.Register(entry);
    }
}

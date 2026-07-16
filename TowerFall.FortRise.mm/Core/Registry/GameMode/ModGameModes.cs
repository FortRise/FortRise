#nullable enable
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public interface IModGameModes
{
    IReadOnlyDictionary<string, ICoOpGameModeEntry> RegisteredCoOpGameModes { get; }
    IReadOnlyDictionary<string, IVersusGameModeEntry> RegisteredVersusGameModes { get; }
    IReadOnlyDictionary<string, IRollcallModesEntry> RegisteredRollcallModes { get; }

    ICoOpGameModeEntry? GetCoOpGameMode(string name);
    ICoOpGameModeEntry RegisterCoOpGameMode(string name, ICoOpGameMode coopGameMode);
    ICoOpGameModeEntry RegisterCoOpGameMode(string name, ICoOpGameMode coopGameMode, Modes modes);

    IVersusGameModeEntry? GetVersusGameMode(string name);
    IVersusGameModeEntry RegisterVersusGameMode(IVersusGameMode gameMode);
    IVersusGameModeEntry RegisterVersusGameMode(string name, IVersusGameMode gameMode);

    IRollcallModesEntry? GetRollcallMode(string name);
    IRollcallModesEntry RegisterRollcallMode(string name);
}

internal sealed class ModGameModes : IModGameModes
{
    private readonly RegistryQueue<IVersusGameModeEntry> versusRegistryQueue;
    private readonly RegistryQueue<ICoOpGameModeEntry> coopRegistryQueue;
    private readonly ModuleMetadata metadata;

    private readonly Dictionary<string, IRollcallModesEntry> registeredRollcallModes = [];

    public IReadOnlyDictionary<string, ICoOpGameModeEntry> RegisteredCoOpGameModes 
        => GameModeRegistry.RegistryCoOpGameModes;

    public IReadOnlyDictionary<string, IVersusGameModeEntry> RegisteredVersusGameModes 
        => GameModeRegistry.RegistryVersusGameModes;

    public IReadOnlyDictionary<string, IRollcallModesEntry> RegisteredRollcallModes 
        => registeredRollcallModes;

    internal ModGameModes(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        versusRegistryQueue = manager.CreateQueue<IVersusGameModeEntry>(VersusInvoke);
        coopRegistryQueue = manager.CreateQueue<ICoOpGameModeEntry>(CoOpInvoke);
    }

    public IVersusGameModeEntry RegisterVersusGameMode(IVersusGameMode gameMode)
    {
        return RegisterVersusGameMode(gameMode.Name, gameMode);
    }

    public IVersusGameModeEntry RegisterVersusGameMode(string name, IVersusGameMode gameMode)
    {
        string id = $"{metadata.Name}/{name}";
        VersusGameModeEntry entry;
        GameModeRegistry.AddVersusGamemode(entry = new VersusGameModeEntry(id, EnumPool.Obtain<Modes>(), gameMode));
        versusRegistryQueue.AddOrInvoke(entry);
        return entry;
    }

    public IVersusGameModeEntry? GetVersusGameMode(string id)
    {
        return GameModeRegistry.GetVersusGameMode(id);
    }

    public ICoOpGameModeEntry RegisterCoOpGameMode(string id, ICoOpGameMode coopGameMode)
    {
        return RegisterCoOpGameMode(id, coopGameMode, EnumPool.Obtain<Modes>());
    }

    public ICoOpGameModeEntry RegisterCoOpGameMode(string id, ICoOpGameMode coopGameMode, Modes modes)
    {
        string name = $"{metadata.Name}/{id}";

        ICoOpGameModeEntry coopModeEntry = new CoOpGameModeEntry(name, coopGameMode, modes);
        GameModeRegistry.AddCoOpGamemode(coopModeEntry);
        coopRegistryQueue.AddOrInvoke(coopModeEntry);
        return coopModeEntry;
    }

    public IRollcallModesEntry RegisterRollcallMode(string name)
    {
        string entryName = $"{metadata.Name}/{name}";

        IRollcallModesEntry rollCallEntry = new RollcallModeEntry(
            entryName, 
            EnumPool.Obtain<MainMenu.RollcallModes>()
        );

        registeredRollcallModes[name] = rollCallEntry;
        return rollCallEntry;
    }


    public IRollcallModesEntry? GetRollcallMode(string name)
    {
        registeredRollcallModes.TryGetValue(name, out var val);
        return val;
    }

    public ICoOpGameModeEntry? GetCoOpGameMode(string name)
    {
        GameModeRegistry.RegistryCoOpGameModes.TryGetValue(name, out var val);
        return val;
    }

    internal void CoOpInvoke(ICoOpGameModeEntry entry)
    {

    }

    internal void VersusInvoke(IVersusGameModeEntry entry)
    {
        GameModeRegistry.Register(entry);
    }
}

#nullable enable
using System.Collections.Generic;
using Monocle;

namespace FortRise;

public interface IModCommands
{
    ICommandEntry RegisterCommands(string id, CommandConfiguration configuration);
}

internal sealed class ModCommands : IModCommands
{
    private readonly Dictionary<string, ICommandEntry> entries = new Dictionary<string, ICommandEntry>();
    private readonly RegistryQueue<ICommandEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModCommands(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<ICommandEntry>(Invoke);
    }

    public ICommandEntry RegisterCommands(string id, CommandConfiguration configuration)
    {
        ICommandEntry command = new CommandEntry(id, configuration);
        entries.Add(id, command);
        registryQueue.AddOrInvoke(command);
        return command;
    }

    internal void Invoke(ICommandEntry entry)
    {
        var commands = Engine.Instance.Commands;
        commands.RegisterCommand(entry.Name, entry.Configuration.Callback);
    }
}

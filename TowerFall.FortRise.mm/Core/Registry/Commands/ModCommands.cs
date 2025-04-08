#nullable enable
using System.Collections.Generic;
using Monocle;

namespace FortRise;

public class ModCommands
{
    private readonly Dictionary<string, ICommand> entries = new Dictionary<string, ICommand>();
    private readonly RegistryQueue<ICommand> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModCommands(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<ICommand>(Invoke);
    }

    public ICommand RegisterCommands(string id, CommandConfiguration configuration)
    {
        ICommand command = new CommandMetadata(id, configuration);
        entries.Add(id, command);
        registryQueue.AddOrInvoke(command);
        return command;
    }

    internal void Invoke(ICommand entry)
    {
        var commands = Engine.Instance.Commands;
        commands.RegisterCommand(entry.Name, entry.Configuration.Callback);
    }
}

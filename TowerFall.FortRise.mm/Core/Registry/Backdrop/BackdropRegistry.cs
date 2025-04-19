using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using TowerFall;

namespace FortRise;

public static class BackdropRegistry 
{
    public delegate Background.BGElement BGElementLoader(Level level, XmlElement xmlElement);

    public static Dictionary<string, BGElementLoader> BGElements = new Dictionary<string, BGElementLoader>();

    public static void Register(string name, BackdropConfiguration configuration)
    {
        ConstructorInfo ctor = configuration.BackdropType.GetConstructor([typeof(Level), typeof(XmlElement)]);

        if (ctor != null)
        {
            Background.BGElement loader(Level level, XmlElement element)
            {
                return (Background.BGElement)ctor.Invoke([level, element]);
            }

            BGElements[name] = loader;
        }
        else 
        {
            Logger.Error($"[BackdropLoader] [{name}] Constructor (TowerFall.Level, System.Xml.XmlElement) couldn't be found!");
        }
    }
}

public readonly struct BackdropConfiguration 
{
    public required Type BackdropType { get; init; }
}

public interface IBackdropEntry 
{
    public string Name { get; }
    public BackdropConfiguration Configuration { get; }
}

internal sealed class BackdropEntry(string name, BackdropConfiguration configuration) : IBackdropEntry
{
    public string Name { get; init; } = name;
    public BackdropConfiguration Configuration { get; init; } = configuration;
}

public class ModBackdrops 
{
    private readonly Dictionary<string, IBackdropEntry> entries = new Dictionary<string, IBackdropEntry>();
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
        IBackdropEntry command = new BackdropEntry(name, configuration);
        entries.Add(name, command);
        registryQueue.AddOrInvoke(command);
        return command;
    }

    internal void Invoke(IBackdropEntry entry)
    {
        BackdropRegistry.Register(entry.Name, entry.Configuration);
    }
}
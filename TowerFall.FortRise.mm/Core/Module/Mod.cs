#nullable enable
using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FortRise;

/// <summary>
/// This is the mod's base class.
/// </summary>
public abstract class Mod
{
    public ModuleMetadata Meta { get; internal set; } = null!;

    public IModuleContext Context { get; internal set; }
    public IModContent ModContent { get; internal set; }
    public ILogger Logger { get; internal set; }

    public Action<IModuleContext>? OnLoad { get; set; }
    public Action<IModuleContext>? OnUnload { get; set; }
    public Action<IModuleContext>? OnInitialize { get; set; }
    public Action<IModuleContext, IModContent>? OnLoadContent { get; set; }

    private object? saveDataCache;
    private object? settingsCache;

    public Mod(IModContent content, IModuleContext context, ILogger logger)
    {
        Context = context;
        ModContent = content;
        Logger = logger;
        if (content != null!)
        {
            Meta = content.Metadata;
        }
    }

    /// <summary>
    /// Override this function and allows you to parse a launch arguments that has been passed to the game.
    /// </summary>
    /// <param name="args">A launch arguments that has been passed to the game</param>
    public virtual void ParseArgs(string[] args) { }
    public virtual object? GetApi() => null;

    public virtual ModuleSettings? CreateSettings() => null;
    public virtual ModuleSaveData? CreateSaveData() => null;

    internal void SaveSaveData()
    {

    }

    internal void SaveSettings()
    {
        if (settingsCache == null)
        {
            return;
        }

        var path = Path.Combine(ModIO.GetRootPath(), "Saves", Meta.Name, Meta.Name + ".settings" + ".json");
        ((ModuleSettings)settingsCache).Save(path);
    }

    internal void VerifySaveData()
    {
        ((ModuleSaveData?)saveDataCache)?.Verify();
    }

    public T? GetSaveData<T>()
    where T : ModuleSaveData
    {
        if (saveDataCache != null)
        {
            return (T)saveDataCache;
        }
        LoadSaveData(typeof(T));
        return (T?)saveDataCache;
    }

    public T? GetSettings<T>()
    where T : ModuleSettings
    {
        if (settingsCache != null)
        {
            return (T)settingsCache;
        }
        LoadSettings(typeof(T));
        return (T?)settingsCache;
    }

    internal ModuleSettings? GetSettings()
    {
        if (settingsCache != null)
        {
            return (ModuleSettings)settingsCache;
        }
        var modSettings = CreateSettings();
        if (modSettings is null)
        {
            return null;
        }
        LoadSettings(modSettings.GetType());
        return (ModuleSettings)settingsCache!;
    }

    internal void LoadSaveData(Type saveDataType)
    {
        var savePath = Path.Combine(ModIO.GetRootPath(), "Saves", Meta.Name, $"{Meta.Name}.saveData.json");
        var dirName = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(dirName))
        {
            Directory.CreateDirectory(dirName!);
        }

        if (File.Exists(savePath))
        {
            saveDataCache = JsonSerializer.Deserialize(File.ReadAllText(savePath), saveDataType);
            return;
        }

        saveDataCache = saveDataType.GetConstructor([])?.Invoke([]);
    }

    internal void LoadSettings(Type settingsType)
    {
        var settingsPath = Path.Combine(ModIO.GetRootPath(), "Saves", Meta.Name, $"{Meta.Name}.settings.json");
        var dirName = Path.GetDirectoryName(settingsPath);
        if (!Directory.Exists(dirName))
        {
            Directory.CreateDirectory(dirName!);
        }

        if (File.Exists(settingsPath))
        {
            settingsCache = JsonSerializer.Deserialize(File.ReadAllText(settingsPath), settingsType);
            return;
        }

        ModuleSettings? settings = (ModuleSettings?)settingsType.GetConstructor([])?.Invoke([]);
        settings?.Load(settingsPath);

        settingsCache = settings;
    }
}

public interface IModContent
{
    public ModuleMetadata Metadata { get; init; }
    /// <summary>
    /// The "Content" folder from a mod resource.
    /// </summary>
    public IResourceInfo Root { get; }
}


public interface IModuleContext
{
    public IModRegistry Registry { get; init; }
    public IModInterop Interop { get; init; }
    public IModEvents Events { get; init; }
    public ILogger Logger { get; init; }
    public IHarmony Harmony { get; init; }
}

internal sealed class ModuleContext : IModuleContext
{
    public IModRegistry Registry { get; init; }
    public IModInterop Interop { get; init; }
    public IModEvents Events { get; init; }
    public ILogger Logger { get; init; }
    public IHarmony Harmony { get; init; }

    public ModuleContext(IModRegistry registry, IModInterop interop, IModEvents events, ILogger logger, IHarmony harmony)
    {
        Registry = registry;
        Interop = interop;
        Events = events;
        Logger = logger;
        Harmony = harmony;
    }
}

internal class ModContent : IModContent
{
    public ModuleMetadata Metadata { get; init; }
    public IResourceInfo Root
    {
        get
        {
            return RiseCore.ResourceTree.Get($"mod:{Metadata.Name}/Content");
        }
    }


    public ModContent(ModuleMetadata metadata)
    {
        Metadata = metadata;
    }
}

public enum ModEventsType
{
    Initialize,
    VerifyData
}

public interface IModEvents
{
    public event EventHandler<ModuleMetadata>? OnModInitialize;
    public event EventHandler? OnModLoadingFinished;
    public event EventHandler? OnModInitializingFinished;
}

internal class ModEvents(ModEventsManager manager) : IModEvents
{
    public event EventHandler<ModuleMetadata>? OnModInitialize
    {
        add => manager.OnModInitialize += value;
        remove => manager.OnModInitialize -= value;
    }

    public event EventHandler? OnModLoadingFinished
    {
        add => manager.OnModLoadingFinished += value;
        remove => manager.OnModLoadingFinished -= value;
    }

    public event EventHandler? OnModInitializingFinished
    {
        add => manager.OnModInitializingFinished += value;
        remove => manager.OnModInitializingFinished -= value;
    }
}
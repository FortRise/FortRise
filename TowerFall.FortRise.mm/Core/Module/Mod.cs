#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace FortRise;

public abstract class Mod
{
    public ModuleMetadata Meta { get; internal set; } = null!;

    public IModuleContext Context { get; internal set; }
    public IModContent ModContent { get; internal set; }

    public Action<IModuleContext>? OnLoad { get; set; }
    public Action<IModuleContext>? OnUnload { get; set; }
    public Action<IModuleContext>? OnInitialize { get; set; }
    public Action<IModuleContext, IModContent>? OnLoadContent { get; set; }

    private object? saveDataCache;
    private object? settingsCache;

    public Mod(IModContent content, IModuleContext context)
    {
        Context = context;
        ModContent = content;
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
    /// <summary>
    /// Load a texture from a mod resource file.
    /// </summary>
    /// <param name="file">A file to be loaded</param>
    /// <returns>A <see cref="FortRise.ISubtextureEntry"/> containing the actual Subtexture</returns>
    ISubtextureEntry LoadTexture(IResourceInfo file);
    /// <summary>
    /// Load a texture from a callback.
    /// </summary>
    /// <param name="callback">A callback that returns a subtexture</param>
    /// <returns>A <see cref="FortRise.ISubtextureEntry"/> containing the actual Subtexture</returns>
    ISubtextureEntry LoadTexture(Func<Subtexture> callback);
}

/// <summary>
/// A container for the Subtexture.
/// </summary>
public interface ISubtextureEntry
{
    /// <summary>
    /// A direct resource path to the texture.
    /// </summary>
    public IResourceInfo? Path { get; init; }
    /// <summary>
    /// An actual subtexture to be used.
    /// </summary>
    public Subtexture? Subtexture { get; }
}

internal class SubtextureEntry : ISubtextureEntry
{
    public IResourceInfo? Path { get; init; }
    public Func<Subtexture>? Callback { get; set; }
    public Subtexture? Subtexture => GetActualSubtexture();

    private Subtexture? cache;

    public SubtextureEntry(IResourceInfo path)
    {
        Path = path;
    }

    public SubtextureEntry(Func<Subtexture> callback)
    {
        Callback = callback;
    }

    private Subtexture? GetActualSubtexture()
    {
        if (cache != null)
        {
            return cache;
        }
        if (Path != null)
        {
            using var stream = Path.Stream;
            var tex2D = Texture2D.FromStream(Engine.Instance.GraphicsDevice, stream);
            return cache = new Subtexture(new Monocle.Texture(tex2D));
        }

        return cache = Callback?.Invoke();
    }
}

public interface IModuleContext
{
    public IModRegistry Registry { get; init; }
    public IModInterop Interop { get; init; }
    public IModEvents Events { get; init; }
    public IHarmony Harmony { get; init; }
}

internal sealed class ModuleContext : IModuleContext
{
    public IModRegistry Registry { get; init; }
    public IModInterop Interop { get; init; }
    public IModEvents Events { get; init; }
    public IHarmony Harmony { get; init; }

    public ModuleContext(IModRegistry registry, IModInterop interop, IModEvents events, IHarmony harmony)
    {
        Registry = registry;
        Interop = interop;
        Events = events;
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

    private RegistryQueue<ISubtextureEntry> subtexturesQueue;
    private Dictionary<string, ISubtextureEntry> subtextureEntries = new();

    public ModContent(ModuleMetadata metadata, ModuleManager manager)
    {
        Metadata = metadata;
        subtexturesQueue = manager.CreateQueue<ISubtextureEntry>(InvokeSubtexture);
    }

    public ISubtextureEntry LoadTexture(IResourceInfo file)
    {
        if (!ModIO.IsFileExists(file.RootPath))
        {
            // TODO replace it with missing texture
            Logger.Error($"[{Metadata.Name}] Error loading a file naned: {file.RootPath}, file does not exists.");
            return null!;
        }
        ref var subtexture = ref CollectionsMarshal.GetValueRefOrAddDefault(subtextureEntries, file.RootPath, out bool exists);
        if (exists)
        {
            return subtexture!;
        }
        var entry = new SubtextureEntry(file);
        subtexturesQueue.AddOrInvoke(entry);
        return subtexture = entry;
    }

    public ISubtextureEntry LoadTexture(Func<Subtexture> callback)
    {
        var entry = new SubtextureEntry(callback);
        subtexturesQueue.AddOrInvoke(entry);
        return entry;
    }

    private void InvokeSubtexture(ISubtextureEntry entry)
    {

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
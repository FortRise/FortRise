#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;

namespace FortRise;


/// <summary>
/// A container for the Subtexture.
/// </summary>
public interface ISubtextureEntry
{
    /// <summary>
    /// A subtexture identity for Atlas.
    /// </summary>
    public string ID { get; init; }
    /// <summary>
    /// A direct resource path to the texture.
    /// </summary>
    public IResourceInfo? Path { get; init; }
    /// <summary>
    /// An actual subtexture to be used.
    /// </summary>
    public Subtexture? Subtexture { get; }
    /// <summary>
    /// The destination to put this subtexture on.
    /// </summary>
    public SubtextureAtlasDestination AtlasDestination { get; }
}

public enum SubtextureAtlasDestination
{
    Atlas,
    BGAtlas,
    MenuAtlas,
    BossAtlas
}

internal class SubtextureEntry : ISubtextureEntry
{
    public string ID { get; init; }
    public IResourceInfo? Path { get; init; }
    public Func<Subtexture>? Callback { get; set; }
    public Subtexture? Subtexture => GetActualSubtexture();
    public SubtextureAtlasDestination AtlasDestination { get; init; }

    private Subtexture? cache;

    public SubtextureEntry(string id, IResourceInfo path, SubtextureAtlasDestination destination)
    {
        ID = id;
        Path = path;
        AtlasDestination = destination;
    }

    public SubtextureEntry(string id, Func<Subtexture> callback, SubtextureAtlasDestination destination)
    {
        ID = id;
        Callback = callback;
        AtlasDestination = destination;
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

public class ModSubtextures
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<ISubtextureEntry> subtexturesQueue;
    private readonly Dictionary<string, ISubtextureEntry> subtextureEntries = new();

    internal ModSubtextures(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        subtexturesQueue = manager.CreateQueue<ISubtextureEntry>(Invoke);
    }

    /// <summary>
    /// Load a texture from a mod resource file.
    /// </summary>
    /// <param name="file">A file to be loaded</param>
    /// <param name="atlasDestination">Where to put this Subtexture to a vanilla Atlas</param>
    /// <returns>A <see cref="FortRise.ISubtextureEntry"/> containing the actual Subtexture</returns>
    public ISubtextureEntry RegisterTexture(IResourceInfo file, SubtextureAtlasDestination atlasDestination = SubtextureAtlasDestination.Atlas)
    {
        return RegisterTexture(file.Path, file, atlasDestination);
    }
    /// <summary>
    /// Load a texture from a callback.
    /// </summary>
    /// <param name="callback">A callback that returns a subtexture</param>
    /// <param name="atlasDestination">Where to put this Subtexture to a vanilla Atlas</param>
    /// <returns>A <see cref="FortRise.ISubtextureEntry"/> containing the actual Subtexture</returns>
    public ISubtextureEntry RegisterTexture(Func<Subtexture> callback, SubtextureAtlasDestination atlasDestination = SubtextureAtlasDestination.Atlas)
    {
        return RegisterTexture(Guid.CreateVersion7().ToString(), callback, atlasDestination);
    }

    /// <summary>
    /// Load a texture from a mod resource file.
    /// </summary>
    /// <param name="id">A subtexture identity for Atlas</param>
    /// <param name="file">A file to be loaded</param>
    /// <param name="atlasDestination">Where to put this Subtexture to a vanilla Atlas</param>
    /// <returns>A <see cref="FortRise.ISubtextureEntry"/> containing the actual Subtexture</returns>
    public ISubtextureEntry RegisterTexture(string id, IResourceInfo file, SubtextureAtlasDestination atlasDestination = SubtextureAtlasDestination.Atlas)
    {
        if (!ModIO.IsFileExists(file.RootPath))
        {
            // TODO replace it with missing texture
            Logger.Error($"[{metadata.Name}] Error loading a file naned: {file.RootPath}, file does not exists.");
            return null!;
        }
        ref var subtexture = ref CollectionsMarshal.GetValueRefOrAddDefault(subtextureEntries, file.RootPath, out bool exists);
        if (exists)
        {
            return subtexture!;
        }
        var name = $"{metadata.Name}/{id}";
        var entry = new SubtextureEntry(name, file, atlasDestination);
        subtexturesQueue.AddOrInvoke(entry);
        return subtexture = entry;
    }
    /// <summary>
    /// Load a texture from a callback.
    /// </summary>
    /// <param name="id">A subtexture identity for Atlas</param>
    /// <param name="callback">A callback that returns a subtexture</param>
    /// <param name="atlasDestination">Where to put this Subtexture to a vanilla Atlas</param>
    /// <returns>A <see cref="FortRise.ISubtextureEntry"/> containing the actual Subtexture</returns>
    public ISubtextureEntry RegisterTexture(string id, Func<Subtexture> callback, SubtextureAtlasDestination atlasDestination = SubtextureAtlasDestination.Atlas)
    {
        var name = $"{metadata.Name}/{id}";
        var entry = new SubtextureEntry(name, callback, atlasDestination);
        subtexturesQueue.AddOrInvoke(entry);
        return entry;
    }

    private void Invoke(ISubtextureEntry entry)
    {
        switch (entry.AtlasDestination)
        {
            case SubtextureAtlasDestination.Atlas:
                TFGame.Atlas.SubTextures[entry.ID] = entry.Subtexture;
                break;
            case SubtextureAtlasDestination.MenuAtlas:
                TFGame.MenuAtlas.SubTextures[entry.ID] = entry.Subtexture;
                break;
            case SubtextureAtlasDestination.BGAtlas:
                TFGame.BGAtlas.SubTextures[entry.ID] = entry.Subtexture;
                break;
            case SubtextureAtlasDestination.BossAtlas:
                TFGame.BossAtlas.SubTextures[entry.ID] = entry.Subtexture;
                break;
        }
    }
}
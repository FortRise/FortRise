#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Monocle;
using TowerFall;

namespace FortRise;

public interface IModSubtextures
{
    /// <summary>
    /// Load a texture from a mod resource file.
    /// </summary>
    /// <param name="file">A file to be loaded</param>
    /// <param name="atlasDestination">Where to put this Subtexture to a vanilla Atlas</param>
    /// <returns>A <see cref="FortRise.ISubtextureEntry"/> containing the actual Subtexture</returns>
    ISubtextureEntry RegisterTexture(IResourceInfo file, SubtextureAtlasDestination atlasDestination = SubtextureAtlasDestination.Atlas);

    /// <summary>
    /// Load a texture from a callback.
    /// </summary>
    /// <param name="callback">A callback that returns a subtexture</param>
    /// <param name="atlasDestination">Where to put this Subtexture to a vanilla Atlas</param>
    /// <returns>A <see cref="FortRise.ISubtextureEntry"/> containing the actual Subtexture</returns>
    ISubtextureEntry RegisterTexture(Func<Subtexture> callback, SubtextureAtlasDestination atlasDestination = SubtextureAtlasDestination.Atlas);

    /// <summary>
    /// Load a texture from a mod resource file.
    /// </summary>
    /// <param name="id">A subtexture identity for Atlas</param>
    /// <param name="file">A file to be loaded</param>
    /// <param name="atlasDestination">Where to put this Subtexture to a vanilla Atlas</param>
    /// <returns>A <see cref="FortRise.ISubtextureEntry"/> containing the actual Subtexture</returns>
    ISubtextureEntry RegisterTexture(string id, IResourceInfo file, SubtextureAtlasDestination atlasDestination = SubtextureAtlasDestination.Atlas);


    /// <summary>
    /// Load a texture from a callback.
    /// </summary>
    /// <param name="id">A subtexture identity for Atlas</param>
    /// <param name="callback">A callback that returns a subtexture</param>
    /// <param name="atlasDestination">Where to put this Subtexture to a vanilla Atlas</param>
    /// <returns>A <see cref="FortRise.ISubtextureEntry"/> containing the actual Subtexture</returns>
    ISubtextureEntry RegisterTexture(string id, Func<Subtexture> callback, SubtextureAtlasDestination atlasDestination = SubtextureAtlasDestination.Atlas);

    ISubtextureEntry? GetTexture(string id, SubtextureAtlasDestination atlasDestination);
}

internal sealed class ModSubtextures : IModSubtextures
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

        var name = $"{metadata.Name}/{id}";
        ref var subtexture = ref CollectionsMarshal.GetValueRefOrAddDefault(subtextureEntries, name, out bool exists);
        if (exists)
        {
            if (subtexture!.Path != file)
            {
                Logger.Warning($"[{metadata.Name}] The subtexture ID: {id} but with a different path has already been registered, overriding!");
            }
            else
            {
                return subtexture!;
            }
        }
        var entry = new SubtextureEntry(name, file, atlasDestination);
        SubtextureRegistry.AddSubtexture(entry, atlasDestination);
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
        ref var subtexture = ref CollectionsMarshal.GetValueRefOrAddDefault(subtextureEntries, id, out bool exists);
        if (exists)
        {
            Logger.Warning($"[{metadata.Name}] The subtexture ID: {id} has already been registered, overriding!");
        }

        var entry = new SubtextureEntry(name, callback, atlasDestination);
        SubtextureRegistry.AddSubtexture(entry, atlasDestination);
        subtexturesQueue.AddOrInvoke(entry);
        return subtexture = entry;
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

    public ISubtextureEntry? GetTexture(string id, SubtextureAtlasDestination atlasDestination)
    {
        return SubtextureRegistry.GetSubtexture(id, atlasDestination);
    }
}

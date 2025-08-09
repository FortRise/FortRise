#nullable enable
using System;
using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise;

public interface IModSFXs
{
    ISFXEntry RegisterSFX(string id, IResourceInfo sfxPath, bool obeysMasterPitch = true);
    ISFXEntry RegisterSFX(string id, Func<SFX> callback, bool obeysMasterPitch = true);

    ISFXInstancedEntry RegisterSFXInstanced(string id, IResourceInfo sfxPath, int instances = 2, bool obeysMasterPitch = true);
    ISFXInstancedEntry RegisterSFXInstanced(string id, Func<patch_SFXInstanced> callback, int instances = 2, bool obeysMasterPitch = true);

    ISFXLoopedEntry RegisterSFXLooped(string id, IResourceInfo path, bool obeysMasterPitch = true);
    ISFXLoopedEntry RegisterSFXLooped(string id, Func<patch_SFXLooped> path, bool obeysMasterPitch = true);

    ISFXVariedEntry RegisterSFXVaried(string id, IResourceInfo[] sfxVariations, bool obeysMasterPitch = true);
    ISFXVariedEntry RegisterSFXVaried(string id, Func<patch_SFXVaried> sfxVariations, bool obeysMasterPitch = true);
    ISFXVariedEntry RegisterSFXVaried(string id, IResourceInfo[] sfxVariations, int count, bool obeysMasterPitch = true);

    ISFXEntry? GetSFX(string id);
    ISFXInstancedEntry? GetSFXInstanced(string id);
    ISFXLoopedEntry? GetSFXLooped(string id);
    ISFXVariedEntry? GetSFXVaried(string id);
}

internal sealed class ModSFXs : IModSFXs
{
    private readonly ModuleMetadata metadata;
    private readonly Dictionary<string, IBaseSFXEntry> sfxEntries = new Dictionary<string, IBaseSFXEntry>();
    private readonly RegistryQueue<IBaseSFXEntry> sfxQueue;

    internal ModSFXs(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        sfxQueue = manager.CreateQueue<IBaseSFXEntry>(Invoke);
    }

    public ISFXEntry RegisterSFX(string id, IResourceInfo sfxPath, bool obeysMasterPitch = true)
    {
        string name = $"{metadata.Name}/{id}";
        ISFXEntry sfxEntry = new SFXEntry(name, sfxPath, obeysMasterPitch);
        sfxEntries.Add(name, sfxEntry);
        sfxQueue.AddOrInvoke(sfxEntry);
        return sfxEntry;
    }

    public ISFXInstancedEntry RegisterSFXInstanced(string id, IResourceInfo sfxPath, int instances = 2, bool obeysMasterPitch = true)
    {
        string name = $"{metadata.Name}/{id}";
        ISFXInstancedEntry sfxInstancedEntry = new SFXInstancedEntry(name, sfxPath, instances, obeysMasterPitch);
        sfxEntries.Add(name, sfxInstancedEntry);
        sfxQueue.AddOrInvoke(sfxInstancedEntry);
        return sfxInstancedEntry;
    }

    public ISFXLoopedEntry RegisterSFXLooped(string id, IResourceInfo path, bool obeysMasterPitch = true)
    {
        string name = $"{metadata.Name}/{id}";
        ISFXLoopedEntry sfxLooped = new SFXLoopedEntry(name, path, obeysMasterPitch);
        sfxEntries.Add(name, sfxLooped);
        sfxQueue.AddOrInvoke(sfxLooped);
        return sfxLooped;
    }

    public ISFXVariedEntry RegisterSFXVaried(string id, IResourceInfo[] sfxVariations, bool obeysMasterPitch = true)
    {
        return RegisterSFXVaried(id, sfxVariations, sfxVariations.Length, obeysMasterPitch);
    }

    public ISFXVariedEntry RegisterSFXVaried(string id, IResourceInfo[] sfxVariations, int count, bool obeysMasterPitch = true)
    {
        string name = $"{metadata.Name}/{id}";
        ISFXVariedEntry sfxVariedEntry = new SFXVariedEntry(name, sfxVariations, count, obeysMasterPitch);
        sfxEntries.Add(name, sfxVariedEntry);
        sfxQueue.AddOrInvoke(sfxVariedEntry);
        return sfxVariedEntry;
    }

    public ISFXEntry RegisterSFX(string id, Func<SFX> callback, bool obeysMasterPitch = true)
    {
        string name = $"{metadata.Name}/{id}";
        ISFXEntry sfxEntry = new SFXEntry(name, callback, obeysMasterPitch);
        sfxEntries.Add(name, sfxEntry);
        sfxQueue.AddOrInvoke(sfxEntry);
        return sfxEntry;
    }

    public ISFXInstancedEntry RegisterSFXInstanced(string id, Func<patch_SFXInstanced> callback, int instances = 2, bool obeysMasterPitch = true)
    {
        string name = $"{metadata.Name}/{id}";
        ISFXInstancedEntry sfxInstancedEntry = new SFXInstancedEntry(name, callback, instances, obeysMasterPitch);
        sfxEntries.Add(name, sfxInstancedEntry);
        sfxQueue.AddOrInvoke(sfxInstancedEntry);
        return sfxInstancedEntry;
    }

    public ISFXLoopedEntry RegisterSFXLooped(string id, Func<patch_SFXLooped> callback, bool obeysMasterPitch = true)
    {
        string name = $"{metadata.Name}/{id}";
        ISFXLoopedEntry sfxLooped = new SFXLoopedEntry(name, callback, obeysMasterPitch);
        sfxEntries.Add(name, sfxLooped);
        sfxQueue.AddOrInvoke(sfxLooped);
        return sfxLooped;
    }

    public ISFXVariedEntry RegisterSFXVaried(string id, Func<patch_SFXVaried> sfxVariations, bool obeysMasterPitch = true)
    {
        string name = $"{metadata.Name}/{id}";
        ISFXVariedEntry sfxVariedEntry = new SFXVariedEntry(name, sfxVariations, obeysMasterPitch);
        sfxEntries.Add(name, sfxVariedEntry);
        sfxQueue.AddOrInvoke(sfxVariedEntry);
        return sfxVariedEntry;
    }

    public ISFXEntry? GetSFX(string id)
    {
        return SFXRegistry.GetSFX<ISFXEntry>(id);
    }

    public ISFXInstancedEntry? GetSFXInstanced(string id)
    {
        return SFXRegistry.GetSFX<ISFXInstancedEntry>(id);
    }

    public ISFXLoopedEntry? GetSFXLooped(string id)
    {
        return SFXRegistry.GetSFX<ISFXLoopedEntry>(id);
    }

    public ISFXVariedEntry? GetSFXVaried(string id)
    {
        return SFXRegistry.GetSFX<ISFXVariedEntry>(id);
    }

    private void Invoke(IBaseSFXEntry entry)
    {
        patch_Sounds.AddSFX(metadata, entry.Name, entry.BaseSFX);
    }
}

#nullable enable
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public interface IModSFXs
{
    ISFXEntry RegisterSFX(string id, IResourceInfo sfxPath, bool obeysMasterPitch = true);
    ISFXInstancedEntry RegisterSFXInstanced(string id, IResourceInfo sfxPath, int instances = 2, bool obeysMasterPitch = true);
    ISFXLoopedEntry RegisterSFXLooped(string id, IResourceInfo path, bool obeysMasterPitch = true);
    ISFXVariedEntry RegisterSFXVaried(string id, IResourceInfo[] sfxVariations, bool obeysMasterPitch = true);
    ISFXVariedEntry RegisterSFXVaried(string id, IResourceInfo[] sfxVariations, int count, bool obeysMasterPitch = true);
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

    private void Invoke(IBaseSFXEntry entry)
    {
        patch_Sounds.AddSFX(metadata, entry.Name, entry.BaseSFX);
    }
}
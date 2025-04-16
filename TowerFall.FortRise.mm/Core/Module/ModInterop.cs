#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Nanoray.Pintail;

namespace FortRise;

internal class ModInterop : IModInterop
{
    private readonly Dictionary<string, object?> apiCache = [];
    private readonly ModuleManager manager;

    public IReadOnlyList<IModResource> LoadedMods => manager.Mods;
    public IReadOnlyList<FortModule> LoadedFortModules => manager.Modules;
    public IProxyManager<string> ProxyManager { get; private set; }

    internal ModInterop(ModuleManager moduleManager, IProxyManager<string> proxyManager)
    {
        manager = moduleManager;
        ProxyManager = proxyManager;
    }

    public string[]? GetTags(string modName) => manager.GetTags(modName);
    public string[] GetAllTags() => manager.GetAllTags();
    public IReadOnlyList<IModResource> GetModsByTag(string tag) => manager.GetModsByTag(tag);
    public IModResource? GetMod(string tag) => manager.GetMod(tag);
    public IModRegistry? GetModRegistry(string modName) => manager.GetRegistry(modName);
    public IModRegistry? GetModRegistry(ModuleMetadata metadata) => manager.AddOrGetRegistry(metadata);

    public bool IsModExists(string name)
    {
        foreach (var mod in LoadedMods)
        {
            if (mod.Metadata.Name == name)
            {
                return true;
            }
        }

        return false;
    }

    public T? GetApi<T>(string modName, Option<SemanticVersion> minimumVersion = default) where T : class
    {
        if (!typeof(T).IsInterface)
            throw new ArgumentException($"The requested API type {typeof(T)} is not an interface.");

        manager.NameToFortModule.TryGetValue(modName, out FortModule? mod);        

        if (mod is null)
        {
            throw new ArgumentException($"The mod {modName} does not exists on the mod lists.");
        }

        if (minimumVersion.HasValue && minimumVersion.Value > mod.Meta.Version)
        {
            throw new ArgumentException($"The minimum version requested for {modName} is {minimumVersion}, but it has {mod.Meta.Version} instead.");
        }

        ref var apiObject = ref CollectionsMarshal.GetValueRefOrAddDefault(apiCache, modName, out bool exists);
        if (!exists)
        {
            apiObject = mod.GetApi();
        }

        if (apiObject is null)
        {
            throw new ArgumentException($"The mod {modName} does not expose an API.");
        }

        return ProxyManager.ObtainProxy<string, T>(apiObject, modName, mod.Meta.Name);
    }
}
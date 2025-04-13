using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FortRise;

// want to make it as a record class, but somehow we need to store a reference to it, so I can't
internal class ModDelayed(int requiredCount, ModuleMetadata metadata) 
{
    public int RequiredCount = requiredCount;
    public ModuleMetadata Metadata = metadata;
}

internal class ModuleManager 
{
    public enum LoadState { Load, Initialiazing, Ready }

    /// <summary>
    /// Contains a read-only access to all of the Modules.
    /// </summary>
    public ReadOnlyCollection<FortModule> Modules => InternalFortModules.AsReadOnly();

    /// <summary>
    /// Contains a read-only access to all of the Mods' metadata and resource.
    /// </summary>
    public ReadOnlyCollection<RiseCore.ModResource> Mods => InternalMods.AsReadOnly();
    internal List<FortModule> InternalFortModules = new();
    internal HashSet<ModuleMetadata> InternalModuleMetadatas = new();
    internal List<RiseCore.ModResource> InternalMods = new();


    private List<RegistryQueue> registryBatch = new List<RegistryQueue>();
    private Dictionary<string, IModRegistry> registries = new Dictionary<string, IModRegistry>();
    public LoadState State { get; set; }

    public enum LoadError { Delayed, Failure }
    internal HashSet<string> BlacklistedMods;
    internal HashSet<string> CantLoad = new();

    internal void LoadModsFromDirectory(string modPath)
    {
        var delayedMods = new List<ModDelayed>();
        var modDirectory = modPath;
        var directory = Directory.GetDirectories(modDirectory);
        foreach (var dir in directory)
        {
            if (dir.Contains("_RelinkerCache"))
                continue;
            var dirInfo = new DirectoryInfo(dir);
            if (BlacklistedMods != null && BlacklistedMods.Contains(dirInfo.Name))
            {
                Logger.Verbose($"[Loader] Ignored {dir} as it's blacklisted");
                continue;
            }
            LoadDir(dir, delayedMods);
        }

        var files = Directory.GetFiles(modDirectory);
        foreach (var file in files)
        {
            if (!file.EndsWith("zip"))
                continue;
            var fileName = Path.GetFileName(file);
            if (BlacklistedMods != null && BlacklistedMods.Contains(Path.GetFileName(fileName)))
            {
                Logger.Verbose($"[Loader] Ignored {file} as it's blacklisted");
                continue;
            }
            LoadZip(file, delayedMods);
        }
        LoadDelayedMods(delayedMods);
    }

    private void LoadDir(string dir, List<ModDelayed> delayedMods)
    {
        var metaPath = Path.Combine(dir, "meta.json");
        if (!File.Exists(metaPath))
        {
            return;
        }

        var result = ModuleMetadata.ParseMetadata(dir, metaPath);
        if (!result.Check(out ModuleMetadata moduleMetadata, out string error))
        {
            ErrorPanel.StoreError(error);
            Logger.Error(error);
            return;
        }

        if (!LoadMod(moduleMetadata, out int requiredDependencies).Check(out _, out LoadError err))
        {
            if (err == LoadError.Delayed)
            {
                delayedMods.Add(new ModDelayed(requiredDependencies, moduleMetadata));
            }
        }
    }

    private void LoadZip(string file, List<ModDelayed> delayedMods)
    {
        using var zipFile = ZipFile.OpenRead(file);

        string metaFile = "meta.json";
        var metaZip = zipFile.GetEntry(metaFile);
        if (metaZip == null)
        {
            return;
        }

        using var memStream = metaZip.ExtractStream();

        var result = ModuleMetadata.ParseMetadata(file, memStream, true);
        if (!result.Check(out ModuleMetadata moduleMetadata, out string error))
        {
            ErrorPanel.StoreError(error);
            Logger.Error(error);
            return;
        }

        if (!LoadMod(moduleMetadata, out int requiredDependencies).Check(out _, out LoadError err))
        {
            if (err == LoadError.Delayed)
            {
                delayedMods.Add(new ModDelayed(requiredDependencies, moduleMetadata));
            }
        }
    }

    public bool CheckDependencyMetadata(ModuleMetadata metadata, bool storeError)
    {
        foreach (var internalMetadata in InternalModuleMetadatas)
        {
            if (metadata.Name != internalMetadata.Name)
            {
                continue;
            }

            if (metadata.Version.Major != internalMetadata.Version.Major || metadata.Version > internalMetadata.Version)
            {
                if (storeError)
                {
                    ErrorPanel.StoreError($"Outdated Dependency {metadata.Name} {metadata.Version} > {internalMetadata.Version}");
                }
                return false;
            }

            return true;
        }

        return false;
    }

    public bool CheckDependencies(ModuleMetadata metadata, out int requiredDependencies)
    {
        requiredDependencies = 0;
        if (metadata.Dependencies != null)
        {
            foreach (var dep in metadata.Dependencies)
            {
                if (CheckDependencyMetadata(dep, true))
                {
                    continue;
                }
                requiredDependencies += 1;

                return false;
            }
        }

        if (metadata.OptionalDependencies != null)
        {
            foreach (var dep in metadata.OptionalDependencies)
            {
                if (CheckDependencyMetadata(dep, false))
                {
                    continue;
                }

                return false;
            }
        }
        return true;
    }

    public Result<Unit, LoadError> LoadMod(ModuleMetadata metadata, out int requiredDependencies)
    {
        if (!CheckDependencies(metadata, out requiredDependencies))
        {
            return LoadError.Delayed;
        }

        return LoadModSkipDependecies(metadata);
    }

    public Result<Unit, LoadError> LoadModSkipDependecies(ModuleMetadata metadata)
    {
        Assembly asm = null;
        RiseCore.ModResource modResource;
        if (!string.IsNullOrEmpty(metadata.PathZip))
        {
            modResource = new RiseCore.ZipModResource(metadata);

            RiseCore.ResourceTree.AddMod(metadata, modResource);

            if (!RiseCore.DisableFortMods)
            {
                using var zip = ZipFile.OpenRead(metadata.PathZip);
                var dllPath = metadata.DLL.Replace('\\', '/');
                var dllMeta = zip.GetEntry(dllPath);
                if (dllMeta != null)
                {
                    metadata.AssemblyLoadContext = new ModAssemblyLoadContext(metadata);

                    using var dll = dllMeta.ExtractStream();
                    asm = RiseCore.Relinker.LoadModAssembly(metadata, metadata.DLL, dll);
                }
            }
        }
        else if (!string.IsNullOrEmpty(metadata.PathDirectory))
        {
            modResource = new RiseCore.FolderModResource(metadata);

            RiseCore.ResourceTree.AddMod(metadata, modResource);
            var fullDllPath = Path.Combine(metadata.PathDirectory, metadata.DLL);

            if (!RiseCore.DisableFortMods)
            {
                if (File.Exists(fullDllPath))
                {
                    metadata.AssemblyLoadContext = new ModAssemblyLoadContext(metadata);

                    using var stream = File.OpenRead(fullDllPath);
                    asm = RiseCore.Relinker.LoadModAssembly(metadata, metadata.DLL, stream);
                }
            }
        }
        else
        {
            Logger.Error($"[Loader] Mod {metadata.Name} not found");
            ErrorPanel.StoreError($"'{metadata.Name}' not found!");
            return LoadError.Failure;
        }

        if (asm != null)
        {
            LoadAssembly(metadata, modResource, asm);
        }

        InternalMods.Add(modResource);
        InternalModuleMetadatas.Add(metadata);

        return new Unit();
    }

    private void LoadDelayedMods(List<ModDelayed> delayedMods)
    {
        List<ModDelayed> successfulLoad = [];
        for (int i = 0; i < delayedMods.Count; i++)
        {
            var delayMod = delayedMods[i];
            if (!LoadMod(delayMod.Metadata, out int requiredDependencies).IsError)
            {
                successfulLoad.Add(delayMod);
            }
            
            delayMod.RequiredCount = requiredDependencies;
            delayedMods[i] = delayMod;
        }
        
        bool loadedAnother = successfulLoad.Count != 0;

        foreach (var success in successfulLoad)
        {
            delayedMods.Remove(success);
        }

        // Loads another batch of mods that are delayed
        if (loadedAnother)
        {
            LoadDelayedMods(delayedMods);
            return;
        }

        foreach (var delayedMod in delayedMods)
        {
            if (delayedMod.RequiredCount > 0)
            {
                // unfortunate mods that cannot be loaded in
                if (!string.IsNullOrEmpty(delayedMod.Metadata.PathDirectory))
                {
                    CantLoad.Add(delayedMod.Metadata.PathDirectory);
                }
                else if (!string.IsNullOrEmpty(delayedMod.Metadata.PathZip))
                {
                    CantLoad.Add(delayedMod.Metadata.PathZip);
                }

                ErrorPanel.StoreError($"'{delayedMod.Metadata.Name}' has missing dependencies.");
            }
            else 
            {
                // Hey, we can load this one, dependency is not required!
                LoadModSkipDependecies(delayedMod.Metadata);
            }
        }
    }

    private void LoadAssembly(ModuleMetadata metadata, RiseCore.ModResource resource, Assembly asm)
    {
        foreach (var t in asm.GetTypes())
        {
            if (t.BaseType != typeof(FortModule))
            {
                continue;
            }

            var customAttribute = t.GetCustomAttribute<FortAttribute>();
            if (customAttribute == null)
            {
                customAttribute = new FortAttribute($"unknown.{metadata.Author}.{metadata.Name}", metadata.Name);
            }

            FortModule module = Activator.CreateInstance(t) as FortModule;

            module.Meta = metadata;
            module.Name = customAttribute.Name;
            module.ID = customAttribute.GUID;
            var content = resource.Content;
            module.s_Content(content);
            module.ParseArgs(RiseCore.ApplicationArgs);
            module.InternalLoad();

            InternalFortModules.Add(module);

            Logger.Info($"[Loader] {module.ID}: {module.Name} Loaded.");
            continue;
        }
    }

    internal void Initialize()
    {
        State = LoadState.Initialiazing;
        foreach (var fortModule in InternalFortModules)
        {
            IModRegistry registry = AddOrGetRegistry(fortModule.Meta);
            IModInterop interop = new ModInterop(this);

            fortModule.IsInitialized = true;
            fortModule.Registry = registry;
            fortModule.Interop = interop;
            fortModule.Initialize();
            RiseCore.Events.Invoke_OnModInitialized(fortModule);
        }

        foreach (var batch in registryBatch)
        {
            batch.Invoke();
        }
    }

#nullable enable
    internal IModRegistry? GetRegistry(string modName)
    {
        registries.TryGetValue(modName, out IModRegistry? value);
        return value;
    }
#nullable disable

    internal IModRegistry AddOrGetRegistry(ModuleMetadata metadata)
    {
        ref var registry = ref CollectionsMarshal.GetValueRefOrAddDefault(registries, metadata.Name, out bool exists);
        if (!exists)
        {
            IModRegistry reg = new ModRegistry(metadata, this);
            registry = reg;
        }

        return registry;
    }

    internal RegistryQueue<T> CreateQueue<T>(Action<T> invoker)
    where T : class
    {
        var registryQueue = new RegistryQueue<T>(this, invoker);
        registryBatch.Add(registryQueue);
        return registryQueue;
    }
}
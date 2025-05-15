using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using HarmonyLib;
using Nanoray.Pintail;

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

    public LoadState State { get; set; }
    /// <summary>
    /// Contains a read-only access to all of the Modules.
    /// </summary>
    public ReadOnlyCollection<FortModule> Modules => InternalFortModules.AsReadOnly();

    /// <summary>
    /// Contains a read-only access to all of the Mods' metadata and resource.
    /// </summary>
    public ReadOnlyCollection<IModResource> Mods => InternalMods.AsReadOnly();
    internal List<FortModule> InternalFortModules = new();
    internal HashSet<ModuleMetadata> InternalModuleMetadatas = new();
    internal List<IModResource> InternalMods = new();
    internal HashSet<string> InternalTags = new();

    internal Dictionary<string, FortModule> NameToFortModule = new Dictionary<string, FortModule>();
    internal Dictionary<string, IModResource> NameToMod = new Dictionary<string, IModResource>();


    private List<RegistryQueue> registryBatch = new List<RegistryQueue>();
    private Dictionary<string, IModRegistry> registries = new Dictionary<string, IModRegistry>();
    private IProxyManager<string> proxyManager;

    public enum LoadError { Delayed, Failure }
    internal HashSet<string> BlacklistedMods;
    internal HashSet<string> CantLoad = new();
    
    internal ModuleManager()
    {
        var moduleBuilders = new Dictionary<(string, string), ModuleBuilder>();
        proxyManager = new ProxyManager<string>((proxyInfo) => {
            var key = (proxyInfo.Target.Context, proxyInfo.Proxy.Context);

            ref var moduleBuilder = ref CollectionsMarshal.GetValueRefOrAddDefault(moduleBuilders, key, out bool exists);

            if (!exists)
            {
                string proxyAsmName = 
                $"{GetType().Namespace}.Proxies{moduleBuilders.Count}, Version={GetType().Assembly.GetName().Version}, Culture=neutral";
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(proxyAsmName), AssemblyBuilderAccess.RunAndCollect);
                moduleBuilder = assemblyBuilder.DefineDynamicModule($"{GetType().Namespace}.Proxies");
            }

            return moduleBuilder;
        },
        new() 
        {
            ProxyPrepareBehavior = ProxyManagerProxyPrepareBehavior.Eager,
            ProxyObjectInterfaceMarking = ProxyObjectInterfaceMarking.IncludeProxyTargetInstance | ProxyObjectInterfaceMarking.IncludeProxyInfo,
            AccessLevelChecking = AccessLevelChecking.DisabledButOnlyAllowPublicMembers
        });
    }

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
        IModResource modResource;
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
        NameToMod.Add(metadata.Name, modResource);
        if (metadata.Tags != null)
        {
            foreach (var tag in metadata.Tags)
            {
                InternalTags.Add(tag);
            }
        }
        

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

    private void LoadAssembly(ModuleMetadata metadata, IModResource resource, Assembly asm)
    {
        foreach (var t in asm.GetTypes())
        {
            if (t.BaseType != typeof(FortModule))
            {
                continue;
            }
            FortModule module = Activator.CreateInstance(t) as FortModule;

            module.Meta = metadata;
            module.Harmony = new Harmony(metadata.Name);
            module.Content = resource.Content;
            module.ParseArgs(RiseCore.ApplicationArgs);
            module.InternalLoad();

            InternalFortModules.Add(module);
            NameToFortModule.Add(metadata.Name, module);

            Logger.Info($"[Loader] {module.Meta.Name} Loaded.");
            continue;
        }
    }

    internal void Initialize()
    {
        var resLoaderHooks = new List<IResourceLoader>();
        State = LoadState.Initialiazing;
        foreach (var fortModule in InternalFortModules)
        {
            IModRegistry registry = AddOrGetRegistry(fortModule.Meta);
            IModInterop interop = new ModInterop(this, proxyManager);

            fortModule.IsInitialized = true;
            fortModule.Registry = registry;
            fortModule.Interop = interop;
            fortModule.Initialize();
            if (fortModule is IResourceLoader loader)
            {
                resLoaderHooks.Add(loader);
            }
            RiseCore.Events.Invoke_OnModInitialized(fortModule);
        }

        foreach (var batch in registryBatch)
        {
            batch.Invoke();
        }

        // run hooks

        foreach (var loader in resLoaderHooks)
        {
            foreach (var mod in InternalMods)
            {
                loader.LoadResource(mod);
            }
        }
    }

#nullable enable
    internal IReadOnlyList<IModResource> GetModsByTag(string tag)
    {
        return [.. InternalMods.Where(x => 
        {
            var tags = x.Metadata.Tags;
            if (tags is null)
            {
                return false;
            }
            return tags.Contains(tag);
        })];
    }

    internal string[] GetAllTags()
    {
        return InternalTags.ToArray();
    }

    internal string[]? GetTags(string modName)
    {
        string[]? tags = null;
        foreach (var mod in InternalMods)
        {
            if (mod.Metadata.Name == modName)
            {
                tags = mod.Metadata.Tags;
                break;
            }
        }

        return tags;
    }

    internal IModResource? GetMod(string modName)
    {
        NameToMod.TryGetValue(modName, out IModResource? resource);
        return resource;
    }

    internal IModRegistry? GetRegistry(string modName)
    {
        registries.TryGetValue(modName, out IModRegistry? value);
        return value;
    }

    internal IModRegistry? GetRegistry(ModuleMetadata metadata)
    {
        registries.TryGetValue(metadata.Name, out IModRegistry? value);
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
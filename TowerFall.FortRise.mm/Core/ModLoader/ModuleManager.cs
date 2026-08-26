using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Monocle;
using Nanoray.Pintail;

namespace FortRise;

internal class ModOrder : IComparer<ModuleMetadata>
{
    public int Compare(ModuleMetadata x, ModuleMetadata y)
    {
        if (x.Priority > y.Priority)
        {
            return 1;
        }

        if (x.Priority < y.Priority)
        {
            return -1;
        }

        return 0;
    }
}

internal class ModuleManager
{
    public enum LoadError { Delayed, Failure }
    public LoadState State { get; set; }
    /// <summary>
    /// Contains a read-only access to all of the Modules.
    /// </summary>
    public ReadOnlyCollection<Mod> Modules => InternalFortModules.AsReadOnly();

    /// <summary>
    /// Contains a read-only access to all of the Mods' metadata and resource.
    /// </summary>
    public ReadOnlyCollection<IModResource> Mods => InternalMods.AsReadOnly();

    public IReadOnlyDictionary<RegistryBatchType, List<RegistryQueue>> RegistryBatches => registryBatch;

    public static ModuleManager Instance { get; private set; }
    internal List<Mod> InternalFortModules = [];
    internal List<IModResource> InternalMods = [];

    internal HashSet<ModuleMetadata> InternalModuleMetadatas = [];

    internal ModEventsManager EventsManager = new();

    // TODO: combine these into one
    internal Dictionary<string, Mod> NameToFortModule = [];
    internal Dictionary<string, IModResource> NameToMod = [];
    internal Dictionary<string, Subtexture> NameToIcon = [];
    internal Dictionary<string, bool> ActiveMods = [];

    internal HashSet<string> BlacklistedMods;

    internal HashSet<string> DisabledMods = [];
    internal string DisabledModsPath;

    private readonly Dictionary<RegistryBatchType, List<RegistryQueue>> registryBatch = [];
    private readonly Dictionary<string, List<RegistryUnloader>> modUnloaders = [];
    private readonly Dictionary<string, IModRegistry> registries = [];
    private readonly IProxyManager<string> proxyManager;
    private readonly Dictionary<(string modTargetName, string modRequesterName), ModuleBuilder> proxyModuleBuilders = [];

    private readonly List<ModuleMetadata> loadOrder = [];
    private readonly Dictionary<string, int> loadOrderIndex = [];

    private readonly Dictionary<string, List<(RegistryBatchType BatchType, RegistryQueue Queue)>> modQueues = [];
    private string registryOwner;

    private readonly ILogger logger;
    private readonly ILoggerFactory loggerFactory;
    // we can cache these for now
    private readonly ModFlags flags;
    private readonly ModEnvironment environment;

    internal ModuleManager(ILogger logger, ILoggerFactory factory)
    {
        Instance = this;
        flags = new ModFlags(RiseCore.IsWindows, RiseCore.IsSteam);
        environment = new ModEnvironment(
            RiseCore.FortRiseVersion,
            RiseCore.GameRootPath,
            AppDomain.CurrentDomain.BaseDirectory
        );
        this.logger = logger;
        loggerFactory = factory;

        proxyManager = new ProxyManager<string>((proxyInfo) =>
        {
            var key = (proxyInfo.Target.Context, proxyInfo.Proxy.Context);

            ref var moduleBuilder = ref CollectionsMarshal.GetValueRefOrAddDefault(proxyModuleBuilders, key, out bool exists);

            if (!exists)
            {
                string proxyAsmName =
                $"{GetType().Namespace}.Proxies{Guid.CreateVersion7()}, Version={GetType().Assembly.GetName().Version}, Culture=neutral";
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
        var mods = new List<ModuleMetadata>();
        var modDirectory = modPath;

        var directories = Directory.GetDirectories(modDirectory);
        ref var dir = ref MemoryMarshal.GetArrayDataReference(directories);
        ref var end = ref Unsafe.Add(ref dir, directories.Length);

        while (Unsafe.IsAddressLessThan(ref dir, ref end))
        {
            if (dir.Contains("_RelinkerCache"))
            {
                dir = ref Unsafe.Add(ref dir, 1);
                continue;
            }

            var dirInfo = new DirectoryInfo(dir);
            if (BlacklistedMods != null && BlacklistedMods.Contains(dirInfo.Name))
            {
                logger.LogDebug("Ignored '{dir}' as it is blacklisted.", dir);
                dir = ref Unsafe.Add(ref dir, 1);
                continue;
            }

            var meta = LoadDir(dir);
            if (meta is not null)
            {
                mods.Add(meta);
            }

            dir = ref Unsafe.Add(ref dir, 1);
        }

        var files = Directory.GetFiles(modDirectory);
        ref var file = ref MemoryMarshal.GetArrayDataReference(files);
        end = ref Unsafe.Add(ref file, files.Length);

        while (Unsafe.IsAddressLessThan(ref file, ref end))
        {
            if (!file.EndsWith("zip"))
            {
                file = ref Unsafe.Add(ref file, 1);
                continue;
            }

            var fileName = Path.GetFileName(file);
            if (BlacklistedMods != null && BlacklistedMods.Contains(Path.GetFileName(fileName)))
            {
                logger.LogDebug("Ignored '{file}' as it is blacklisted.", file);
                file = ref Unsafe.Add(ref file, 1);
                continue;
            }

            var meta = LoadZip(file);
            if (meta is not null)
            {
                mods.AddRange(meta);
            }

            file = ref Unsafe.Add(ref file, 1);
        }

        mods.Sort(new ModOrder());
        LoadMods(mods);
    }

    private static ModuleMetadata LoadDir(string dir)
    {
        var metaPath = Path.Combine(dir, "meta.json");
        if (!File.Exists(metaPath))
        {
            return null;
        }

        var result = ModuleMetadata.ParseMetadata(dir, metaPath);
        if (!result.Check(out ModuleMetadata moduleMetadata, out string error))
        {
            ErrorPanel.StoreError(error);
            Logger.Error(error);
            return null;
        }

        return moduleMetadata;
    }

    private static List<ModuleMetadata> LoadZip(string file)
    {
        var modules = new List<ModuleMetadata>();
        using var zipFile = ZipFile.OpenRead(file);

        foreach (var entry in zipFile.Entries)
        {
            var normalizedPath = entry.FullName.Replace('\\', '/');

            if (IsMetaFile(normalizedPath))
            {
                int lastSlashIndex = normalizedPath.LastIndexOf('/');
                var rootZip = lastSlashIndex >= 0 ? normalizedPath[..lastSlashIndex] : string.Empty;

                using var memStream = entry.ExtractStream();
                var result = ModuleMetadata.ParseMetadata(file, memStream, true);

                if (result.Check(out var metadata, out string error))
                {
                    metadata.RootZip = rootZip;
                    modules.Add(metadata);
                }
                else 
                {
                    ErrorPanel.StoreError(error);
                    RiseCore.logger.LogError("{error}", error);
                }
            }
        }

        return modules;

        static bool IsMetaFile(string path)
        {
            return path.Equals("meta.json") || path.EndsWith("/meta.json");
        }
    }

    private static Dictionary<string, List<ModuleMetadata>> CreateDependencyGraph(List<ModuleMetadata> mods)
    {
        Dictionary<string, List<ModuleMetadata>> dependencyGraph = [];
        foreach (var mod in mods)
        {
            if (mod.Dependencies is not null)
            {
                foreach (var dep in mod.Dependencies)
                {
                    ref var graph = ref CollectionsMarshal.GetValueRefOrAddDefault(dependencyGraph, mod.Name, out bool exists);
                    if (!exists)
                    {
                        graph = [];
                    }

                    graph.Add(dep);
                }
            }

            if (mod.OptionalDependencies is not null)
            {
                foreach (var dep in mod.OptionalDependencies)
                {
                    ref var graph = ref CollectionsMarshal.GetValueRefOrAddDefault(dependencyGraph, mod.Name, out bool exists);
                    if (!exists)
                    {
                        graph = [];
                    }

                    graph.Add(dep);
                }
            }
        }

        return dependencyGraph;
    }

    private void LoadMods(List<ModuleMetadata> mods)
    {
        // create a dependency graph
        var dependencyGraph = CreateDependencyGraph(mods);
        Dictionary<string, List<ModuleMetadata>> toLoadAfter = [];


        RiseCore.logger.LogDebug("Dependency Graph: ");
        foreach (var graph in dependencyGraph)
        {
            RiseCore.logger.LogDebug("{modName}", graph.Key);
            foreach (var dep in graph.Value)
            {
                RiseCore.logger.LogDebug("- {name}::{version}", dep.Name, dep.Version);
            }
        }

        var modSpan = CollectionsMarshal.AsSpan(mods);

        for (int i = modSpan.Length - 1; i >= 0; i -= 1)
        {
            var mod = modSpan[i];
            if (!LoadMod(mod, mods, dependencyGraph, toLoadAfter, false).Check(out _, out LoadError err))
            {
                if (err is LoadError.Failure)
                {
                    mods.RemoveAt(i);
                }
                continue;
            }

            mods.RemoveAt(i);
        }

        modSpan = CollectionsMarshal.AsSpan(mods);

        // excess mod to load after
        for (int i = modSpan.Length - 1; i >= 0; i -= 1)
        {
            var mod = modSpan[i];
            if (!LoadMod(mod, mods, dependencyGraph, toLoadAfter, true).Check(out _, out LoadError err))
            {
                if (err is LoadError.Delayed)
                {
                    continue;
                }
            }

            mods.RemoveAt(i);
        }

        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int i = mods.Count - 1; i >= 0; i -= 1)
            {
                if (IsBlockedOnlyByInactiveMods(mods[i]))
                {
                    RegisterInactive(mods[i]);
                    mods.RemoveAt(i);
                    changed = true;
                }
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

            if (!IsActive(internalMetadata.Name))
            {
                return false;
            }

            if (metadata.Version.IsSatisfiedBy(internalMetadata.Version))
            {
                return true;
            }

            if (!storeError)
            {
                return false;
            }

            string op = "^";
            switch (metadata.Version.Operator)
            {
                case "=":
                    ErrorPanel.StoreError($"Dependency Version Mismatched {metadata.Name} {metadata.Version} != {internalMetadata.Version}");
                    logger.LogError(
                        "Dependency Version Mismatched {modName} {modVersion} != {targetModVersion}",
                        metadata.Name,
                        metadata.Version,
                        internalMetadata.Version
                    );
                    return false;
                case ">":
                    op = "<";
                    break;
                case ">=":
                    op = "<=";
                    break;
                case "<":
                case "<=":
                    op = metadata.Version.Operator;
                    ErrorPanel.StoreError($"Possible mod instability with incompatible range {metadata.Name} {metadata.Version} {op} {internalMetadata.Version}");
                    logger.LogError(
                        "Possible mod instability with incompatible range {modName} {modVersion} {op} {targetModVersion}",
                        metadata.Name,
                        metadata.Version,
                        op,
                        internalMetadata.Version
                    );
                    return false;
                case "~":
                    ErrorPanel.StoreError($"Outdated Minor Dependency {metadata.Name} {metadata.Version} < {internalMetadata.Version}");
                    logger.LogError(
                        "Outdated Minor Dependency {modName} {modVersion} < {targetModVersion}",
                        metadata.Name,
                        metadata.Version,
                        internalMetadata.Version
                    );
                    return false;
                case "^":
                    ErrorPanel.StoreError($"Outdated Major Dependency {metadata.Name} {metadata.Version} < {internalMetadata.Version}");
                    logger.LogError(
                        "Outdated Major Dependency {modName} {modVersion} < {targetModVersion}",
                        metadata.Name,
                        metadata.Version,
                        internalMetadata.Version
                    );
                    return false;
                default:
                    break;
            }

            ErrorPanel.StoreError($"Outdated Dependency {metadata.Name} {metadata.Version} {op} {internalMetadata.Version}");
            logger.LogError(
                "Outdated Dependency {modName} {modVersion} {op} {targetModVersion}",
                metadata.Name,
                metadata.Version,
                op,
                internalMetadata.Version
            );
            return false;
        }

        return false;
    }

    public Result<Unit, LoadError> LoadMod(
        ModuleMetadata metadata, 
        List<ModuleMetadata> mods,
        Dictionary<string, List<ModuleMetadata>> dependencyGraph,
        Dictionary<string, List<ModuleMetadata>> toLoadAfter,
        bool ignoreOptional
    )
    {
        if (!IsProtected(metadata) && DisabledMods.Contains(metadata.Name))
        {
            logger.LogInformation("'{modName}' is disabled, skipping.", metadata.Name);
            RegisterInactive(metadata);
            return LoadError.Failure;
        }

        foreach (var dep in metadata.Dependencies)
        {
            if (!CheckDependencyMetadata(dep, true))
            {
                ref var graph = ref CollectionsMarshal.GetValueRefOrAddDefault(toLoadAfter, dep.Name, out bool exists);
                if (!exists)
                {
                    graph = [];
                }

                graph.Add(metadata);
                return LoadError.Delayed;
            }

            if (dependencyGraph.TryGetValue(metadata.Name, out var list))
            {
                list.Remove(dep);
            }
        }

        if (!ignoreOptional && metadata.OptionalDependencies is not null)
        {
            foreach (var dep in metadata.OptionalDependencies)
            {
                if (!CheckDependencyMetadata(dep, false))
                {
                    ref var graph = ref CollectionsMarshal.GetValueRefOrAddDefault(toLoadAfter, dep.Name, out bool exists);
                    if (!exists)
                    {
                        graph = [];
                    }

                    graph.Add(metadata);
                    return LoadError.Delayed;
                }


                if (dependencyGraph.TryGetValue(metadata.Name, out var list))
                {
                    list.Remove(dep);
                }
            }
        }


        if (!LoadModSkipDependecies(metadata).Check(out var ok, out var error))
        {
            return error;
        }

        if (toLoadAfter.TryGetValue(metadata.Name, out var toLoad))
        {
            // HACK: using for loop instead of foreach to prevent crashes
            // probably, might investigate why? :/
            for (int i = 0; i < toLoad.Count; i += 1)
            {
                var mod = toLoad[i];

                if (!LoadMod(mod, mods, dependencyGraph, toLoadAfter, false).Check(out _, out _))
                {
                    continue;
                }

                mods.Remove(mod);
            }

            toLoadAfter.Remove(metadata.Name);
        }

        return ok;
    }

    public Result<Unit, LoadError> LoadModSkipDependecies(ModuleMetadata metadata)
    {
        if (ActiveMods.TryGetValue(metadata.Name, out bool active) && active)
        {
            logger.LogWarning("'{modName}' is already loaded, skipping.", metadata.Name);
            return LoadError.Failure;
        }

        Assembly asm = null;
        IModContent content = new ModContent(metadata);
        IModResource modResource;
        if (!string.IsNullOrEmpty(metadata.PathZip))
        {
            if (!string.IsNullOrEmpty(metadata.RootZip))
            {
                modResource = new ZipModResource(metadata, content, metadata.RootZip);
            }
            else 
            {
                modResource = new ZipModResource(metadata, content);
            }
        }
        else if (!string.IsNullOrEmpty(metadata.PathDirectory))
        {
            modResource = new FolderModResource(metadata, content);
        }
        else
        {
            logger.LogError("Mod named: '{modName}' not found!", metadata.Name);
            ErrorPanel.StoreError($"'{metadata.Name}' not found!");
            return LoadError.Failure;
        }

        // first load decides the position, reloads keep it
        if (loadOrderIndex.TryAdd(metadata.Name, loadOrder.Count))
        {
            loadOrder.Add(metadata);
        }

        ResourceTree.AddMod(metadata, modResource);

        if (!string.IsNullOrEmpty(metadata.DLL))
        {
            metadata.AssemblyLoadContext = new ModAssemblyLoadContext(metadata);

            using var stream = modResource.GetResource(metadata.DLL)?.Stream
                ?? throw new FileNotFoundException($"{metadata.DLL} not found on mod: '{metadata}'");

            asm = Resolver.LoadModAssembly(modResource, metadata.DLL, stream);
        }

        NameToMod[metadata.Name] = modResource;
        InsertInLoadOrder(InternalMods, modResource, static x => x.Metadata);
        InternalModuleMetadatas.Add(metadata);

        if (asm != null)
        {
            LoadAssembly(metadata, content, asm);
        }
        else
        {
            // for content mods that does not have C# Mod class
            var logger = loggerFactory.CreateLogger(metadata.Name);
            var context = GetModuleContext(metadata, logger);
            EventsManager.ModBeforeInstantiation.Raise(null, new BeforeModInstantiationEventArgs(content, context));

            logger.LogInformation("{modName} {modVersion} has been loaded.", metadata.Name, metadata.Version);
        }

        ActiveMods[metadata.Name] = true;

        if (State == LoadState.Ready)
        {
            WatchForHotReload(metadata);
        }

        return new Unit();
    }

    private Mod LoadAssembly(ModuleMetadata metadata, IModContent content, Assembly asm)
    {
        foreach (var t in asm.GetTypes())
        {
            Mod mod;
            if (t.BaseType == typeof(Mod))
            {
                var logger = loggerFactory.CreateLogger(metadata.Name);
                var context = GetModuleContext(metadata, logger);
                EventsManager.ModBeforeInstantiation.Raise(null, new BeforeModInstantiationEventArgs(content, context));
                mod = Activator.CreateInstance(t, [content, context, logger]) as Mod;
            }
            else
            {
                continue;
            }


            mod.Meta = metadata;
            mod.ParseArgs(RiseCore.ApplicationArgs);
            mod.OnLoad?.Invoke(mod.Context);


            InsertInLoadOrder(InternalFortModules, mod, static x => x.Meta);
            NameToFortModule[metadata.Name] = mod;

            logger.LogInformation("{modName} {modVersion} has been loaded.", mod.Meta.Name, mod.Meta.Version);

            return mod;
        }

        return null;
    }

    private ModuleContext GetModuleContext(ModuleMetadata metadata, ILogger logger)
    {
        return new ModuleContext(
            AddOrGetRegistry(metadata, logger),
            new ModInterop(this, metadata, proxyManager),
            new ModEvents(metadata, EventsManager),
            flags,
            new ModStorage(metadata),
            environment,
            logger,
            new NotificationHandler(metadata),
            new LimitedHarmony(new Harmony(metadata.Name)
        ));
    }

    internal void Initialize()
    {
        State = LoadState.Initialize;

        foreach (var batch in registryBatch[RegistryBatchType.Initialization])
        {
            batch.Invoke();
        }

        foreach (var fortModule in InternalFortModules)
        {
            fortModule.OnInitialize?.Invoke(fortModule.Context);
            EventsManager.ModInitialize.Raise(fortModule, fortModule.Meta);
        }

        EventsManager.ModLoadStateFinished.Raise(null, LoadState.Initialize);

        LogPatches();
    }

    internal Mod CreateFortRiseModule()
    {
        var fortRiseMetadata = new ModuleMetadata()
        {
            Name = "FortRise",
            Version = RiseCore.FortRiseVersion,
        };

        var module = new FortRiseModule(new ModContent(fortRiseMetadata), GetModuleContext(fortRiseMetadata, logger), logger);
        InternalFortModules.Add(module);
        InternalModuleMetadatas.Add(module.Meta);

        return module;
    }

#nullable enable
    internal ILogger? GetModLogger(string modName) 
    {
        foreach (var mod in InternalFortModules)
        {
            if (mod.Meta.Name == modName)
            {
                return mod.Logger;
            }
        }

        return null;
    }

    public bool IsModLoaded(ModuleMetadata metadata)
    {
        if (metadata.Name.StartsWith("FortRise"))
        {
            return true;
        }

        return IsActive(metadata.Name);
    }

    private bool IsActive(string modName)
    {
        if (ActiveMods.TryGetValue(modName, out bool active))
        {
            return active;
        }

        // FortRise itself is never part of ActiveMods
        return modName.StartsWith("FortRise");
    }

    private static bool IsProtected(ModuleMetadata metadata) => metadata.Name.StartsWith("FortRise");

    public void UnloadMod(ModuleMetadata metadata) => SetModsActive([], [metadata]);

    public void EnableMod(ModuleMetadata metadata) => SetModsActive([metadata], []);

    public void SetModsActive(IEnumerable<ModuleMetadata> toEnable, IEnumerable<ModuleMetadata> toDisable)
    {
        if (State != LoadState.Ready)
        {
            logger.LogWarning("Mods can only be loaded or unloaded at runtime once the game is ready.");
            return;
        }

        HashSet<string> disable = [];
        List<ModuleMetadata> roots = [];

        foreach (var metadata in toDisable)
        {
            if (!IsProtected(metadata) && IsActive(metadata.Name) && disable.Add(metadata.Name))
            {
                roots.Add(metadata);
            }
        }

        List<ModuleMetadata> enable = [];
        foreach (var metadata in toEnable)
        {
            if (!IsActive(metadata.Name) && !disable.Contains(metadata.Name))
            {
                enable.Add(metadata);
                roots.Add(metadata);
            }
        }

        if (roots.Count == 0)
        {
            return;
        }

        DisabledMods.UnionWith(disable);
        foreach (var metadata in enable)
        {
            DisabledMods.Remove(metadata.Name);
        }
        SaveDisabledMods();

        // Every active mod that depends on a changed mod was loaded against the old state, so rewind it too.
        var rewind = CollectActiveDependents(roots);
        UnloadInReverseOrder(rewind);
        CollectGarbage();

        List<ModuleMetadata> toLoad = [.. enable];
        foreach (var metadata in rewind)
        {
            if (!disable.Contains(metadata.Name))
            {
                toLoad.Add(metadata);
            }
        }

        toLoad.AddRange(CollectInactiveDependents(enable));

        LoadInOrder(toLoad);
        RunPendingRefreshes();
    }

    private readonly Dictionary<string, Action> pendingRefreshes = [];

    internal void RequestRefresh(string key, Action refresh)
    {
        if (State == LoadState.Ready)
        {
            pendingRefreshes[key] = refresh;
        }
    }

    private void RunPendingRefreshes()
    {
        foreach (var (key, refresh) in pendingRefreshes)
        {
            try
            {
                refresh();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to refresh '{key}' after loading or unloading mods.", key);
            }
        }

        pendingRefreshes.Clear();
    }

    private void RegisterInactive(ModuleMetadata metadata)
    {
        if (loadOrderIndex.TryAdd(metadata.Name, loadOrder.Count))
        {
            loadOrder.Add(metadata);
        }

        InternalModuleMetadatas.Add(metadata);
        ActiveMods[metadata.Name] = false;
    }

    private bool IsBlockedOnlyByInactiveMods(ModuleMetadata metadata)
    {
        if (metadata.Dependencies is null)
        {
            return false;
        }

        bool blockedByInactive = false;
        foreach (var dep in metadata.Dependencies)
        {
            if (!InternalModuleMetadatas.Any(x => x.Name == dep.Name))
            {
                return false;
            }

            if (!IsActive(dep.Name))
            {
                blockedByInactive = true;
            }
        }

        return blockedByInactive;
    }

    private List<ModuleMetadata> CollectInactiveDependents(List<ModuleMetadata> enabled)
    {
        List<ModuleMetadata> result = [];
        HashSet<string> visited = [.. enabled.Select(x => x.Name)];
        Stack<ModuleMetadata> stack = new(enabled);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            foreach (var other in loadOrder)
            {
                if (visited.Contains(other.Name) || IsActive(other.Name) || IsProtected(other)
                    || DisabledMods.Contains(other.Name) || !IsModDepends(other, current))
                {
                    continue;
                }

                visited.Add(other.Name);
                result.Add(other);
                stack.Push(other);
            }
        }

        return result;
    }

    private void SaveDisabledMods()
    {
        if (DisabledModsPath is null)
        {
            return;
        }

        try
        {
            List<string> names = [.. DisabledMods.Order()];
            File.WriteAllText(DisabledModsPath, JsonSerializer.Serialize(names));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not write the disabled mods to '{path}'.", DisabledModsPath);
        }
    }

    private readonly ConcurrentDictionary<string, (ModuleMetadata Metadata, DateTime LastChange)> pendingHotReloads = new();
    private static readonly TimeSpan HotReloadSettleTime = TimeSpan.FromMilliseconds(500);

    internal void QueueHotReload(ModuleMetadata metadata)
    {
        pendingHotReloads[metadata.Name] = (metadata, DateTime.UtcNow);
    }

    internal void UpdateHotReload()
    {
        if (pendingHotReloads.IsEmpty || State != LoadState.Ready)
        {
            return;
        }

        List<KeyValuePair<string, (ModuleMetadata Metadata, DateTime LastChange)>> ready = [];
        foreach (var pending in pendingHotReloads)
        {
            var (metadata, lastChange) = pending.Value;
            if (DateTime.UtcNow - lastChange >= HotReloadSettleTime && IsDllReadable(metadata))
            {
                ready.Add(pending);
            }
        }

        foreach (var pending in ready)
        {
            if (!pendingHotReloads.TryRemove(pending))
            {
                continue;
            }

            var metadata = pending.Value.Metadata;
            logger.LogInformation("{modName} {modVersion} hot reloading.", metadata.Name, metadata.Version);
            ReloadMod(metadata);
        }
    }

    private readonly Dictionary<string, FileSystemWatcher> hotReloadWatchers = [];

    private bool ShouldHotReload(ModuleMetadata metadata)
    {
        if (IsProtected(metadata) || !string.IsNullOrEmpty(metadata.PathZip)
            || string.IsNullOrEmpty(metadata.PathDirectory) || !Directory.Exists(metadata.PathDirectory))
        {
            return false;
        }

        if (RiseCore.HotReloadMods)
        {
            return true;
        }

#pragma warning disable FR001
        return NameToFortModule.TryGetValue(metadata.Name, out var mod) && mod.HotReload;
#pragma warning restore FR001
    }

    internal void StartHotReloadWatchers()
    {
        foreach (var metadata in loadOrder)
        {
            if (IsActive(metadata.Name))
            {
                WatchForHotReload(metadata);
            }
        }
    }

    private void WatchForHotReload(ModuleMetadata metadata)
    {
        if (hotReloadWatchers.ContainsKey(metadata.Name) || !ShouldHotReload(metadata))
        {
            return;
        }

        var watcher = new FileSystemWatcher(metadata.PathDirectory)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
                | NotifyFilters.Size | NotifyFilters.CreationTime,
        };

        void OnChange(object _, FileSystemEventArgs e)
        {
            if (!IsIgnoredHotReloadChange(e.FullPath))
            {
                QueueHotReload(metadata);
            }
        }
        watcher.Changed += OnChange;
        watcher.Created += OnChange;
        watcher.Deleted += OnChange;
        watcher.Renamed += (_, e) =>
        {
            if (!IsIgnoredHotReloadChange(e.FullPath) || !IsIgnoredHotReloadChange(e.OldFullPath))
            {
                QueueHotReload(metadata);
            }
        };
        watcher.Error += (_, _) => QueueHotReload(metadata);

        watcher.EnableRaisingEvents = true;
        hotReloadWatchers[metadata.Name] = watcher;

        logger.LogInformation("Watching '{modName}' for hot reload.", metadata.Name);
    }

    private void StopWatchingForHotReload(string modName)
    {
        if (hotReloadWatchers.Remove(modName, out var watcher))
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
    }

    private static bool IsIgnoredHotReloadChange(string path)
    {
        var normalized = path.Replace('\\', '/');
        if (normalized.Contains("/.git/") || normalized.EndsWith("/.git"))
        {
            return true;
        }

        var name = Path.GetFileName(normalized);
        return name.EndsWith('~') || name.StartsWith(".#")
            || name.EndsWith(".swp") || name.EndsWith(".swx") || name.EndsWith(".swo")
            || name.EndsWith(".tmp");
    }

    private static bool IsDllReadable(ModuleMetadata metadata)
    {
        // content mods have nothing to wait on
        if (string.IsNullOrEmpty(metadata.DLL))
        {
            return true;
        }

        try
        {
            using var stream = File.Open(Path.Combine(metadata.PathDirectory, metadata.DLL), FileMode.Open, FileAccess.Read, FileShare.Read);
            return stream.Length > 0;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    public void ReloadMod(ModuleMetadata metadata)
    {
        if (State != LoadState.Ready || IsProtected(metadata) || !IsActive(metadata.Name))
        {
            return;
        }

        var rewind = CollectActiveDependents([metadata]);
        LevelHotReload.BeforeUnload(rewind);

        IDReservationPool = new IndexReservationPool(rewind.Select(x => x.Name));
        try
        {
            UnloadInReverseOrder(rewind);
            CollectGarbage();
            LoadInOrder(rewind);
        }
        finally
        {
            var session = IDReservationPool;
            IDReservationPool = null;
            session.ReleaseUnclaimed((step, release) => TryUnloadStep(metadata, step, release));
        }

        RunPendingRefreshes();
        LevelHotReload.AfterReload();
    }

    internal IndexReservationPool? IDReservationPool { get; private set; }

    internal void UnloadAll()
    {
        List<ModuleMetadata> active = [];
        foreach (var metadata in loadOrder)
        {
            if (!IsProtected(metadata) && IsActive(metadata.Name))
            {
                active.Add(metadata);
            }
        }

        UnloadInReverseOrder(active);
    }

    private List<ModuleMetadata> CollectActiveDependents(IEnumerable<ModuleMetadata> roots)
    {
        List<ModuleMetadata> result = [];
        HashSet<string> visited = [];
        Stack<ModuleMetadata> stack = new(roots);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!visited.Add(current.Name))
            {
                continue;
            }

            if (!IsProtected(current) && IsActive(current.Name))
            {
                result.Add(current);
            }

            foreach (var other in loadOrder)
            {
                if (!visited.Contains(other.Name) && IsActive(other.Name) && IsModDepends(other, current))
                {
                    stack.Push(other);
                }
            }
        }

        return result;
    }

    private void UnloadInReverseOrder(List<ModuleMetadata> mods)
    {
        mods.Sort((x, y) => GetLoadOrderIndex(y).CompareTo(GetLoadOrderIndex(x)));
        foreach (var metadata in mods)
        {
            UnloadModCore(metadata);
        }
    }

    private void LoadInOrder(List<ModuleMetadata> mods)
    {
        List<ModuleMetadata> pending = [.. mods.OrderBy(x => loadOrderIndex.TryGetValue(x.Name, out int i) ? i : int.MaxValue)];
        List<ModuleMetadata> loaded = [];

        while (pending.Count > 0)
        {
            int next = pending.FindIndex(AreDependenciesActive);
            if (next < 0)
            {
                break;
            }

            var metadata = pending[next];
            pending.RemoveAt(next);

            if (LoadModSkipDependecies(metadata).Check(out _, out _))
            {
                loaded.Add(metadata);
            }
        }

        foreach (var metadata in pending)
        {
            logger.LogWarning("'{modName}' was not loaded, one or more dependencies are not loaded.", metadata.Name);
        }

        ReplayBootEvents(loaded);
    }

    private void ReplayBootEvents(List<ModuleMetadata> loaded)
    {
        if (loaded.Count == 0)
        {
            return;
        }

        var events = EventsManager;

        foreach (var metadata in loaded)
        {
            events.ModLoadStateFinished.RaiseFor(metadata, null, LoadState.Load);
        }

        foreach (var metadata in loaded)
        {
            if (!NameToIcon.ContainsKey(metadata.Name) && GetMod(metadata.Name) is { } resource)
            {
                LoadIcon(resource);
            }
        }

        var dataLoadArgs = new DataLoadEventArgs(false);
        foreach (var metadata in loaded)
        {
            events.OnBeforeDataLoad.RaiseFor(metadata, null, dataLoadArgs);
        }

        foreach (var metadata in loaded)
        {
            events.OnAfterDataLoad.RaiseFor(metadata, null, dataLoadArgs);
        }

        var game = (TowerFall.TFGame)Engine.Instance;
        foreach (var metadata in loaded)
        {
            events.GameInitialized.RaiseFor(metadata, game, game);
        }

        foreach (var metadata in loaded)
        {
            if (NameToFortModule.TryGetValue(metadata.Name, out var mod))
            {
                mod.OnInitialize?.Invoke(mod.Context);
                events.ModInitialize.Raise(mod, mod.Meta);
            }
        }

        foreach (var metadata in loaded)
        {
            events.ModLoadStateFinished.RaiseFor(metadata, null, LoadState.Initialize);
        }

        foreach (var metadata in loaded)
        {
            events.ModLoadStateFinished.RaiseFor(metadata, null, LoadState.Ready);
        }

        if (Engine.Instance.Scene is TowerFall.MainMenu menu)
        {
            var menuArgs = new MenuLoadedEventArgs(menu, false);
            foreach (var metadata in loaded)
            {
                events.GameLoaded.RaiseFor(metadata, null, menuArgs);
            }
        }
    }

    internal void LoadIcon(IModResource mod)
    {
        if (!mod.OwnedResources.TryGetValue("icon.png", out var img))
        {
            return;
        }

        using var png = img.Stream;
        var texture = new Monocle.Texture(
            Microsoft.Xna.Framework.Graphics.Texture2D.FromStream(Engine.Instance.GraphicsDevice, png));

        NameToIcon[mod.Metadata.Name] = new Subtexture(texture);
    }

    private bool AreDependenciesActive(ModuleMetadata metadata)
    {
        if (metadata.Dependencies is null)
        {
            return true;
        }

        foreach (var dep in metadata.Dependencies)
        {
            if (!CheckDependencyMetadata(dep, false))
            {
                return false;
            }
        }

        return true;
    }

    private void UnloadModCore(ModuleMetadata metadata)
    {
        NameToFortModule.TryGetValue(metadata.Name, out var mod);
        var resource = GetMod(metadata.Name);

        if (mod is not null)
        {
            TryUnloadStep(metadata, "unpatching", () => mod.Context.Harmony.UnpatchAll());
            TryUnloadStep(metadata, "OnUnload", () => mod.OnUnload?.Invoke(mod.Context));
            StopWatchingForHotReload(metadata.Name);
        }

        if (modUnloaders.Remove(metadata.Name, out var unloaders))
        {
            foreach (var unload in unloaders)
            {
                TryUnloadStep(metadata, "detaching registry entries", unload.Unload);
            }
        }

        if (modQueues.Remove(metadata.Name, out var queues))
        {
            foreach (var (batchType, queue) in queues)
            {
                if (registryBatch.TryGetValue(batchType, out var batch))
                {
                    batch.Remove(queue);
                }
            }
        }

        registries.Remove(metadata.Name);

        ModEventsManager.Instance.RemoveByMetadata(metadata);

        if (resource is not null)
        {
            ResourceTree.RemoveMod(metadata, resource);
            InternalMods.Remove(resource);
        }

        NameToMod.Remove(metadata.Name);
        NameToFortModule.Remove(metadata.Name);
        if (mod is not null)
        {
            InternalFortModules.Remove(mod);
        }

        ActiveMods[metadata.Name] = false;

        List<(string modTargetName, string modRequesterName)> toRemove = [];

        foreach (var key in proxyModuleBuilders.Keys)
        {
            if (key.modRequesterName == metadata.Name || key.modTargetName == metadata.Name)
            {
                toRemove.Add(key);
            }
        }

        foreach (var proxyCacheRemove in toRemove)
        {
            proxyModuleBuilders.Remove(proxyCacheRemove);
        }

        PintailCache.RemoveMod(proxyManager, metadata.Name, logger);

        if (metadata.AssemblyLoadContext is not null)
        {
            metadata.AssemblyLoadContext.Dispose();
            metadata.AssemblyLoadContext.Unload();
            metadata.AssemblyLoadContext = null;
        }

        logger.LogInformation("{modName} {modVersion} has been unloaded.", metadata.Name, metadata.Version);
    }

    private void TryUnloadStep(ModuleMetadata metadata, string step, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "'{modName}' failed while {step}, continuing the unload.", metadata.Name, step);
            ErrorPanel.StoreError($"'{metadata.Name}' failed while {step}: {ex.Message}");
        }
    }

    private static void CollectGarbage()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private int GetLoadOrderIndex(ModuleMetadata metadata)
    {
        return metadata is not null && loadOrderIndex.TryGetValue(metadata.Name, out int index) ? index : -1;
    }

    private void InsertInLoadOrder<T>(List<T> list, T item, Func<T, ModuleMetadata> getMetadata)
    {
        int index = GetLoadOrderIndex(getMetadata(item));
        int at = list.Count;

        for (int i = 0; i < list.Count; i += 1)
        {
            if (GetLoadOrderIndex(getMetadata(list[i])) > index)
            {
                at = i;
                break;
            }
        }

        list.Insert(at, item);
    }

    internal static bool IsModDepends(ModuleMetadata mod, ModuleMetadata targetMod)
    {
        if (mod.Dependencies is null)
        {
            return false;
        }

        foreach (var dependent in mod.Dependencies)
        {
            if (dependent.Name == targetMod.Name)
            {
                return true;
            }
        }

        if (mod.OptionalDependencies != null)
        {
            foreach (var dependent in mod.OptionalDependencies)
            {
                if (dependent.Name == targetMod.Name)
                {
                    return true;
                }
            }
        }


        return false;
    }

    internal IReadOnlyList<IModResource> GetModDependents(string modName)
    {
        List<IModResource> list = [];
        foreach (var mod in InternalMods)
        {
            var dependencies = mod.Metadata.Dependencies;
            for (int i = 0; i < dependencies?.Length; i++)
            {
                var dependency = dependencies[i];
                if (dependency.Name == modName)
                {
                    list.Add(mod);
                    break;
                }
            }

            if (mod.Metadata.OptionalDependencies != null)
            {
                var optDependencies = mod.Metadata.OptionalDependencies;
                for (int i = 0; i < optDependencies.Length; i++)
                {
                    var dependency = optDependencies[i];
                    if (dependency.Name == modName)
                    {
                        list.Add(mod);
                        break;
                    }
                }
            }
        }

        return list;
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
    internal IModRegistry AddOrGetRegistry(ModuleMetadata metadata) => AddOrGetRegistry(metadata, GetModLogger(metadata.Name));

    internal IModRegistry AddOrGetRegistry(ModuleMetadata metadata, ILogger logger)
    {
        ref var registry = ref CollectionsMarshal.GetValueRefOrAddDefault(registries, metadata.Name, out bool exists);
        if (!exists)
        {
            registryOwner = metadata.Name;
            try
            {
                registry = new ModRegistry(metadata, this, logger);
            }
            finally
            {
                registryOwner = null;
            }
        }

        return registry;
    }

    private void TrackQueue(RegistryBatchType batchType, RegistryQueue queue)
    {
        if (registryOwner is null)
        {
            return;
        }

        ref var queues = ref CollectionsMarshal.GetValueRefOrAddDefault(modQueues, registryOwner, out bool exists);
        if (!exists)
        {
            queues = [];
        }

        queues.Add((batchType, queue));
    }

    internal PreparedRegistryQueue<T> CreatePrepareQueue<T>(Action<List<T>> prepare, Action<T> invoker)
    where T : class
    {
        return CreatePrepareQueue(prepare, invoker, RegistryBatchType.Initialization);
    }

    internal PreparedRegistryQueue<T> CreatePrepareQueue<T>(Action<List<T>> prepare, Action<T> invoker, RegistryBatchType batchType)
    where T : class
    {
        ref var col = ref CollectionsMarshal.GetValueRefOrAddDefault(registryBatch, batchType, out bool exists);

        if (!exists)
        {
            col = [];
        }

        var registryQueue = new PreparedRegistryQueue<T>(this, prepare, invoker);
        col.Add(registryQueue);
        TrackQueue(batchType, registryQueue);

        return registryQueue;
    }

    internal RegistryQueue<T> CreateQueue<T>(Action<T> invoker)
    where T : class
    {
        return CreateQueue(invoker, RegistryBatchType.Initialization);
    }

    internal RegistryQueue<T> CreateQueue<T>(Action<T> invoker, RegistryBatchType batchType)
    where T : class
    {
        ref var col = ref CollectionsMarshal.GetValueRefOrAddDefault(registryBatch, batchType, out bool exists);

        if (!exists)
        {
            col = [];
        }

        var registryQueue = new RegistryQueue<T>(this, invoker);
        col.Add(registryQueue);
        TrackQueue(batchType, registryQueue);

        return registryQueue;
    }

    internal RegistryQueue<T> CreateQueue<T>(ModuleMetadata metadata, Action<T> invoker, Action<T> detach)
    where T : class
    {
        return CreateQueue(metadata, invoker, detach, RegistryBatchType.Initialization);
    }

    internal RegistryQueue<T> CreateQueue<T>(ModuleMetadata metadata, Action<T> invoker, Action<T> detach, RegistryBatchType batchType)
    where T : class
    {
        ref var col = ref CollectionsMarshal.GetValueRefOrAddDefault(registryBatch, batchType, out bool exists);
        ref var modBatch = ref CollectionsMarshal.GetValueRefOrAddDefault(modUnloaders, metadata.Name, out bool modExists);

        if (!exists)
        {
            col = [];
        }

        if (!modExists)
        {
            modBatch = [];
        }

        var registryQueue = new RegistryQueue<T>(this, invoker);
        col.Add(registryQueue);
        TrackQueue(batchType, registryQueue);
        modBatch.Add(new RegistryUnloader<T>(detach, registryQueue));

        return registryQueue;
    }

    private void LogPatches()
    {
        StringBuilder builder = new StringBuilder("\n");

        var patchedMethods = Harmony.GetAllPatchedMethods();
        foreach (var method in patchedMethods)
        {
            string methodFullName = $"{method.DeclaringType.FullName}/{method.Name}";
            builder.AppendLine("\t" + methodFullName);
            Dictionary<string, PatchInfo> infos = [];
            var patches = Harmony.GetPatchInfo(method);

            foreach (var owner in patches.Owners)
            {
                if (owner is null)
                {
                    continue;
                }

                infos[owner] = new PatchInfo();
            }

            foreach (var prefix in patches.Prefixes)
            {
                ref var patchInfo = ref CollectionsMarshal.GetValueRefOrNullRef(infos, prefix.owner);
                if (Unsafe.IsNullRef(ref patchInfo))
                {
                    continue;
                }

                patchInfo.Prefix = prefix.PatchMethod.ReturnType != typeof(bool);
                patchInfo.SkippingPrefix = prefix.PatchMethod.ReturnType == typeof(bool);
            }

            foreach (var postfix in patches.Postfixes)
            {
                ref var patchInfo = ref CollectionsMarshal.GetValueRefOrNullRef(infos, postfix.owner);
                if (Unsafe.IsNullRef(ref patchInfo))
                {
                    continue;
                }

                patchInfo.Postfix = true;
            }

            foreach (var transpiler in patches.Transpilers)
            {
                ref var patchInfo = ref CollectionsMarshal.GetValueRefOrNullRef(infos, transpiler.owner);
                if (Unsafe.IsNullRef(ref patchInfo))
                {
                    continue;
                }

                patchInfo.Transpiler = true;
            }

            foreach (var finalizer in patches.Finalizers)
            {
                ref var patchInfo = ref CollectionsMarshal.GetValueRefOrNullRef(infos, finalizer.owner);
                if (Unsafe.IsNullRef(ref patchInfo))
                {
                    continue;
                }

                patchInfo.Finalizer = true;
            }

            foreach (var info in infos)
            {
                var patchInfo = info.Value;
                List<string> opts = new List<string>(5);
                if (patchInfo.Prefix)
                {
                    opts.Add("prefix");
                }

                if (patchInfo.Postfix)
                {
                    opts.Add("postfix");
                }

                if (patchInfo.Transpiler)
                {
                    opts.Add("transpiler");
                }

                if (patchInfo.Finalizer)
                {
                    opts.Add("finalizer");
                }

                if (patchInfo.SkippingPrefix)
                {
                    opts.Add("skippingPrefix");
                }
                
                builder.AppendLine($"\t\t {info.Key} [{string.Join(',', opts)}]");
            }
        }


        logger.LogDebug("Patches from Harmony: {patches}", builder.ToString());
    }

    private record struct PatchInfo(
        bool Prefix,
        bool Postfix,
        bool Transpiler,
        bool Finalizer,
        bool SkippingPrefix = false
    );
}

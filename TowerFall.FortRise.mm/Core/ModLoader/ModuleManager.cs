using System;
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

    internal Dictionary<string, Mod> NameToFortModule = [];
    internal Dictionary<string, IModResource> NameToMod = [];
    internal Dictionary<string, Subtexture> NameToIcon = [];

    internal HashSet<string> BlacklistedMods;

    private readonly Dictionary<RegistryBatchType, List<RegistryQueue>> registryBatch = [];
    private readonly Dictionary<string, IModRegistry> registries = [];
    private readonly IProxyManager<string> proxyManager;

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

        var moduleBuilders = new Dictionary<(string, string), ModuleBuilder>();
        proxyManager = new ProxyManager<string>((proxyInfo) =>
        {
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

    private void LoadMods(List<ModuleMetadata> mods)
    {
        // create a dependency graph
        Dictionary<string, List<ModuleMetadata>> dependencyGraph = [];
        Dictionary<string, List<ModuleMetadata>> toLoadAfter = [];
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
    }

    public bool CheckDependencyMetadata(ModuleMetadata metadata, bool storeError)
    {
        foreach (var internalMetadata in InternalModuleMetadatas)
        {
            if (metadata.Name != internalMetadata.Name)
            {
                continue;
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

        ResourceTree.AddMod(metadata, modResource);

        if (!string.IsNullOrEmpty(metadata.DLL))
        {
            metadata.AssemblyLoadContext = new ModAssemblyLoadContext(metadata);

            using var stream = modResource.GetResource(metadata.DLL)?.Stream
                ?? throw new FileNotFoundException($"{metadata.DLL} not found on mod: '{metadata}'");

            asm = Resolver.LoadModAssembly(modResource, metadata.DLL, stream);
        }

        NameToMod.Add(metadata.Name, modResource);
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

        InternalMods.Add(modResource);
        InternalModuleMetadatas.Add(metadata);

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

            mod.SetupHotReload(() =>
            {
                logger.LogInformation("{modName} {modVersion} reloading.", metadata.Name, metadata.Version);

                InternalFortModules.Remove(mod);
                NameToFortModule.Remove(metadata.Name);

                metadata.AssemblyLoadContext.Unload();
                metadata.AssemblyLoadContext = new ModAssemblyLoadContext(metadata);

                GC.Collect();
                GC.WaitForPendingFinalizers();

                var modResource = RiseCore.ModuleManager.GetMod(metadata.Name);

                var fullDllPath = Path.Combine(metadata.PathDirectory, metadata.DLL);

                if (File.Exists(fullDllPath))
                {
                    using var stream = File.OpenRead(fullDllPath);
                    var asm = Resolver.LoadModAssembly(modResource, metadata.DLL, stream);

                    LoadAssembly(metadata, content, asm);
                }
            });

            InternalFortModules.Add(mod);
            NameToFortModule.Add(metadata.Name, mod);

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
            new LimitedHarmony(new Harmony(metadata.Name))
        );
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
            IModRegistry reg = new ModRegistry(metadata, this, logger);
            registry = reg;
        }

        return registry;
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

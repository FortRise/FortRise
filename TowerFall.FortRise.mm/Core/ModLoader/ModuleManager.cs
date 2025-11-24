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

// want to make it as a record class, but somehow we need to store a reference to it, so I can't
internal class ModDelayed(int requiredCount, ModuleMetadata metadata) 
{
    public int RequiredCount = requiredCount;
    public ModuleMetadata Metadata = metadata;
}

internal class ModuleManager
{
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
    internal HashSet<string> InternalTags = [];

    internal ModEventsManager EventsManager = new();

    internal Dictionary<string, Mod> NameToFortModule = [];
    internal Dictionary<string, IModResource> NameToMod = [];
    internal HashSet<string> BlacklistedMods;
    internal HashSet<string> CantLoad = [];
    internal Dictionary<string, Subtexture> NameToIcon = [];

    private readonly Dictionary<RegistryBatchType, List<RegistryQueue>> registryBatch = [];
    private readonly Dictionary<string, IModRegistry> registries = [];
    private readonly IProxyManager<string> proxyManager;

    public enum LoadError { Delayed, Failure }

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
                logger.LogDebug("Ignored '{dir}' as it is blacklisted.", dir);
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
                logger.LogDebug("Ignored '{file}' as it is blacklisted.", file);
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
                    logger.LogError(
                        "Outdated Dependency {modName} {modVersion} > {targetModVersion}",
                        metadata.Name,
                        metadata.Version,
                        internalMetadata.Version
                    );
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
        IModContent content;
        if (!string.IsNullOrEmpty(metadata.PathZip))
        {
            content = new ModContent(metadata);
            modResource = new ZipModResource(metadata, content);

            RiseCore.ResourceTree.AddMod(metadata, modResource);

            using var zip = ZipFile.OpenRead(metadata.PathZip);
            var dllPath = metadata.DLL.Replace('\\', '/');
            var dllMeta = zip.GetEntry(dllPath);
            if (dllMeta != null)
            {
                metadata.AssemblyLoadContext = new ModAssemblyLoadContext(metadata);

                using var dll = dllMeta.ExtractStream();
                asm = Relinker.LoadModAssembly(metadata, metadata.DLL, dll);
            }
        }
        else if (!string.IsNullOrEmpty(metadata.PathDirectory))
        {
            content = new ModContent(metadata);
            modResource = new FolderModResource(metadata, content);

            RiseCore.ResourceTree.AddMod(metadata, modResource);
            var fullDllPath = Path.Combine(metadata.PathDirectory, metadata.DLL);

            if (File.Exists(fullDllPath))
            {
                metadata.AssemblyLoadContext = new ModAssemblyLoadContext(metadata);

                using var stream = File.OpenRead(fullDllPath);
                asm = Relinker.LoadModAssembly(metadata, metadata.DLL, stream);
            }
        }
        else
        {
            logger.LogError("Mod named: '{modName}' not found!", metadata.Name);
            ErrorPanel.StoreError($"'{metadata.Name}' not found!");
            return LoadError.Failure;
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
            EventsManager.OnBeforeModInstantiation.Raise(null, new BeforeModInstantiationEventArgs(content, context));

            logger.LogInformation("{modName} {modVersion} has been loaded.", metadata.Name, metadata.Version);
        }

        InternalMods.Add(modResource);
        InternalModuleMetadatas.Add(metadata);
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

                logger.LogError("Mod named '{modName}' has missing dependencies.", delayedMod.Metadata.Name);
                ErrorPanel.StoreError($"'{delayedMod.Metadata.Name}' has missing dependencies.");
            }
            else
            {
                // Hey, we can load this one, dependency is not required!
                LoadModSkipDependecies(delayedMod.Metadata);
            }
        }
    }

    private void LoadAssembly(ModuleMetadata metadata, IModContent content, Assembly asm)
    {
        foreach (var t in asm.GetTypes())
        {
            Mod mod;
            if (t.BaseType == typeof(Mod))
            {
                var logger = loggerFactory.CreateLogger(metadata.Name);
                var context = GetModuleContext(metadata, logger);
                EventsManager.OnBeforeModInstantiation.Raise(null, new BeforeModInstantiationEventArgs(content, context));
                mod = Activator.CreateInstance(t, [content, context, logger]) as Mod;
            }
            else
            {
                continue;
            }


            mod.Meta = metadata;
            mod.ParseArgs(RiseCore.ApplicationArgs);
            mod.OnLoad?.Invoke(mod.Context);

            InternalFortModules.Add(mod);
            NameToFortModule.Add(metadata.Name, mod);

            logger.LogInformation("{modName} {modVersion} has been loaded.", mod.Meta.Name, mod.Meta.Version);
        }
    }

    private ModuleContext GetModuleContext(ModuleMetadata metadata, ILogger logger)
    {
        return new ModuleContext(
            AddOrGetRegistry(metadata, logger),
            new ModInterop(this, metadata, proxyManager),
            new ModEvents(metadata, EventsManager),
            flags,
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
            EventsManager.OnModInitialize.Raise(fortModule, fortModule.Meta);
        }

        EventsManager.OnModLoadStateFinished.Raise(null, LoadState.Initialize);

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
        return [.. InternalTags];
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

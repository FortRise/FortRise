using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;

namespace FortRise;

public static partial class RiseCore
{
    public class ModDelayed(int requiredCount, ModuleMetadata metadata) 
    {
        public int RequiredCount = requiredCount;
        public ModuleMetadata Metadata = metadata;
    }

    public static class Loader
    {
        public enum LoadError { Delayed, Failure }
        internal static HashSet<string> BlacklistedMods;
        internal static HashSet<string> CantLoad = new();

        internal static void InitializeMods()
        {
            var delayedMods = new List<ModDelayed>();
            var modDirectory = Path.Combine(GameRootPath, "Mods");
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

        public static void LoadDir(string dir, List<ModDelayed> delayedMods)
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

            if (!Loader.LoadMod(moduleMetadata, out int requiredDependencies).Check(out _, out LoadError err))
            {
                if (err == LoadError.Delayed)
                {
                    delayedMods.Add(new ModDelayed(requiredDependencies, moduleMetadata));
                }
            }
        }

        public static void LoadZip(string file, List<ModDelayed> delayedMods)
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

            if (!Loader.LoadMod(moduleMetadata, out int requiredDependencies).Check(out _, out LoadError err))
            {
                if (err == LoadError.Delayed)
                {
                    delayedMods.Add(new ModDelayed(requiredDependencies, moduleMetadata));
                }
            }
        }

        public static bool CheckDependencyMetadata(ModuleMetadata metadata, bool storeError)
        {
            foreach (var internalMetadata in RiseCore.InternalModuleMetadatas)
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

        public static bool CheckDependencies(ModuleMetadata metadata, out int requiredDependencies)
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

        public static Result<Unit, LoadError> LoadMod(ModuleMetadata metadata, out int requiredDependencies)
        {
            if (!CheckDependencies(metadata, out requiredDependencies))
            {
                return LoadError.Delayed;
            }

            return LoadModSkipDependecies(metadata);
        }

        public static Result<Unit, LoadError> LoadModSkipDependecies(ModuleMetadata metadata)
        {
            Assembly asm = null;
            ModResource modResource;
            if (!string.IsNullOrEmpty(metadata.PathZip))
            {
                modResource = new ZipModResource(metadata);

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
                        asm = Relinker.LoadModAssembly(metadata, metadata.DLL, dll);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(metadata.PathDirectory))
            {
                modResource = new FolderModResource(metadata);

                RiseCore.ResourceTree.AddMod(metadata, modResource);
                var fullDllPath = Path.Combine(metadata.PathDirectory, metadata.DLL);

                if (!RiseCore.DisableFortMods)
                {
                    if (File.Exists(fullDllPath))
                    {
                        metadata.AssemblyLoadContext = new ModAssemblyLoadContext(metadata);

                        using var stream = File.OpenRead(fullDllPath);
                        asm = Relinker.LoadModAssembly(metadata, metadata.DLL, stream);
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
            RiseCore.InternalModuleMetadatas.Add(metadata);

            if (metadata.DLL == string.Empty)
            {
                // Generate custom guids for DLL-Less mods
                string generatedGuid;
                if (string.IsNullOrEmpty(metadata.Author))
                {
                    Logger.Warning($"[Loader] [{metadata.Name}] Author is empty. Guids might conflict with other DLL-Less mods.");
                    generatedGuid = $"{metadata.Name}.{metadata.Version}";
                }
                else
                    generatedGuid = $"{metadata.Name}.{metadata.Version}.{metadata.Author}";
                if (ModuleGuids.Contains(generatedGuid))
                {
                    ErrorPanel.StoreError($"'{metadata.Name}' cannot load due to guid conflict with {generatedGuid}.");
                    Logger.Error($"[Loader] [{metadata.Name}] Guid conflict with {generatedGuid}");
                    return LoadError.Failure;
                }
                Logger.Verbose($"[Loader] [{metadata.Name}] Guid generated! {generatedGuid}");
            }

            return new Unit();
        }

        private static void LoadDelayedMods(List<ModDelayed> delayedMods)
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

        private static void LoadAssembly(ModuleMetadata metadata, ModResource resource, Assembly asm)
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
                if (metadata.Name == string.Empty)
                {
                    metadata.Name = customAttribute.Name;
                }
                module.Meta = metadata;
                module.Name = customAttribute.Name;
                module.ID = customAttribute.GUID;
                var content = resource.Content;
                module.s_Content(content);
                module.ParseArgs(RiseCore.ApplicationArgs);
                module.InternalLoad();
                lock (InternalFortModules)
                    InternalFortModules.Add(module);

                ModuleGuids.Add(module.ID);

                Logger.Info($"[Loader] {module.ID}: {module.Name} Loaded.");
                break;
            }
        }
    }
}

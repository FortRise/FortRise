using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;

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
        public enum LoadResult { Success, Delayed, Failure }
        internal static HashSet<string> BlacklistedMods;
        internal static HashSet<string> CantLoad = new();

        internal static void InitializeMods()
        {
            var delayedMods = new List<ModDelayed>();
            var directory = Directory.GetDirectories("Mods");
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

            var files = Directory.GetFiles("Mods");
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
            ModuleMetadata moduleMetadata = null;
            if (!File.Exists(metaPath))
            {
                return;
            }

            var result = ModuleMetadata.ParseMetadata(dir, metaPath);
            if (!result.Check(out moduleMetadata, out string error))
            {
                Logger.Error(error);
                return;
            }

            switch (Loader.LoadMod(moduleMetadata, out int requiredDependencies))
            {
            case LoadResult.Success:
            case LoadResult.Failure:
                break;
            case LoadResult.Delayed:
                delayedMods.Add(new ModDelayed(requiredDependencies, moduleMetadata));
                break;
            }
        }

        public static void LoadZip(string file, List<ModDelayed> delayedMods)
        {
            using var zipFile = ZipFile.OpenRead(file);

            ModuleMetadata moduleMetadata = null;
            string metaFile = "meta.json";
            var metaZip = zipFile.GetEntry(metaFile);
            if (metaZip == null)
            {
                return;
            }

            using var memStream = metaZip.ExtractStream();

            var result = ModuleMetadata.ParseMetadata(file, memStream, true);
            if (!result.Check(out moduleMetadata, out string error))
            {
                Logger.Error(error);
                return;
            }

            switch (Loader.LoadMod(moduleMetadata, out int requiredDependencies))
            {
            case LoadResult.Success:
            case LoadResult.Failure:
                break;
            case LoadResult.Delayed:
                delayedMods.Add(new ModDelayed(requiredDependencies, moduleMetadata));
                break;
            }
        }

        public static bool CheckDependencies(ModuleMetadata metadata, out int requiredDependencies)
        {
            requiredDependencies = 0;
            if (metadata.Dependencies != null)
            {
                foreach (var dep in metadata.Dependencies)
                {
                    if (RiseCore.InternalModuleMetadatas.Contains(dep))
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
                    if (RiseCore.InternalModuleMetadatas.Contains(dep))
                    {
                        continue;
                    }

                    return false;
                }
            }
            return true;
        }

        public static LoadResult LoadMod(ModuleMetadata metadata, out int requiredDependencies)
        {
            if (!CheckDependencies(metadata, out requiredDependencies))
            {
                return LoadResult.Delayed;
            }

            return LoadModSkipDependecies(metadata);
        }

        public static LoadResult LoadModSkipDependecies(ModuleMetadata metadata)
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
                        using var stream = File.OpenRead(fullDllPath);
                        asm = Relinker.LoadModAssembly(metadata, metadata.DLL, stream);
                    }
                }
            }
            else
            {
                Logger.Error($"[Loader] Mod {metadata.Name} not found");
                return LoadResult.Failure;
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
                    Logger.Error($"[Loader] [{metadata.Name}] Guid conflict with {generatedGuid}");
                    return LoadResult.Failure;
                }
                Logger.Verbose($"[Loader] [{metadata.Name}] Guid generated! {generatedGuid}");
            }

            return LoadResult.Success;
        }

        private static void LoadDelayedMods(List<ModDelayed> delayedMods)
        {
            List<ModDelayed> successfulLoad = new List<ModDelayed>();
            for (int i = 0; i < delayedMods.Count; i++)
            {
                var delayMod = delayedMods[i];
                if (LoadMod(delayMod.Metadata, out int requiredDependencies) == LoadResult.Success)
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

            successfulLoad = null;

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
                module.Content = content;
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

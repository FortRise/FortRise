using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Ionic.Zip;

namespace FortRise;

public static partial class RiseCore 
{
    public static class Loader 
    {
        internal static HashSet<string> BlacklistedMods;

        internal static void InitializeMods() 
        {
            BlacklistedMods = ReadBlacklistedMods("Mods/blacklist.txt");

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
                LoadDir(dir);
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
                LoadZip(file);
            }
        }

        public static void LoadDir(string dir) 
        {
            var metaPath = Path.Combine(dir, "meta.json");
            ModuleMetadata moduleMetadata = null;
            if (!File.Exists(metaPath))
                return;
            
            moduleMetadata = ParseMetadataWithJson(dir, metaPath);

            Loader.LoadMod(moduleMetadata);
        }

        public static void LoadZip(string file)
        {
            using var zipFile = ZipFile.Read(file);

            ModuleMetadata moduleMetadata = null;
            if (!zipFile.ContainsEntry("meta.json"))
                return;
            
            var entry = zipFile["meta.json"];
            using var memStream = entry.ExtractStream();

            moduleMetadata = ParseMetadataWithJson(file, memStream, true);

            Loader.LoadMod(moduleMetadata);
        }

        public static void LoadMod(ModuleMetadata metadata, bool shouldCheck = false) 
        {
            if (metadata == null)
                return;
            
            if (shouldCheck && CheckModsLoaded(metadata))
                return;
            
            // Check dependencies
            if (metadata.Dependencies != null) 
            {
                foreach (var dep in metadata.Dependencies) 
                {
                    if (RiseCore.InternalModuleMetadatas.Contains(dep))
                        continue;

                    Logger.Error($"[Loader] [{metadata.Name}] Dependency {dep} not found!");
                }
            }

            Assembly asm = null;
            ModResource modResource;
            if (!string.IsNullOrEmpty(metadata.PathZip)) 
            {
                modResource = new ZipModResource(metadata);

                RiseCore.ResourceTree.AddMod(metadata, modResource);

                using var zip = new ZipFile(metadata.PathZip);
                var dllPath = metadata.DLL.Replace('\\', '/');
                if (zip.ContainsEntry(dllPath)) 
                {
                    using var dll = zip[dllPath].ExtractStream();
                    asm = Relinker.GetRelinkedAssembly(metadata, metadata.DLL, dll);
                }
            }
            else if (!string.IsNullOrEmpty(metadata.PathDirectory)) 
            {
                modResource = new FolderModResource(metadata);

                RiseCore.ResourceTree.AddMod(metadata, modResource);

                if (File.Exists(metadata.DLL)) 
                {
                    using var stream = File.OpenRead(metadata.DLL);
                    asm = Relinker.GetRelinkedAssembly(metadata, metadata.DLL, stream);
                }
            }
            else 
            {
                Logger.Error($"[Loader] Mod {metadata.Name} not found");
                return;
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
                    return;
                }
                Logger.Verbose($"[Loader] [{metadata.Name}] Guid generated! {generatedGuid}");
                return;
            }
        }

        private static void LoadAssembly(ModuleMetadata metadata, ModResource resource, Assembly asm) 
        {
            foreach (var t in asm.GetTypes())
            {
                var customAttribute = t.GetCustomAttribute<FortAttribute>();
                if (customAttribute == null)
                    continue;
                
                FortModule obj = Activator.CreateInstance(t) as FortModule;
                if (metadata.Name == string.Empty)
                {
                    metadata.Name = customAttribute.Name;
                }
                obj.Meta = metadata;
                obj.Name = customAttribute.Name;
                obj.ID = customAttribute.GUID;
                var content = resource.Content;
                obj.Content = content;

                ModuleGuids.Add(obj.ID);
                obj.Register();

                Logger.Info($"[Loader] {obj.ID}: {obj.Name} Registered.");
                break;
            }
        }

        public static bool CheckModsLoaded(ModuleMetadata metadata) 
        {
            foreach (var mod in RiseCore.InternalMods) 
            {
                if (mod.Metadata == metadata)
                    return true;
            }
            return false;
        }
    }
}
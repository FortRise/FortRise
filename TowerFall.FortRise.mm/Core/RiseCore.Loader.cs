using System.IO;
using Ionic.Zip;

namespace FortRise;

public static partial class RiseCore 
{
    public static class Loader 
    {
        internal static void InitializeMods() 
        {
            var blackListed = ReadBlacklistedMods("Mods/blacklist.txt");

            var directory = Directory.GetDirectories("Mods");
            foreach (var dir in directory) 
            {
                Logger.Log(dir);
                if (dir.Contains("_RelinkerCache"))
                    continue;    
                var dirInfo = new DirectoryInfo(dir);
                if (blackListed != null && blackListed.Contains(dirInfo.Name))  
                {
                    Logger.Verbose($"[Loader] Ignored {dir} as it's blacklisted");
                    continue;
                }
                LoadDir(dir);
            }

            var files = Directory.GetFiles("Mods");
            foreach (var file in files) 
            {
                Logger.Log(file);
                if (!file.EndsWith("zip"))
                    continue;

                if (blackListed != null && blackListed.Contains(Path.GetFileName(file))) 
                {
                    Logger.Verbose($"[Loader] Ignored {file} as it's blacklisted");
                    continue;
                }
                LoadZip(file);
            }

            foreach (var mod in InternalMods) 
            {
                var metadata = mod.Metadata;
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
                        continue;
                    }
                    Logger.Verbose($"[Loader] [{metadata.Name}] Guid generated! {generatedGuid}");
                    continue;
                }

                // Check dependencies
                if (metadata.Dependencies != null) 
                {
                    foreach (var dep in metadata.Dependencies) 
                    {
                        if (ContainsGuidMod(dep))
                            continue;
                        if (ContainsMod(dep))
                            continue;
                        if (ContainsComplexName(dep))
                            continue;
                        Logger.Error($"[Loader] [{metadata.Name}] Dependency {dep} not found!");
                    }
                }
                var dllPath = mod.IsZip ? metadata.DLL : Path.GetFileName(metadata.DLL);

                using var fs = mod.Content[dllPath].Stream;
                if (fs == null)
                    return;
                var pathToAssembly = Path.Combine(metadata.PathDirectory, metadata.DLL);

                var asm = Relinker.GetRelinkedAssembly(metadata, pathToAssembly, fs);
                if (asm == null) 
                {
                    Logger.Error("[Loader] Failed to load assembly: " + asm.FullName);
                    return;
                }
                RegisterAssembly(metadata, mod, asm);
            }
        }

        public static void LoadDir(string dir) 
        {
            var metaPath = Path.Combine(dir, "meta.json");
            ModuleMetadata moduleMetadata = null;
            if (!File.Exists(metaPath)) 
                metaPath = Path.Combine(dir, "meta.xml");
            if (!File.Exists(metaPath))
                return;
            
            if (metaPath.EndsWith("json"))
                moduleMetadata = ParseMetadataWithJson(dir, metaPath);
            else
                moduleMetadata = ParseMetadataWithXML(dir, metaPath);


            // Assembly Mod Loading
            var fortContent = new FortContent(moduleMetadata.PathDirectory);
            var modResource = new ModResource(fortContent, moduleMetadata);
            InternalMods.Add(modResource);
        }

        public static void LoadZip(string file) 
        {
            using var zipFile = ZipFile.Read(file);

            ModuleMetadata moduleMetadata = null;
            string metaPath = null;
            if (zipFile.ContainsEntry("meta.json")) 
            {
                metaPath = "meta.json";
            }
            else if (zipFile.ContainsEntry("meta.xml")) 
            {
                metaPath = "meta.xml";
            }
            else 
                return;
            
            var entry = zipFile[metaPath];
            using var memStream = entry.ExtractStream();
            
            if (metaPath.EndsWith("json"))
                moduleMetadata = ParseMetadataWithJson(file, memStream, true);
            else
                moduleMetadata = ParseMetadataWithXML(file, memStream, true);

            var fortContent = new FortContent(moduleMetadata.PathZip, true);
            var modResource = new ModResource(fortContent, moduleMetadata, true);
            InternalMods.Add(modResource);
        }
    }
}
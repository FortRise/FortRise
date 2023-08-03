using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;

namespace FortRise;

public partial class RiseCore 
{
    internal static HashSet<string> BlacklistedExtension = new() {
        ".csproj", ".cs", ".md", ".toml", ".aseprite", ".ase"
    };

    internal static HashSet<string> BlacklistedRootFolders = new() {
        "bin", "obj"
    };

    public abstract class Resource 
    {
        public string FullPath;
        public string Path;
        public string Root;
        public List<Resource> Childrens = new();
        public ModResource Source;

        public abstract Stream Stream { get; }

        public Resource(ModResource resource, string path, string fullPath) 
        {
            FullPath = fullPath;
            Path = path;
            Source = resource;
        }
    }

    public class FileResource : Resource
    {
        public FileResource(ModResource resource, string path, string fullPath) : base(resource, path, fullPath)
        {
        }

        public override Stream Stream 
        {
            get 
            {
                if (!File.Exists(FullPath))
                    return null;
                return File.OpenRead(FullPath);
            }
        }
    }

    public class ZipResource : Resource
    {
        public ZipEntry Entry;

        public ZipResource(ModResource resource, string path, string fullPath, ZipEntry entry) : base(resource, path, fullPath)
        {
            Entry = entry;
        }

        public override Stream Stream 
        {
            get => Entry.ExtractStream();
        }
    }

    public class GlobalLevelResource : FileResource
    {
        public GlobalLevelResource(ModResource resource, string path, string fullPath) : base(resource, path, fullPath)
        {
        }
    }

    public abstract class ModResource : IDisposable
    {
        public ModuleMetadata Metadata;
        public FortContent Content;
        public Dictionary<string, Resource> Resources = new();
        private bool disposedValue;

        public ModResource(ModuleMetadata metadata) 
        {
            Metadata = metadata;
            Content = new FortContent(Metadata, this);
        }

        public ModResource(string path) 
        {
            Content = new FortContent(path, this);
        }


        public void Add(string path, Resource resource) 
        {
            var rootName = (Metadata is not null ? Metadata.Name : "::global::");
            var rootPath = resource.Root = $"mod:{rootName}/";

            Logger.Verbose("[RESOURCE] Loaded On:" + rootPath);
            Logger.Verbose("[RESOURCE] Loaded:" + path);
            if (Resources.ContainsKey(path)) 
                return;
            
            Resources.Add(path, resource);
            RiseCore.Resources.GlobalResources.Add($"{rootPath}{path}", resource);
        }

        public abstract void Lookup(string prefix);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeInternal();
                }

                disposedValue = true;
            }
        }

        internal virtual void DisposeInternal() {}

        ~ModResource()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class ZipModResource : ModResource
    {
        private ZipFile zipFile;


        public ZipModResource(ModuleMetadata metadata) : base(metadata)
        {
            zipFile = new ZipFile(metadata.PathZip);
        }

        internal override void DisposeInternal()
        {
            zipFile.Dispose();
        }


        public override void Lookup(string prefix) 
        {
            var folders = new Dictionary<string, ZipResource>();
            foreach (var entry in zipFile.Entries) 
            {
                var fileName = entry.FileName.Replace('\\', '/');

                if (entry.IsDirectory)  
                {
                    var file = fileName.Remove(fileName.Length - 1);
                    var zipResource = new ZipResource(this, file, prefix + file, entry);
                    Add(file, zipResource);
                    folders.Add(file, zipResource);
                    var split = file.Split('/');
                    Array.Resize(ref split, split.Length - 1);
                    var newPath = CombineAllPath(split);
                    if (folders.TryGetValue(newPath, out var resource)) 
                    {
                        resource.Childrens.Add(zipResource);
                    }
                }
                else 
                {
                    if (BlacklistedExtension.Contains(Path.GetExtension(fileName))) 
                        continue;
                    var zipResource = new ZipResource(this, fileName, prefix + fileName, entry);
                    Add(fileName, zipResource);
                    if (folders.TryGetValue(Path.GetDirectoryName(fileName).Replace('\\', '/'), out var resource)) 
                    {
                        resource.Childrens.Add(zipResource);
                    }
                }
            }
        }

        private static string CombineAllPath(string[] paths) 
        {
            var sb = new StringBuilder();
            for (int i = 0; i < paths.Length; i++) 
            {
                var path = paths[i];
                sb.Append(path);
                if (i != paths.Length - 1)
                    sb.Append('/');
            }
            return sb.ToString();
        }
    }

    public class AdventureGlobalLevelResource : FolderModResource
    {
        public AdventureGlobalLevelResource() : base("Content/Mod/Adventure")
        {
        }

        public override void Lookup(string prefix)
        {
            var files = Directory.GetFiles(FolderDirectory);
            for (int i = 0; i < files.Length; i++) 
            {
                var filePath = "Content/Mod/Adventure/" + files[i].Replace('\\', '/');
                if (BlacklistedExtension.Contains(Path.GetExtension(filePath))) 
                    continue;
                var simplifiedPath = filePath;
                var fileResource = new GlobalLevelResource(this, simplifiedPath, filePath);
                Add(filePath, fileResource);
            }
            var folders = Directory.EnumerateDirectories(FolderDirectory).ToList();
            foreach (var folder in folders) 
            {
                var fixedFolder = "Content/Mod/Adventure/" + folder.Replace('\\', '/');
                var simpliPath = fixedFolder;
                var newFolderResource = new GlobalLevelResource(this, simpliPath, fixedFolder);
                Lookup(prefix, folder, FolderDirectory, newFolderResource);
                Add(fixedFolder, newFolderResource);
            }
        }

        public void Lookup(string prefix, string path, string modDirectory, FileResource folderResource) 
        {
            var files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++) 
            {
                var filePath = "Content/Mod/Adventure/" + files[i].Replace('\\', '/');
                var simplifiedPath = filePath;
                var fileResource = new GlobalLevelResource(this, simplifiedPath, filePath);
                Add(filePath, fileResource);
                folderResource.Childrens.Add(fileResource);
            }
            var folders = Directory.EnumerateDirectories(path).ToList();
            foreach (var folder in folders) 
            {
                var fixedFolder = "Content/Mod/Adventure/" + folder.Replace('\\', '/');
                var simpliPath = fixedFolder;
                var newFolderResource = new GlobalLevelResource(this, simpliPath, fixedFolder);
                Lookup(prefix, folder, modDirectory, newFolderResource);
                Add(simpliPath, newFolderResource);
                folderResource.Childrens.Add(newFolderResource);
            }
        }
    }

    public class FolderModResource : ModResource
    {
        public string FolderDirectory;
        public FolderModResource(ModuleMetadata metadata) : base(metadata)
        {
            FolderDirectory = metadata.PathDirectory.Replace('\\', '/');
        }

        public FolderModResource(string path) : base(path)
        {
            FolderDirectory = path.Replace('\\', '/');
        }

        public override void Lookup(string prefix)
        {
            var files = Directory.GetFiles(FolderDirectory);
            for (int i = 0; i < files.Length; i++) 
            {
                var filePath = files[i].Replace('\\', '/');
                if (BlacklistedExtension.Contains(Path.GetExtension(filePath))) 
                    continue;
                var simplifiedPath = filePath.Replace(FolderDirectory + '/', "");
                var fileResource = new FileResource(this, simplifiedPath, filePath);
                Add(simplifiedPath, fileResource);
            }
            var folders = Directory.EnumerateDirectories(FolderDirectory).ToList();
            foreach (var folder in folders) 
            {
                var fixedFolder = folder.Replace('\\', '/');
                var simpliPath = fixedFolder.Replace(FolderDirectory + '/', "");
                var newFolderResource = new FileResource(this, simpliPath, fixedFolder);
                Lookup(prefix, folder, FolderDirectory, newFolderResource);
                Add(fixedFolder, newFolderResource);
            }
        }

        public void Lookup(string prefix, string path, string modDirectory, FileResource folderResource) 
        {
            var files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++) 
            {
                var filePath = files[i].Replace('\\', '/');
                var simplifiedPath = filePath.Replace(modDirectory + '/', "");
                var fileResource = new FileResource(this, simplifiedPath, filePath);
                Add(simplifiedPath, fileResource);
                folderResource.Childrens.Add(fileResource);
            }
            var folders = Directory.EnumerateDirectories(path).ToList();
            foreach (var folder in folders) 
            {
                var fixedFolder = folder.Replace('\\', '/');
                var simpliPath = fixedFolder.Replace(modDirectory + '/', "");
                var newFolderResource = new FileResource(this, simpliPath, prefix + simpliPath);
                Lookup(prefix, folder, modDirectory, newFolderResource);
                Add(simpliPath, newFolderResource);
                folderResource.Childrens.Add(newFolderResource);
            }
        }
    }

    public static class Resources 
    {
        public static Dictionary<string, Resource> GlobalResources = new();


        public static void AddMod(ModuleMetadata metadata, ModResource resource) 
        {
            var name = (metadata is not null ? metadata.Name : "::global::");
            var prefixPath = $"mod:{name}/";

            if (GlobalResources.ContainsKey(prefixPath)) 
            {
                Logger.Warning($"[RESOURCE] Conflicting mod asset name found: {prefixPath}");
                return;
            }
            resource.Lookup(prefixPath);
        }

        public static async Task DumpAll() 
        {
            try 
            {
                if (!Directory.Exists("DUMP"))
                    Directory.CreateDirectory("DUMP");
                using var file = File.Create("DUMP/resourcedump.txt");
                using TextWriter tw = new StreamWriter(file);

                tw.WriteLine("FORTRISE RESOURCE DUMP");
                tw.WriteLine("VERSION 4.1.0.0");
                tw.WriteLine("==============================");
                foreach (var globalResource in GlobalResources) 
                {
                    await tw.WriteLineAsync("Global File Path: " + globalResource.Key);
                    await tw.WriteLineAsync("Source: ");
                    await tw.WriteLineAsync("\t FullPath: " + globalResource.Value.FullPath);
                    await tw.WriteLineAsync("\t Path: " + globalResource.Value.Path);
                    await tw.WriteLineAsync("\t Root: " + globalResource.Value.Root);
                    await tw.WriteLineAsync("\t Childrens: ");
                    foreach (var child in globalResource.Value.Childrens) 
                    {
                        await DumpResource(child, "\t");
                    }
                }

                async Task DumpResource(Resource childResource, string line) 
                {
                    await tw.WriteLineAsync(line + "\t FullPath: " + childResource.FullPath);
                    await tw.WriteLineAsync(line + "\t Path: " + childResource.Path);
                    await tw.WriteLineAsync(line + "\t Root: " + childResource.Root);
                    await tw.WriteLineAsync(line + "\t Childrens: ");
                    foreach (var resource in childResource.Childrens) 
                    {
                        await DumpResource(resource, line + "\t");
                    }
                }
            }
            catch (Exception e) 
            {
                Logger.Error("[DUMPRESOURCE]" + e.ToString());
                throw;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace FortRise;

public partial class RiseCore 
{
    internal static HashSet<string> BlacklistedExtension = new() {
        ".csproj", ".cs", ".md", ".toml"
    };

    internal static HashSet<string> BlacklistedRootFolders = new() {
        "bin", "obj"
    };

    public abstract class Resource 
    {
        public string FullPath;
        public string Path;
        public List<Resource> Childrens = new();

        public abstract Stream Stream { get; }

        public Resource(string path, string fullPath) 
        {
            FullPath = fullPath;
            Path = path;
        }
    }

    public class FileResource : Resource
    {
        public FileResource(string path, string fullPath) : base(path, fullPath)
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

        public ZipResource(string path, ZipEntry entry) : base(path, path)
        {
            Entry = entry;
        }

        public override Stream Stream 
        {
            get => Entry.ExtractStream();
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

        public abstract void Lookup();

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

        public override void Lookup() 
        {
            var folders = new Dictionary<string, ZipResource>();
            foreach (var entry in zipFile.Entries) 
            {
                var fileName = entry.FileName.Replace('\\', '/');

                if (entry.IsDirectory)  
                {
                    var file = fileName.Remove(fileName.Length - 1);
                    var zipResource = new ZipResource(file, entry);
                    Resources.Add(file, zipResource);
                    folders.Add(file, zipResource);
                    Logger.Verbose("[RESOURCE] " + file);

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
                    var zipResource = new ZipResource(fileName, entry);
                    Resources.Add(fileName, zipResource);
                    Logger.Verbose("[RESOURCE] " + fileName);
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

    public class FolderModResource : ModResource
    {
        private string modDirectory;
        public FolderModResource(ModuleMetadata metadata) : base(metadata)
        {
            modDirectory = metadata.PathDirectory.Replace('\\', '/');
        }

        public FolderModResource(string path) : base(path)
        {
            modDirectory = path.Replace('\\', '/');
        }

        public override void Lookup()
        {
            var files = Directory.GetFiles(modDirectory);
            for (int i = 0; i < files.Length; i++) 
            {
                var filePath = files[i].Replace('\\', '/');
                if (BlacklistedExtension.Contains(Path.GetExtension(filePath))) 
                    continue;
                var simplifiedPath = filePath.Replace(modDirectory + '/', "");
                Logger.Verbose("[RESOURCE] " + filePath.Replace(modDirectory + '/', ""));
                var fileResource = new FileResource(simplifiedPath, filePath);
                Resources.Add(simplifiedPath, fileResource);
            }
            var folders = Directory.EnumerateDirectories(modDirectory).ToList();
            foreach (var folder in folders) 
            {
                var fixedFolder = folder.Replace('\\', '/');
                var simpliPath = fixedFolder.Replace(modDirectory + '/', "");
                var newFolderResource = new FileResource(simpliPath, fixedFolder);
                Lookup(folder, modDirectory, newFolderResource);
                Resources.Add(simpliPath, newFolderResource);
            }
        }

        public void Lookup(string path, string modDirectory, FileResource folderResource) 
        {
            var files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++) 
            {
                var filePath = files[i].Replace('\\', '/');
                var simplifiedPath = filePath.Replace(modDirectory + '/', "");
                Logger.Verbose("[RESOURCE] " + filePath.Replace(modDirectory + '/', ""));
                var fileResource = new FileResource(simplifiedPath, filePath);
                Resources.Add(simplifiedPath, fileResource);
                folderResource.Childrens.Add(fileResource);
            }
            var folders = Directory.EnumerateDirectories(path).ToList();
            foreach (var folder in folders) 
            {
                var fixedFolder = folder.Replace('\\', '/');
                var simpliPath = fixedFolder.Replace(modDirectory + '/', "");
                var newFolderResource = new FileResource(simpliPath, fixedFolder);
                Logger.Verbose("[RESOURCE] " + simpliPath);
                Lookup(folder, modDirectory, newFolderResource);
                Resources.Add(simpliPath, newFolderResource);
                folderResource.Childrens.Add(newFolderResource);
            }
        }
    }

    public static class Resources 
    {
        public static Dictionary<string, Resource> GlobalResources = new();


        public static void AddMod(ModuleMetadata metadata, ModResource resource) 
        {
            var name = metadata.Name;
            var prefixPath = $"mod:{name}/";

            if (GlobalResources.ContainsKey(prefixPath)) 
            {
                Logger.Warning($"[RESOURCE] Conflicting mod asset name found: {prefixPath}");
                return;
            }
            resource.Lookup();
        }
    }
}
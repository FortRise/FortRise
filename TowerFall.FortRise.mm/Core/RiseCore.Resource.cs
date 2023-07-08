using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Ionic.Zip;
using Monocle;

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

    public abstract class ResourceSystem : IDisposable
    {
        private bool disposedValue;
        public Dictionary<string, RiseCore.Resource> MapResource = new();
        public IReadOnlyCollection<RiseCore.Resource> ListResource => MapResource.Values; 

        public abstract Dictionary<string, Resource> GetFilesystem(string path);

        public virtual void Open(string path) 
        {
            MapResource = GetFilesystem(path);
        }

        protected virtual void DisposeResource() 
        {

        }

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeResource();
                }
                disposedValue = true;
            }
        }

        ~ResourceSystem()
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

    public class FolderResourceSystem : ResourceSystem
    {
        private string modDirectory;
        public FolderResourceSystem(string modDirectory) 
        {
            this.modDirectory = modDirectory.Replace('\\', '/');
        }

        public override Dictionary<string, Resource> GetFilesystem(string path)
        {
            var resources = new Dictionary<string, Resource>();
            var files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++) 
            {
                var filePath = files[i].Replace('\\', '/');
                if (BlacklistedExtension.Contains(Path.GetExtension(filePath))) 
                    continue;
                var simplifiedPath = filePath.Replace(modDirectory + '/', "");
                Logger.Verbose("[RESOURCE] " + filePath.Replace(modDirectory + '/', ""));
                var fileResource = new FileResource(simplifiedPath, filePath);
                resources.Add(simplifiedPath, fileResource);
            }
            var folders = Directory.EnumerateDirectories(path).ToList();
            foreach (var folder in folders) 
            {
                var fixedFolder = folder.Replace('\\', '/');
                var simpliPath = fixedFolder.Replace(modDirectory + '/', "");
                var newFolderResource = new FileResource(simpliPath, fixedFolder);
                GetFilesystem(resources, folder, newFolderResource);
                resources.Add(simpliPath, newFolderResource);
            }
            return resources;
        }

        public void GetFilesystem(Dictionary<string, Resource> resources, string path, FileResource folderResource) 
        {
            var files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++) 
            {
                var filePath = files[i].Replace('\\', '/');
                var simplifiedPath = filePath.Replace(modDirectory + '/', "");
                Logger.Verbose("[RESOURCE] " + filePath.Replace(modDirectory + '/', ""));
                var fileResource = new FileResource(simplifiedPath, filePath);
                resources.Add(simplifiedPath, fileResource);
                folderResource.Childrens.Add(fileResource);
            }
            var folders = Directory.EnumerateDirectories(path).ToList();
            foreach (var folder in folders) 
            {
                var fixedFolder = folder.Replace('\\', '/');
                var simpliPath = fixedFolder.Replace(modDirectory + '/', "");
                var newFolderResource = new FileResource(simpliPath, fixedFolder);
                Logger.Verbose("[RESOURCE] " + simpliPath);
                GetFilesystem(resources, folder, newFolderResource);
                resources.Add(simpliPath, newFolderResource);
                folderResource.Childrens.Add(newFolderResource);
            }
        }
    }

    public class ZipResourceSystem : ResourceSystem
    {
        private ZipFile zipFile;
        public Dictionary<string, ZipResource> Folders = new();
        public ZipResourceSystem(ZipFile zipFile) 
        {
            this.zipFile = zipFile;
        }

        public override Dictionary<string, Resource> GetFilesystem(string path)
        {
            var resources = new Dictionary<string, Resource>();
            foreach (var entry in zipFile.Entries) 
            {
                var fileName = entry.FileName.Replace('\\', '/');

                if (entry.IsDirectory)  
                {
                    var file = fileName.Remove(fileName.Length - 1);
                    var zipResource = new ZipResource(file, entry);
                    resources.Add(file, zipResource);
                    Folders.Add(file, zipResource);
                    Logger.Verbose("[RESOURCE] " + file);

                    var split = file.Split('/');
                    Array.Resize(ref split, split.Length - 1);
                    var newPath = CombineAllPath(split);
                    if (Folders.TryGetValue(newPath, out var resource)) 
                    {
                        resource.Childrens.Add(zipResource);
                    }
                }

                else 
                {
                    if (BlacklistedExtension.Contains(Path.GetExtension(fileName))) 
                        continue;
                    var zipResource = new ZipResource(fileName, entry);
                    resources.Add(fileName, zipResource);
                    Logger.Verbose("[RESOURCE] " + fileName);
                    if (Folders.TryGetValue(Path.GetDirectoryName(fileName).Replace('\\', '/'), out var resource)) 
                    {
                        resource.Childrens.Add(zipResource);
                    }
                }
            }
            return resources;
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

        protected override void DisposeResource()
        {
            zipFile.Dispose();
        }
    }
}
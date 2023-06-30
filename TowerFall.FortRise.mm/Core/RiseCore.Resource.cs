using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Ionic.Zip;
using Monocle;

namespace FortRise;

public partial class RiseCore 
{
    public abstract class Resource 
    {
        public string Path;
        public List<Resource> Childrens = new();

        public abstract Stream Stream { get; }

        public Resource(string path) 
        {
            Path = path;
        }
    }

    public class FileResource : Resource
    {
        public FileResource(string path) : base(path)
        {
        }

        public override Stream Stream 
        {
            get 
            {
                if (!File.Exists(Path))
                    return null;
                return File.OpenRead(Path);
            }
        }
    }

    public class ZipResource : Resource
    {
        public ZipEntry Entry;

        public ZipResource(string path, ZipEntry entry) : base(path)
        {
            Entry = entry;
        }

        public override Stream Stream 
        {
            get 
            {
                var memStream = new MemoryStream();
                Entry.Extract(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                return memStream;
            }
        }
    }

    public abstract class ResourceSystem : IDisposable
    {
        private bool disposedValue;

        public abstract Dictionary<string, Resource> GetFilesystem(string path);

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
                Logger.Log(filePath.Replace(modDirectory + '/', ""));
                resources.Add(filePath.Replace(modDirectory + '/', ""), new FileResource(filePath));
            }
            var folders = Directory.EnumerateDirectories(path).ToList();
            foreach (var folder in folders) 
            {
                GetFilesystem(resources, folder);
            }
            return resources;
        }

        public void GetFilesystem(Dictionary<string, Resource> resources, string path) 
        {
            var files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++) 
            {
                var filePath = files[i].Replace('\\', '/');
                Logger.Log(filePath.Replace(modDirectory + '/', ""));
                resources.Add(filePath.Replace(modDirectory + '/', ""), new FileResource(filePath));
            }
            var folders = Directory.EnumerateDirectories(path).ToList();
            foreach (var folder in folders) 
            {
                GetFilesystem(resources, folder);
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

                Logger.Log(fileName);
                var zipResource = new ZipResource(entry.FileName, entry);
                resources.Add(fileName, zipResource);
                if (entry.IsDirectory) 
                    Folders.Add(fileName, zipResource);
                else if (Folders.TryGetValue(Path.GetDirectoryName(fileName), out var resource)) 
                {
                    resource.Childrens.Add(zipResource);
                }
            }
            return resources;
        }

        protected override void DisposeResource()
        {
            zipFile.Dispose();
        }
    }
}
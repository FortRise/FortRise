using System.IO;
using Ionic.Zip;

namespace FortRise;

public partial class RiseCore 
{
    public abstract class Resource 
    {
        public string Path;

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
        }

        public override Stream Stream 
        {
            get 
            {
                var memStream = new MemoryStream();
                Entry.Extract(memStream);
                return memStream;
            }
        }
    }

    public abstract class ResourceSystem
    {
        public abstract Resource[] GetFiles(string path);
    }

    public class FolderResourceSystem : ResourceSystem
    {
        public override Resource[] GetFiles(string path)
        {
            var files = Directory.GetFiles(path);
            var resources = new Resource[files.Length];
            for (int i = 0; i < files.Length; i++) 
            {
                resources[i] = new FileResource(files[i]);
            }
            return resources;
        }
    }

    public class ZipResourceSystem : ResourceSystem
    {
        public ZipResourceSystem() 
        {

        }

        public override Resource[] GetFiles(string path)
        {
            return null;
        }
    }
}
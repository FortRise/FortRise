using System.IO;

namespace FortRise;

public class FileResourceInfo : ResourceInfo
{
    public FileResourceInfo(IModResource resource, string path, string fullPath) : base(resource, path, fullPath)
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

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace FortRise;

public class ZipResourceInfo : ResourceInfo
{
    public ZipArchiveEntry Entry;

    public ZipResourceInfo(IModResource resource, string path, string fullPath, ZipArchiveEntry entry) : base(resource, path, fullPath)
    {
        Entry = entry;
    }

    public override Stream Stream
    {
        get 
        {
            ZipModResource modSource = (ZipModResource)Source;
            var entry = modSource.Zip.GetEntry(Path);
            if (entry == null) 
            {
                throw new KeyNotFoundException($"File {Path} not found in archive {modSource.Metadata.PathZip}");
            }
            return entry.ExtractStream();
        } 
    }
}

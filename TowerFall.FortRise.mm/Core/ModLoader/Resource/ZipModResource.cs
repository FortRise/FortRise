using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace FortRise;

public class ZipModResource : ModResource
{
    private static readonly char[] SplitSeparator = ['/'];
    public ZipArchive Zip;


    public ZipModResource(ModuleMetadata metadata, IModContent content) : base(metadata, content)
    {
        Zip = ZipFile.OpenRead(metadata.PathZip);
    }

    internal override void DisposeInternal()
    {
        Zip.Dispose();
    }


    public override void Lookup(string prefix)
    {
        var folders = new Dictionary<string, ZipResourceInfo>();

        var entries = Zip.Entries.OrderBy(f => f.FullName);

        foreach (var entry in entries)
        {
            var fileName = entry.FullName.Replace('\\', '/');

            if (entry.IsEntryDirectory())
            {
                var file = fileName.Remove(fileName.Length - 1);
                var zipResource = new ZipResourceInfo(this, file, prefix + file, entry);
                Add(file, zipResource);
                folders.Add(file, zipResource);
                var split = file.Split(SplitSeparator);
                Array.Resize(ref split, split.Length - 1);
                var newPath = CombineAllPath(split);
                if (folders.TryGetValue(newPath, out var resource))
                {
                    resource.Childrens.Add(zipResource);
                }
            }
            else
            {
                var zipResource = new ZipResourceInfo(this, fileName, prefix + fileName, entry);
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

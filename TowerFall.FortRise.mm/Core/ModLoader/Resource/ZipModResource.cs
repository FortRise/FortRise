using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace FortRise;

public class ZipModResource : ModResource
{
    public ZipArchive Zip;
    public string Root;


    public ZipModResource(ModuleMetadata metadata, IModContent content) : base(metadata, content)
    {
        Zip = ZipFile.OpenRead(metadata.PathZip);
    }

    public ZipModResource(ModuleMetadata metadata, IModContent content, string root) : base(metadata, content)
    {
        Root = root;
        Zip = ZipFile.OpenRead(metadata.PathZip);
    }

    internal override void DisposeInternal()
    {
        Zip?.Dispose();
        Zip = null;
    }


    public override void Lookup(string prefix)
    {
        Root = ResolveRoot();
        var rootPrefix = !string.IsNullOrEmpty(Root) ? Root.TrimEnd('/') + "/" : string.Empty;

        var rootDirectory = new ZipResourceInfo(this, "", prefix + '/', null);
        var directories = new Dictionary<string, ZipResourceInfo>();

        var entries = Zip.Entries.OrderBy(f => f.FullName);

        foreach (var entry in entries)
        {
            var rawPath = entry.FullName.Replace('\\', '/');
            var fileName = rawPath;

            if (!string.IsNullOrEmpty(rootPrefix))
            {
                if (fileName.StartsWith(rootPrefix))
                {
                    fileName = rawPath[rootPrefix.Length..];
                }
                else 
                {
                    continue;
                }
            }

            if (string.IsNullOrEmpty(fileName))
            {
                continue;
            }

            ZipResourceInfo zipResource;
            if (entry.IsEntryDirectory)
            {
                var directory = fileName[..^1];

                zipResource = new ZipResourceInfo(this, directory, prefix + directory, entry);
                Add(directory, zipResource);
                directories[directory] = zipResource;

                LinkToParent(directory, zipResource, directories, rootDirectory);
            }
            else
            {
                zipResource = new ZipResourceInfo(this, fileName, prefix + fileName, entry);
                Add(fileName, zipResource);

                LinkToParent(fileName, zipResource, directories, rootDirectory);
            }
        }

        Add("", rootDirectory);
    }

    private static void LinkToParent(
        string path,
        ZipResourceInfo resource,
        Dictionary<string, ZipResourceInfo> directories,
        ZipResourceInfo rootDirectory
    )
    {
        int lastSlash = path.LastIndexOf('/');
        string parentPath = lastSlash > 0 ? path[..lastSlash] : string.Empty;

        if (string.IsNullOrEmpty(parentPath))
        {
            rootDirectory.Childrens.Add(resource);
        }
        else if (directories.TryGetValue(parentPath, out var parentDirectory))
        {
            parentDirectory.Childrens.Add(resource);
        }
    }

    private string ResolveRoot()
    {
        if (!string.IsNullOrEmpty(Root))
        {
            return Root.TrimEnd('/');
        }

        bool hasRootMeta = Zip.Entries.Any(e =>
            e.FullName.Replace('\\', '/').Equals("meta.json", StringComparison.OrdinalIgnoreCase));

        if (hasRootMeta)
        {
            return string.Empty;
        }

        var metaEntry = Zip.Entries.FirstOrDefault(e => 
            e.FullName.Replace('\\', '/').EndsWith("/meta.json", StringComparison.OrdinalIgnoreCase));

        if (metaEntry is not null)
        {
            var normalized = metaEntry.FullName.Replace('\\', '/');
            var parts = normalized.Split('/');
            if (parts.Length == 2)
            {
                return parts[0];
            }
        }

        string commonRoot = null;
        foreach (var entry in Zip.Entries)
        {
            var path = entry.FullName.Replace('\\', '/').TrimStart('/');
            if (string.IsNullOrEmpty(path)) { continue; }

            int slashIndex = path.IndexOf('/');
            if (slashIndex == -1)
            {
                return string.Empty;
            }

            string topDir = path[..slashIndex];
            if (commonRoot is null)
            {
                commonRoot = topDir;
            }
            else if (!string.Equals(commonRoot, topDir))
            {
                return string.Empty;
            }
        }

        return commonRoot ?? "";
    }
}

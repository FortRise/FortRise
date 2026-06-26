#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;

namespace FortRise;

internal sealed class DirectoryStorageResourceInfo : IStorageResourceInfo
{
    private readonly IModStorage storage;
    public DirectoryStorageResourceInfo(
        string fullPath,
        IModStorage storage)
    {
        this.storage = storage;
        FullPath = fullPath.Replace('\\', '/');
        Path = FullPath.Replace(storage.StoragePath + '/', "");

        if (!Directory.Exists(FullPath))
        {
            Directory.CreateDirectory(FullPath);
        }
    }

    public string Text => string.Empty;
    public XmlDocument? Xml => null;

    public IReadOnlyList<IStorageResourceInfo> Childrens
    {
        get
        {
            var resources = new List<IStorageResourceInfo>();

            var files = Directory.GetFiles(FullPath);
            var directories = Directory.GetDirectories(FullPath);

            foreach (var file in files)
            {
                resources.Add(new FileStorageResourceInfo(file, storage));
            }

            foreach (var dir in directories)
            {
                resources.Add(new FileStorageResourceInfo(dir, storage));
            }

            return resources;
        }
    }

    public Stream ReadStream => throw new InvalidOperationException($"You cannot use a stream from '{storage.StoragePath}' as its a directory."); 
    public Stream WriteStream => throw new InvalidOperationException($"You cannot use a stream from '{storage.StoragePath}' as its a directory."); 

    public string FullPath { get; set; }
    public string Path { get; set; }

    public IStorageResourceInfo AddFile(string filename)
    {
        var file = new FileStorageResourceInfo(filename, storage);
        return file;
    }

    public bool ExistsRelativePath(string path)
    {
        return File.Exists(System.IO.Path.Combine(FullPath, path));
    }

    public IStorageResourceInfo GetRelativePath(string path)
    {
        if (TryGetRelativePath(path, out var res))
        {
            return res;
        }

        throw new FileNotFoundException($"'{path}' cannot be found from storage: {storage.StoragePath}");
    }

    public bool TryGetRelativePath(string path, [NotNullWhen(true)] out IStorageResourceInfo resource)
    {
        if (ExistsRelativePath(path))
        {
            resource = new FileStorageResourceInfo(System.IO.Path.Combine(FullPath, path), storage);
            return true;
        }

        resource = null!;
        return false;
    }
}

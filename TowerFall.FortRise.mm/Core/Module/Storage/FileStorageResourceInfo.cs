#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using Monocle;

namespace FortRise;

internal sealed class FileStorageResourceInfo : IStorageResourceInfo
{
    private readonly IModStorage storage;
    public FileStorageResourceInfo(
        string fullPath,
        IModStorage storage)
    {
        this.storage = storage;
        FullPath = fullPath.Replace('\\', '/');
        Path = FullPath.Replace(storage.StoragePath + '/', "");
    }

    public string Text => File.ReadAllText(FullPath);
    public XmlDocument? Xml => Calc.LoadXML(FullPath);

    public IReadOnlyList<IStorageResourceInfo> Childrens => []; 

    public Stream ReadStream
    {
        get
        {
            if (!File.Exists(FullPath))
            {
                var dirName = System.IO.Path.GetDirectoryName(FullPath);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName!);
                }
                return File.Create(FullPath);
            }

            return File.Open(FullPath, FileMode.Open, FileAccess.ReadWrite);
        }
    }

    public Stream WriteStream
    {
        get
        {
            if (!File.Exists(FullPath))
            {
                var dirName = System.IO.Path.GetDirectoryName(FullPath);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName!);
                }
                return File.Create(FullPath);
            }

            return File.Open(FullPath, FileMode.Create, FileAccess.ReadWrite);
        }
    }

    public string FullPath { get; set; }
    public string Path { get; set; }

    public IStorageResourceInfo AddFile(string filename)
    {
        throw new InvalidOperationException("You cannot add a file inside of a file.");
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
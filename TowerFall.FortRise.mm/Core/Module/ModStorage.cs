#nullable enable
using System;
using System.IO;

namespace FortRise;

internal sealed class ModStorage : IModStorage
{
    private ModuleMetadata modMeta;

    public ModStorage(ModuleMetadata metadata)
    {
        modMeta = metadata;
        if (!Directory.Exists(StoragePath))
        {
            Directory.CreateDirectory(StoragePath);
        }
    }

    public string StoragePath => Path.Combine(ModIO.GetRootPath(), "Saves", modMeta.Name).Replace('\\', '/');

    public void WriteAllText(string filepath, string text)
    {
        var file = OpenOrCreate(filepath);
        using var stream = file.WriteStream;
        
        using TextWriter writer = new StreamWriter(stream);
        writer.Write(text);
    }

    public IStorageResourceInfo? Open(string filepath)
    {
        string file = GetValidPath(filepath);
        if (File.Exists(file))
        {
            return new FileStorageResourceInfo(file, this);
        }

        if (Directory.Exists(file))
        {
            return new DirectoryStorageResourceInfo(file, this);
        }

        return null;
    }

    public IStorageResourceInfo OpenOrCreate(string filepath)
    {
        return new FileStorageResourceInfo(GetValidPath(filepath), this);
    }

    public bool Delete(string filepath, bool recursive)
    {
        var file = GetValidPath(filepath);
        if (File.Exists(file))
        {
            File.Delete(file);
            return true;
        }

        if (Directory.Exists(file))
        {
            Directory.Delete(file, recursive);
        }

        return false;
    }

    public bool Exists(string filepath)
    {
        var file = GetValidPath(filepath);
        return File.Exists(file) || Directory.Exists(file);
    }

    private string GetValidPath(string filepath)
    {
        var rootPath = Path.GetFullPath(StoragePath).Replace('\\', '/');
        var file = Path.GetFullPath(Path.Combine(StoragePath, filepath)).Replace('\\', '/');

        if (!file.StartsWith(rootPath))
        {
            throw new UnauthorizedAccessException($"""
            [{modMeta.Name}] Cannot traverse path outside of the storage.
            Requested Access: {file}
            Allowed Root Access: {rootPath}
            """);
        }
        return file;
    }

    public IStorageResourceInfo CreateDirectory(string filepath)
    {
        return new DirectoryStorageResourceInfo(filepath, this);
    }
}
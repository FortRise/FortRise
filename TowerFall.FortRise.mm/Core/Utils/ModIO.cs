using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Monocle;

namespace FortRise;

/// <summary>
/// Utility class that can access to mods' content or outside of the mods' content in a portable way.
/// Mod's content path generally starts with the prefix of 'mod:{mod_metadata_name}/' where 'mod_metadata_name' is
/// the name of the Mod in its metadata name.
/// </summary>
public static class ModIO
{
    /// <summary>
    /// Get the FortRise path where the launcher is.
    /// </summary>
    /// <returns>A path to launcher directory</returns>
    public static string GetRootPath()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    [Obsolete("Use ModIO.IsDirectory instead")]
    public static bool IsFolder(IResourceInfo resource)
    {
        return IsDirectory(resource);
    }

    public static bool IsDirectory(IResourceInfo resource)
    {
        return resource.ResourceType == typeof(ResourceTypeFolder);
    }

    public static bool IsFile(IResourceInfo resource)
    {
        return resource.ResourceType != typeof(ResourceTypeFolder);
    }

    [Obsolete("Use ModIO.IsDirectory instead")]
    public static bool IsFolder(string path)
    {
        return IsDirectory(path);
    }

    public static bool IsDirectory(string path)
    {
        if (ResourceTree.TryGetValue(path, out var res))
        {
            return IsDirectory(res);
        }

        FileAttributes attr = File.GetAttributes(path);
        return attr == FileAttributes.Directory;
    }

    public static bool IsFile(string path)
    {
        if (ResourceTree.TryGetValue(path, out var res))
        {
            return IsFile(res);
        }
        FileAttributes attr = File.GetAttributes(FileSystemUtils.NormalizedPath(path));
        return attr != FileAttributes.Directory;
    }

    public static bool IsDirectoryOrFileExists(string path)
    {
        return ResourceTree.IsExist(path) || 
            Directory.Exists(FileSystemUtils.NormalizedPath(path)) || 
            File.Exists(FileSystemUtils.NormalizedPath(path));
    }

    public static bool IsFileExists(string path)
    {
        return ResourceTree.IsExist(path) || File.Exists(FileSystemUtils.NormalizedPath(path));
    }

    public static bool IsDirectoryExists(string path)
    {
        return ResourceTree.IsExist(path) || Directory.Exists(FileSystemUtils.NormalizedPath(path));
    }

    public static XmlDocument LoadXml(IResourceInfo resource)
    {
        using var text = OpenRead(resource);
        return patch_Calc.LoadXML(text);
    }

    public static XmlDocument LoadXml(string path)
    {
        if (ResourceTree.TryGetValue(path, out var resource))
        {
            return LoadXml(resource);
        }

        using var text = OpenRead(path);
        return patch_Calc.LoadXML(text);
    }

    public static string[] GetFiles(string path)
    {
        if (ResourceTree.TryGetValue(path, out var res))
        {
            return GetFiles(res);
        }
        return Directory.GetFiles(FileSystemUtils.NormalizedPath(path));
    }

    public static string[] GetFiles(IResourceInfo res)
    {
        List<IResourceInfo> childs = [];
        foreach (var r in res.Childrens)
        {
            if (r.ResourceType == typeof(ResourceTypeFile))
            {
                childs.Add(r);
            }
        }
        string[] files = new string[childs.Count];
        for (int i = 0; i < childs.Count; i++)
        {
            var child = childs[i];
            files[i] = child.RootPath;
        }

        return files;
    }

    public static string[] GetDirectories(string path)
    {
        if (ResourceTree.TryGetValue(path, out var res))
        {
            return GetDirectories(res);
        }

        return Directory.GetDirectories(FileSystemUtils.NormalizedPath(path));
    }

    public static string[] GetDirectories(IResourceInfo res)
    {
        List<IResourceInfo> childs = [];
        foreach (var r in res.Childrens)
        {
            if (r.ResourceType == typeof(ResourceTypeFolder))
            {
                childs.Add(r);
            }
        }
        string[] directories = new string[childs.Count];
        for (int i = 0; i < childs.Count; i++)
        {
            var child = childs[i];
            directories[i] = child.RootPath;
        }

        return directories;
    }

    public static Stream OpenRead(IResourceInfo resource)
    {
        return resource.Stream;
    }

    public static Stream OpenRead(string path)
    {
        if (ResourceTree.TryGetValue(path, out var res))
        {
            return OpenRead(res);
        }

        return File.OpenRead(FileSystemUtils.NormalizedPath(path));
    }

    public static Stream Open(string path, FileMode mode)
    {
        if (ResourceTree.TryGetValue(path, out var res))
        {
            var rfs = res.Stream;
            return rfs;
        }

        return File.Open(FileSystemUtils.NormalizedPath(path), mode);
    }

    public static StreamReader OpenText(IResourceInfo resource)
    {
        Stream fs = resource.Stream;
        return new StreamReader(fs);
    }

    public static StreamReader OpenText(string path)
    {
        if (ResourceTree.TryGetValue(path, out var res))
        {
            return OpenText(res);
        }

        return File.OpenText(FileSystemUtils.NormalizedPath(path));
    }

    public static string ReadAllText(string path) 
    {
        if (ResourceTree.TryGetValue(path, out var res))
        {
            return ReadAllText(res);
        }

        return File.ReadAllText(FileSystemUtils.NormalizedPath(path));
    }

    public static string ReadAllText(IResourceInfo resource)
    {
        using var fs = resource.Stream;
        using var tr = new StreamReader(fs);
        return tr.ReadToEnd();
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Monocle;

// I am using namespace now for transition with FortRise 5
namespace FortRise.IO;

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

    public static bool IsFolder(RiseCore.Resource resource)
    {
        return resource.ResourceType == typeof(RiseCore.ResourceTypeFolder);
    }

    public static bool IsFile(RiseCore.Resource resource)
    {
        return resource.ResourceType != typeof(RiseCore.ResourceTypeFolder);
    }

    public static bool IsFolder(string path)
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res))
        {
            return IsFolder(res);
        }
        FileAttributes attr = File.GetAttributes(path);
        return attr == FileAttributes.Directory;
    }

    public static bool IsFile(string path)
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res))
        {
            return IsFile(res);
        }
        FileAttributes attr = File.GetAttributes(path);
        return attr != FileAttributes.Directory;
    }

    public static bool IsDirectoryOrFileExists(string path)
    {
        return RiseCore.ResourceTree.IsExist(path) || Directory.Exists(path) || File.Exists(path);
    }

    public static bool IsFileExists(string path)
    {
        return RiseCore.ResourceTree.IsExist(path) || File.Exists(path);
    }

    public static bool IsDirectoryExists(string path)
    {
        return RiseCore.ResourceTree.IsExist(path) || Directory.Exists(path);
    }

    public static XmlDocument LoadXml(RiseCore.Resource resource)
    {
        var text = ModIO.OpenRead(resource);
        return patch_Calc.LoadXML(text);
    }

    public static XmlDocument LoadXml(string path)
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var resource))
        {
            return ModIO.LoadXml(resource);
        }
        return Calc.LoadXML(path);
    }

    public static string[] GetFiles(string path)
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res))
        {
            return GetFiles(res);
        }
        return Directory.GetFiles(path);
    }

    public static string[] GetFiles(RiseCore.Resource res)
    {
        List<RiseCore.Resource> childs = new List<RiseCore.Resource>();
        foreach (var r in res.Childrens)
        {
            if (r.ResourceType == typeof(RiseCore.ResourceTypeFile))
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
        if (RiseCore.ResourceTree.TryGetValue(path, out var res))
        {
            return GetDirectories(res);
        }
        return Directory.GetDirectories(path);
    }

    public static string[] GetDirectories(RiseCore.Resource res)
    {
        List<RiseCore.Resource> childs = new List<RiseCore.Resource>();
        foreach (var r in res.Childrens)
        {
            if (r.ResourceType == typeof(RiseCore.ResourceTypeFolder))
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

    public static Stream OpenRead(RiseCore.Resource resource)
    {
        return resource.Stream;
    }

    public static Stream OpenRead(string path)
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res))
        {
            return OpenRead(res);
        }

        return File.OpenRead(path);
    }

    public static Stream Open(string path, FileMode mode)
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res))
        {
            var rfs = res.Stream;
            return rfs;
        }

        return File.Open(path, mode);
    }

    public static StreamReader OpenText(RiseCore.Resource resource)
    {
        Stream fs = resource.Stream;
        return new StreamReader(fs);
    }

    public static StreamReader OpenText(string path)
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res))
        {
            return OpenText(res);
        }

        return File.OpenText(path);
    }

    public static string ReadAllText(string path) 
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res))
        {
            return ReadAllText(res);
        }

        return File.ReadAllText(path);
    }

    public static string ReadAllText(RiseCore.Resource resource)
    {
        using var fs = resource.Stream;
        using TextReader tr = new StreamReader(fs);
        return tr.ReadToEnd();
    }
}

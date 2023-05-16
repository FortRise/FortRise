using System;
using System.IO;
using System.Reflection;

namespace FortRise;

public partial class FortModule
{
    [Obsolete("Use the PathUtils.ToContentPath(string path) instead")]
    public static string ToContentPath(string path) 
    {
        var modDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
        var result = Path.Combine(modDirectory, "Content", path).Replace("\\", "/");
        return result;
    }
}

public static class PathUtils 
{
    public static string ToContentPath(string path) 
    {
        var modDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
        var result = Path.Combine(modDirectory, "Content", path).Replace("\\", "/");
        return result;
    }

    public static string CombinePrefixPath(string path, string path2, string prefix) 
    {
        if (!path.Contains(prefix)) 
        {
            return Path.Combine(path2, path);
        }
        var length = prefix.Length;
        var slicedPath = path.AsSpan().Slice(length).ToString();
        var combined = Path.Combine(path2, slicedPath);
        return prefix + combined;
    }
}
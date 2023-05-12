using System;
using System.IO;
using System.Reflection;
using Monocle;

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
}
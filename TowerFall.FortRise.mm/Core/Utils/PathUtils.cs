using System.IO;
using System.Reflection;
using Monocle;

namespace FortRise;

public partial class FortModule
{
    public static string ToContentPath(string path) 
    {
        var modDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
        var result = Path.Combine(modDirectory, "Content", path).Replace("\\", "/");
        return result;
    }
}
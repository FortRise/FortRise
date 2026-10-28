using System;

namespace FortRise;

public static class FileSystemUtils 
{
    public static string GetSimplifiedPath(string path, int baseLength)
    {
        var span = path.AsSpan();
        int offset = baseLength;

        if (offset < span.Length && (span[offset] == '/' || span[offset] == '\\'))
        {
            offset += 1;
        }

        int actualOffset =  Math.Min(offset, span.Length);
        var relativeSpan = span[actualOffset..];

        if (relativeSpan.IndexOf('\\') < 0)
        {
            return relativeSpan.ToString();
        }

        return string.Create(relativeSpan.Length, (path, actualOffset), (dest, src) => 
        {
            src.path.AsSpan(src.actualOffset).CopyTo(dest);
            dest.Replace('\\', '/');
        });
    }

    public static string NormalizedPath(string path)
    {
        return path.Replace('\\', '/');
    }
}


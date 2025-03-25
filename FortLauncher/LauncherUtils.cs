using System;
using System.Linq;

namespace FortLauncher;

internal static class LauncherUtils
{
    public static bool ChecksumsEqual(string[] a, string[] b) 
    {
        if (a.Length != b.Length)
            return false;
        for (int i = 0; i < a.Length; i++) 
        {
            var left = a[i].AsSpan().Trim();
            var right = b[i].AsSpan().Trim();
            if (!left.SequenceEqual(right))
                return false;
        }
        return true;
    }

    public static ReadOnlySpan<char> ToHexadecimalString(this ReadOnlySpan<byte> data)
    {
        return Convert.ToHexString(data);
    }
}
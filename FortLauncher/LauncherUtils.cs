using System;
using System.Linq;
using System.Text;
using SDL3;

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

    public static unsafe byte* EncodeAsUTF8(ReadOnlySpan<char> str)
    {
        if (str == ReadOnlySpan<char>.Empty)
        {
            return (byte*) 0;
        }

        var size = (str.Length * 4) + 1;
        var buffer = (byte*) SDL.SDL_malloc((UIntPtr) size);
        fixed (char* strPtr = str)
        {
            Encoding.UTF8.GetBytes(strPtr, str.Length + 1, buffer, size);
        }

        return buffer;
    }
}
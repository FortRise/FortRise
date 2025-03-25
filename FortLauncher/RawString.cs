using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FortLauncher;

public unsafe ref struct RawString : IDisposable
{
    private byte *data;
    public int Length => len;
    private int len;

    public RawString(ReadOnlySpan<char> utf16String)
    {
        byte *data = (byte*)NativeMemory.Alloc((nuint)(utf16String.Length));

        var size = (utf16String.Length * 4) + 1;
        int len;
        fixed (char *str = utf16String)
        {
            len = Encoding.UTF8.GetBytes(str, utf16String.Length + 1, data, size);
        }

        this.data = data;
        this.len = len;
    }

    public IntPtr ToPointer()
    {
        return (IntPtr)data;
    }

    public void Dispose()
    {
        NativeMemory.Free(data);
    }

    public static implicit operator byte*(in RawString rawString)
    {
        return rawString.data;
    }

    public static implicit operator RawString(ReadOnlySpan<char> str)
    {
        return new RawString(str);
    }

    public static implicit operator RawString(string str)
    {
        return new RawString(str);
    }
}
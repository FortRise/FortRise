using System;
using System.IO;
using Ionic.Zip;

namespace FortRise;

public static class HelperExtensions 
{
    public static string ToHexadecimalString(this byte[] data)
        => BitConverter.ToString(data).Replace("-", string.Empty);

    public static MemoryStream ExtractStream(this ZipEntry entry) 
    {
        var memStream = new MemoryStream();
        entry.Extract(memStream);
        memStream.Seek(0, SeekOrigin.Begin);
        return memStream;
    }
}
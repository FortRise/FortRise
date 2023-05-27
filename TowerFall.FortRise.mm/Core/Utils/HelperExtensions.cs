using System;

namespace FortRise;

public static class HelperExtensions 
{
    public static string ToHexadecimalString(this byte[] data)
        => BitConverter.ToString(data).Replace("-", string.Empty);
}
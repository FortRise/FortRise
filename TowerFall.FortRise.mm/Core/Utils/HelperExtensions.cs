using System;
using System.IO;
using MonoMod.Utils;
using Ionic.Zip;
using Microsoft.Xna.Framework;

namespace FortRise;

public static class HelperExtensions 
{
    public static Rectangle Overlap(this Rectangle rect, in Rectangle other) 
    {
        bool overlapX = rect.Right > other.Left && rect.Left < other.Right;
        bool overlapY = rect.Bottom > other.Top && rect.Top < other.Bottom;

        Rectangle result = new Rectangle();

        if (overlapX) 
        {
            result.X = Math.Max(rect.Left, other.Left);
            result.Width = Math.Min(rect.Right, other.Right) - result.X;
        }

        if (overlapY) 
        {
            result.Y = Math.Max(rect.Top, other.Top);
            result.Height = Math.Min(rect.Bottom, other.Bottom) - result.Y;
        }

        return result;
    }

    public static string ToHexadecimalString(this byte[] data)
        => BitConverter.ToString(data).Replace("-", string.Empty);

    public static MemoryStream ExtractStream(this ZipEntry entry) 
    {
        var memStream = new MemoryStream();
        entry.Extract(memStream);
        memStream.Seek(0, SeekOrigin.Begin);
        return memStream;
    }

    public static DynamicData Dynamic(this object obj) 
    {
        return DynamicData.For(obj);
    }

    public static T DynGetData<T>(this object obj, string name) 
    {
        return DynamicData.For(obj).Get<T>(name);
    }

    public static T DynGetData<TTarget, T>(this TTarget obj, string name) 
    where TTarget : class
    {
        using var dyn = new DynData<TTarget>(obj);
        return dyn.Get<T>(name);
        
    }

    public static void DynSetData(this object obj, string name, object value) 
    {
        DynamicData.For(obj).Set(name, value);
    }

    public static void DynSetData<T>(this T obj, string name, object value) 
    where T : class
    {
        using var dyn = new DynData<T>(obj);
        dyn.Set(name, value);
    }
}
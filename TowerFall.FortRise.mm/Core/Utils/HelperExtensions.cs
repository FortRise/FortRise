#nullable enable
using System;
using System.IO;
using Microsoft.Xna.Framework;
using System.IO.Compression;
using System.Reflection;
using HarmonyLib;
using System.Linq;

namespace FortRise;

public static class ColorExt
{
    extension(Color color)
    {
        public string ColorToRGBHex => $"{ToHexString(color.R)}{ToHexString(color.G)}{ToHexString(color.B)}";
        public string ColorToRGBAHex => $"{ToHexString(color.R)}{ToHexString(color.G)}{ToHexString(color.B)}{ToHexString(color.A)}";
    }

    public static string ToHexString(float f)
    {
        return ((byte)(f * 255)).ToString("X2");
    }
}

public static class RectangleExt
{
    extension(in Rectangle rect)
    {
        public Rectangle Overlap(in Rectangle other)
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
    }
}

public static class ZipExt
{
    extension(ZipArchiveEntry entry)
    {
        public bool IsEntryDirectory
        {
            get 
            {
                // I'm not sure if this is the best way to do this
                // - Teuria
                int len = entry.FullName.Length;
                return len > 0 && (entry.FullName.EndsWith('\\') || entry.FullName.EndsWith('/'));
            }
        }

        public bool IsEntryFile => !entry.IsEntryDirectory;

        public MemoryStream ExtractStream()
        {
            var memStream = new MemoryStream();
            // ZipArchive must only open one entry at a time, 
            // we had 2 separate threads that uses this 
            lock (entry.Archive)
            {
                using var stream = entry.Open();

                // Perhaps, it is safe to do this?
                stream.CopyTo(memStream);
            }

            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }
    }
}

public static class StreamExt
{
    extension(byte[] data)
    {
        public string ToHexadecimalString()
            => Convert.ToHexString(data);
    }

    extension(ReadOnlySpan<byte> data)
    {
        public string ToHexadecimalString()
            => Convert.ToHexString(data);
    }
}

public static class HarmonyExt
{
    extension(IHarmony harmony)
    {
        public void PatchVirtual(
            MethodBase? original,
            HarmonyMethod? prefix = null,
            HarmonyMethod? postfix = null,
            HarmonyMethod? transpiler = null,
            HarmonyMethod? finalizer = null,
            bool includeBaseMethod = true
        )
        {
            ArgumentNullException.ThrowIfNull(original);
            ArgumentNullException.ThrowIfNull(original.DeclaringType);

            var declaringType = original.DeclaringType;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var subType in assembly.GetTypes().Where(t => t.IsAssignableFrom(declaringType)))
                {
                    if (!includeBaseMethod && subType == original.DeclaringType)
                    {
                        continue;
                    }

                    var origParams = original.GetParameters();
                    var subTypeOrig = AccessTools.DeclaredMethod(subType, original.Name, [.. origParams.Select(x => x.ParameterType)]);

                    if (subTypeOrig is null)
                    {
                        continue;
                    }

                    if (!subTypeOrig.HasMethodBody())
                    {
                        continue;
                    }

                    if (
                        prefix is not null && prefix.method.GetParameters().Any(x => !(x.Name ?? "").StartsWith("__")) ||
                        postfix is not null && postfix.method.GetParameters().Any(x => !(x.Name ?? "").StartsWith("__")) ||
                        finalizer is not null && finalizer.method.GetParameters().Any(x => !(x.Name ?? "").StartsWith("__"))
                    )
                    {
                        var subTypeOrigParams = subTypeOrig.GetParameters();

                        for (int i = 0; i < origParams.Length; i++)
                        {
                            if (origParams[i].Name != subTypeOrigParams[i].Name)
                            {
                                throw new InvalidOperationException(
                                    $"Method {declaringType.Name}.{original.Name} has a mistmatched for {subType.Name} with argument #{i}: '{origParams[i].Name}' with '{subTypeOrigParams[i].Name}'."
                                );
                            }
                        }
                    }

                    harmony.Patch(subTypeOrig, prefix, subTypeOrig.HasMethodBody() ? postfix : null, transpiler, finalizer);
                }
            }
        }
    }
}

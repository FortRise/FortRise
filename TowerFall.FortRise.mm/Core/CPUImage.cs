using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;

namespace FortRise;

public class CPUImage : IDisposable
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public unsafe Span<Color> Pixels 
    {
        get 
        {
            if (Width <= 0 || Height <= 0)
                return Span<Color>.Empty;
            return new Span<Color>((void*)data, Width * Height);
        }
    }

    public IntPtr Data => data;
    private IntPtr data;
    private bool useSIMD;
    private int len;

    private bool disposedValue;

    internal CPUImage() {}

    public unsafe CPUImage(int width, int height) 
    {
        Width = width;
        Height = height;
        this.len = (int)(width * height * 4);
        data = SDL.SDL_SIMDAlloc((uint)len);

        byte *ptr = (byte*)data.ToPointer();
        for (int i = 0; i < len; i += 4) 
        {
            ptr[i + 0] = 0;
            ptr[i + 1] = 0;
            ptr[i + 2] = 0;
            ptr[i + 3] = 0;
        }
    }

    public CPUImage(string path) 
    {
        using FileStream fs = File.OpenRead(path);
        Load(fs);
    }

    public CPUImage(Stream stream) 
    {
        Load(stream);
    }

    private unsafe void Load(Stream stream) 
    {
        var pixelData = FNA3D_ReadImageStream(stream, out int w, out int h, out len);

        Width = w;
        Height = h;
        data = (IntPtr)pixelData;

        if (data == IntPtr.Zero) 
        {
            throw new FailedToLoadImageException();
        }
    }

    public unsafe void CopyFrom(ReadOnlySpan<Color> pixels, int x, int y, int srcWidth, int srcHeight)
    {
        Rectangle destination = new Rectangle(x, y, srcWidth, srcHeight);

        Rectangle dst = new Rectangle(0, 0, Width, Height).Overlap(destination);
        if (dst.Width <= 0 || dst.Height <= 0) 
        {
            return;
        }

        Point pixel = new Point(dst.X - destination.X, dst.Y - destination.Y); 

        fixed (Color* pixPtr = pixels) 
        {
            Color* dataPtr = (Color*)data.ToPointer();
            int size = dst.Width;

            for (int yh = 0; yh < dst.Height; yh++) 
            {
                Color* srcPtr = pixPtr + ((pixel.Y + yh) * srcWidth + pixel.X);
                Color* destPtr = dataPtr + ((dst.Y + yh) * Width + dst.X);
                
                Buffer.MemoryCopy(srcPtr, destPtr, size * 4, size * 4);
            }
        }
    }

    public unsafe void CopyFrom(CPUImage image, int x, int y) 
    {
        CopyFrom(image.Pixels, x, y, image.Width, image.Height);
    }

    public unsafe void SavePNG(string path, int width, int height) 
    {
        using var fs = File.Create(path);
        WritePNGStream(fs, Width, Height, width, height, data);
    }

    public unsafe void Premultiply() 
    {
        fixed (Color *ptr = Pixels) 
        {
            byte alpha;
            for (int i = 0; i < Width * Height; i++) 
            {
                Color col = ptr[i];

                alpha = col.A;
                ptr[i].R = (byte)(ptr[i].R * alpha / 255);
                ptr[i].G = (byte)(ptr[i].G * alpha / 255);
                ptr[i].B = (byte)(ptr[i].B * alpha / 255);
            }
        }
    }

    public Texture2D UploadAsTexture(GraphicsDevice device) 
    {
        Texture2D texture = new Texture2D(device, Width, Height);
        texture.SetDataPointerEXT(0, null, data, len);
        return texture;
    }

    public unsafe CPUImage GetRegion(int sx, int sy, int w, int h) 
    {
        Color[] subTexData = new Color[w * h];

        var subX = sx;
        var subY = sy;
        var width = w;
        var height = h;
        var texWidth = Width;
        var subTexWidth = w;

        Color *rawTex = (Color*)data;
        fixed (Color* rawSubTex = subTexData) 
        {
            for (int y = height - 1; y > -1; y--) 
            {
                for (int x = width - 1; x > -1; x--) 
                {
                    rawSubTex[y * subTexWidth + x] = rawTex[(subY + y) * texWidth + subX + x];
                }
            }
        }
        var len = (uint)(w * h * 4);
        
        var dat = SDL.SDL_SIMDAlloc(len);
        Color *d = (Color*)dat;
        for (int i = 0; i < subTexData.Length; i++) 
        {
            d[i] = subTexData[i];
        }
        CPUImage image = new CPUImage();
        image.Width = w;
        image.Height = h;
        image.data = dat;
        image.useSIMD = true;
        return image;
    }

    public enum Format { PNG, QOI }

    protected virtual unsafe void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            IntPtr lockedPtr = Interlocked.Exchange(ref data, IntPtr.Zero);
            if (lockedPtr != IntPtr.Zero) 
            {
                if (useSIMD) 
                {
                    SDL.SDL_SIMDFree(lockedPtr);
                } 
                else 
                {
                    FNA3D_Image_Free(lockedPtr);
                }

                data = IntPtr.Zero;
            }

            GC.SuppressFinalize(this);
            disposedValue = true;
        }
    }

    ~CPUImage()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public static Func<IntPtr, IntPtr> Malloc = (Func<IntPtr, IntPtr>)typeof(SDL2.SDL)
        .GetMethod("SDL_malloc", BindingFlags.Static | BindingFlags.NonPublic)
        .CreateDelegate(typeof(Func<IntPtr, IntPtr>));

    public static Action<IntPtr> Free = (Action<IntPtr>)typeof(SDL2.SDL)
        .GetMethod("SDL_free", BindingFlags.Static | BindingFlags.NonPublic)
        .CreateDelegate(typeof(Action<IntPtr>));

    public delegate IntPtr FNA3D_ReadImageStreamDelegate(Stream stream, out int width, out int height, out int len, int forceW = -1, int forceH = -1, bool zoom = false);
    public static FNA3D_ReadImageStreamDelegate FNA3D_ReadImageStream = (FNA3D_ReadImageStreamDelegate)typeof(FNALoggerEXT).Assembly
        .GetType("Microsoft.Xna.Framework.Graphics.FNA3D", true)
        .GetMethod("ReadImageStream", BindingFlags.Static | BindingFlags.Public)
        .CreateDelegate(typeof(FNA3D_ReadImageStreamDelegate));

    public static Action<IntPtr> FNA3D_Image_Free = (Action<IntPtr>)typeof(FNALoggerEXT).Assembly
        .GetType("Microsoft.Xna.Framework.Graphics.FNA3D", true)
        .GetMethod("FNA3D_Image_Free", BindingFlags.Static | BindingFlags.Public)
        .CreateDelegate(typeof(Action<IntPtr>));

    public static Action<Stream, int, int, int, int, IntPtr> WritePNGStream = (Action<Stream, int, int, int, int, IntPtr>)typeof(FNALoggerEXT).Assembly
        .GetType("Microsoft.Xna.Framework.Graphics.FNA3D", true)
        .GetMethod("WritePNGStream", BindingFlags.Static | BindingFlags.Public)
        .CreateDelegate(typeof(Action<Stream, int, int, int, int, IntPtr>));
}

public sealed class FailedToLoadImageException : Exception {}
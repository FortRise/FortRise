using System;
using Microsoft.Xna.Framework;
using MonoMod;

namespace Monocle;

public class patch_Sprite<T> : Sprite<T>
{
    public patch_Sprite(Texture texture, Rectangle? clipRect, int frameWidth, int frameHeight, int frameSep = 0) : base(texture, clipRect, frameWidth, frameHeight, frameSep)
    {
    }

    public void orig_ctor(Subtexture texture, Rectangle? clipRect, int frameWidth, int frameHeight, int frameSep = 0) {}
    public void orig_ctor(Texture texture, Rectangle? clipRect, int frameWidth, int frameHeight, int frameSep = 0) {}

    [MonoModConstructor]
    public void ctor(Subtexture texture, Rectangle? clipRect, int frameWidth, int frameHeight, int frameSep = 0)
    {
        if (texture.Width < frameWidth)
        {
            throw new ArgumentException($"FrameWidth is higher than the texture width ({frameWidth} > {texture.Width}).");
        }
        if (texture.Height < frameHeight)
        {
            throw new ArgumentException($"FrameHeight is higher than the texture height ({frameHeight} > {texture.Height}).");
        }
        orig_ctor(texture, clipRect, frameWidth, frameHeight, frameSep);
    }

    [MonoModConstructor]
    public void ctor(Texture texture, Rectangle? clipRect, int frameWidth, int frameHeight, int frameSep = 0)
    {
        if (texture.Width < frameWidth)
        {
            throw new ArgumentException($"FrameWidth is higher than the texture width ({frameWidth} > {texture.Width}).");
        }
        if (texture.Height < frameHeight)
        {
            throw new ArgumentException($"FrameHeight is higher than the texture height ({frameHeight} > {texture.Height}).");
        }
        orig_ctor(texture, clipRect, frameWidth, frameHeight, frameSep);
    }
}

using System;
using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace FortRise.ImGuiLib;

internal static class TexturePreview
{
    private static readonly Dictionary<Texture2D, IntPtr> bound = [];

    public static void Draw(IFortRiseImGuiAPI.IRenderer renderer, Subtexture subtexture, Rectangle source, float zoom)
    {
        var texture = subtexture.Texture2D;
        if (texture is null || texture.IsDisposed)
        {
            ImGui.TextDisabled("Texture not loaded");
            return;
        }

        if (!bound.TryGetValue(texture, out var id))
        {
            id = renderer.BindTexture(texture);
            bound[texture] = id;
        }

        float x = subtexture.X + source.X;
        float y = subtexture.Y + source.Y;
        var uv0 = new System.Numerics.Vector2(x / texture.Width, y / texture.Height);
        var uv1 = new System.Numerics.Vector2((x + source.Width) / texture.Width, (y + source.Height) / texture.Height);
        ImGui.Image(id, new System.Numerics.Vector2(source.Width * zoom, source.Height * zoom), uv0, uv1);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise.ImGuiLib;

internal sealed class SpriteTab : IFortRiseImGuiAPI.ITabItem
{
    public string Title => "Sprites";
    private static readonly string[] spriteDataNames = ["SpriteData", "CorpseSpriteData", "MenuSpriteData", "BGSpriteData", "BossSpriteData"];
    private int currentData;
    private string searchBar = "";
    private string? selected;
    private string[]? keys;
    private float zoom = 4f;

    private SpriteInfo? info;
    private int currentAnim;
    private float timer;
    private int animFrame;
    private bool playing = true;

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        if (ImGui.Combo("Sprite Data", ref currentData, spriteDataNames, spriteDataNames.Length))
        {
            keys = null;
            Select(null, null);
        }

        var data = GetSpriteData(currentData);
        if (data is null)
        {
            ImGui.TextDisabled("Sprite data not loaded");
            return;
        }

        if (keys is null || ImGui.Button("Refresh"))
        {
            keys = [.. data.sprites.Keys.Order()];
        }
        ImGui.SameLine();
        ImGui.InputText("Search", ref searchBar, 100);

        ImGui.BeginChild("List", new System.Numerics.Vector2(300, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);
        foreach (var key in keys)
        {
            if (!string.IsNullOrEmpty(searchBar) && !key.Contains(searchBar, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            if (ImGui.Selectable(key, key == selected))
            {
                Select(key, data);
            }
        }
        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("Preview", System.Numerics.Vector2.Zero, ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);
        if (info is not null)
        {
            RenderPreview(renderer, info);
        }
        ImGui.EndChild();
    }

    private void RenderPreview(IFortRiseImGuiAPI.IRenderer renderer, SpriteInfo info)
    {
        ImGui.Text(selected);
        ImGui.Text($"Texture: {info.TextureName}  Frame: {info.FrameWidth}x{info.FrameHeight}");
        if (info.Texture is null)
        {
            ImGui.TextDisabled("Texture not found in atlas");
            return;
        }

        if (info.Animations.Length == 0)
        {
            ImGui.SliderFloat("Zoom", ref zoom, 1f, 16f);
            TexturePreview.Draw(renderer, info.Texture, new Rectangle(0, 0, info.Texture.Width, info.Texture.Height), zoom);
            return;
        }

        if (ImGui.BeginListBox("Animations", new System.Numerics.Vector2(0, 120)))
        {
            for (int i = 0; i < info.Animations.Length; i++)
            {
                var anim = info.Animations[i];
                if (ImGui.Selectable($"{anim.ID}  (delay {anim.Delay}, loop {anim.Loop}, frames {string.Join(',', anim.Frames)})", i == currentAnim))
                {
                    currentAnim = i;
                    animFrame = 0;
                    timer = 0;
                }
            }
            ImGui.EndListBox();
        }

        var current = info.Animations[currentAnim];
        if (current.Frames.Length == 0)
        {
            return;
        }

        ImGui.Checkbox("Play", ref playing);
        ImGui.SameLine();
        ImGui.SliderInt("Frame", ref animFrame, 0, current.Frames.Length - 1);
        ImGui.SliderFloat("Zoom", ref zoom, 1f, 16f);

        if (playing && current.Delay > 0)
        {
            timer += ImGui.GetIO().DeltaTime;
            while (timer >= current.Delay)
            {
                timer -= current.Delay;
                animFrame++;
                if (animFrame >= current.Frames.Length)
                {
                    animFrame = current.Loop ? 0 : current.Frames.Length - 1;
                }
            }
        }

        int frame = current.Frames[Math.Clamp(animFrame, 0, current.Frames.Length - 1)];
        ImGui.Text("Sheet Frame: " + frame);
        TexturePreview.Draw(renderer, info.Texture, info.GetFrameRect(frame), zoom);
    }

    private void Select(string? key, SpriteData? data)
    {
        selected = key;
        currentAnim = 0;
        animFrame = 0;
        timer = 0;
        info = key is null || data is null ? null : SpriteInfo.Create(data.sprites[key], data.atlas);
    }

    private static SpriteData? GetSpriteData(int index)
    {
        return index switch
        {
            0 => TFGame.SpriteData,
            1 => TFGame.CorpseSpriteData,
            2 => TFGame.MenuSpriteData,
            3 => TFGame.BGSpriteData,
            4 => TFGame.BossSpriteData,
            _ => null
        };
    }

    private record AnimInfo(string ID, float Delay, bool Loop, int[] Frames);

    private record SpriteInfo(string TextureName, Subtexture? Texture, int FrameWidth, int FrameHeight, AnimInfo[] Animations)
    {
        public Rectangle GetFrameRect(int frame)
        {
            int columns = Math.Max(1, Texture!.Width / FrameWidth);
            return new Rectangle(frame % columns * FrameWidth, frame / columns * FrameHeight, FrameWidth, FrameHeight);
        }

        public static SpriteInfo Create(XmlElement xml, Atlas atlas)
        {
            string textureName = xml["Texture"]?.InnerText ?? "";
            atlas.SubTextures.TryGetValue(textureName, out var texture);
            int frameWidth = ReadInt(xml["FrameWidth"], texture?.Width ?? 0);
            int frameHeight = ReadInt(xml["FrameHeight"], texture?.Height ?? 0);

            var animations = new List<AnimInfo>();
            if (xml["Animations"] is { } animationsXml && frameWidth > 0 && frameHeight > 0)
            {
                foreach (XmlElement anim in animationsXml.GetElementsByTagName("Anim"))
                {
                    animations.Add(new AnimInfo(
                        anim.GetAttribute("id"),
                        float.TryParse(anim.GetAttribute("delay"), System.Globalization.CultureInfo.InvariantCulture, out var delay) ? delay : 0f,
                        anim.GetAttribute("loop") != "false",
                        anim.HasAttribute("frames") ? Calc.ReadCSVInt(anim.GetAttribute("frames")) : []
                    ));
                }
            }

            return new SpriteInfo(textureName, texture, frameWidth, frameHeight, [.. animations]);
        }

        private static int ReadInt(XmlElement? element, int fallback)
        {
            return element is not null && int.TryParse(element.InnerText, out var value) ? value : fallback;
        }
    }
}

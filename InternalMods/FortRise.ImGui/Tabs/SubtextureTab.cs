using System;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise.ImGuiLib;

internal sealed class SubtextureTab : IFortRiseImGuiAPI.ITabItem
{
    public string Title => "Subtextures";
    private static readonly string[] atlasNames = ["Atlas", "MenuAtlas", "BGAtlas", "BossAtlas", "EditorAtlas"];
    private int currentAtlas;
    private string searchBar = "";
    private string? selected;
    private string[]? keys;
    private float zoom = 4f;

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        if (ImGui.Combo("Atlas", ref currentAtlas, atlasNames, atlasNames.Length))
        {
            keys = null;
            selected = null;
        }

        var atlas = GetAtlas(currentAtlas);
        if (atlas is null)
        {
            ImGui.TextDisabled("Atlas not loaded");
            return;
        }

        if (keys is null || ImGui.Button("Refresh"))
        {
            keys = [.. atlas.SubTextures.Keys.Order()];
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
                selected = key;
            }
        }
        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("Preview", System.Numerics.Vector2.Zero, ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);
        if (selected is not null && atlas.SubTextures.TryGetValue(selected, out var subtexture))
        {
            ImGui.Text(selected);
            ImGui.Text($"Size: {subtexture.Width}x{subtexture.Height}  Position: {subtexture.X}, {subtexture.Y}");
            ImGui.SliderFloat("Zoom", ref zoom, 1f, 16f);
            TexturePreview.Draw(renderer, subtexture, new Rectangle(0, 0, subtexture.Width, subtexture.Height), zoom);
        }
        ImGui.EndChild();
    }

    private static Atlas? GetAtlas(int index)
    {
        return index switch
        {
            0 => TFGame.Atlas,
            1 => TFGame.MenuAtlas,
            2 => TFGame.BGAtlas,
            3 => TFGame.BossAtlas,
            4 => TFGame.EditorAtlas,
            _ => null
        };
    }
}

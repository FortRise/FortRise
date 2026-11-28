using System;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Monocle;

namespace FortRise.ImGuiLib;

internal sealed class EntityTab : IFortRiseImGuiAPI.ITabItem
{
    public string Title => "Entities";
    private string searchBar = "";

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        var scene = Engine.Instance.Scene;
        if (scene is null)
        {
            return;
        }

        ImGui.InputText("Search", ref searchBar, 100);

        var groups = scene.Layers.Values
            .SelectMany(x => x.Entities)
            .Where(x => string.IsNullOrEmpty(searchBar) || x.GetType().Name.Contains(searchBar, StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.GetType().Name)
            .OrderBy(x => x.Key);

        foreach (var group in groups)
        {
            if (!ImGui.TreeNode($"{group.Key} ({group.Count()})"))
            {
                continue;
            }

            foreach (var entity in group.ToArray())
            {
                ImGui.PushID(entity.GetHashCode());
                if (ImGui.TreeNode(entity.GetType().FullName))
                {
                    var pos = new System.Numerics.Vector2(entity.Position.X, entity.Position.Y);
                    if (ImGui.DragFloat2("Position", ref pos))
                    {
                        entity.Position = new Vector2(pos.X, pos.Y);
                    }
                    ImGui.Checkbox("Active", ref entity.Active);
                    ImGui.SameLine();
                    ImGui.Checkbox("Visible", ref entity.Visible);
                    ImGui.Text("Collidable: " + entity.Collidable);
                    ImGui.Text("Components: " + entity.Components.Count);
                    if (ImGui.Button("Remove"))
                    {
                        entity.RemoveSelf();
                    }
                    ImGui.TreePop();
                }
                ImGui.PopID();
            }

            ImGui.TreePop();
        }
    }
}

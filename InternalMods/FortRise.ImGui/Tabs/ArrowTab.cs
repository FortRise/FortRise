using System;
using System.Linq;
using ImGuiNET;
using Monocle;
using TowerFall;

namespace FortRise.ImGuiLib;

internal sealed class ArrowTab : IFortRiseImGuiAPI.ITabItem
{
    private int currentArrow;
    private bool dirty;
    private ArrowDB[]? arrowsCache;
    private string searchBar = "";
    public string Title => "Arrows";

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        ImGui.InputText("Search", ref searchBar, 100);
        
        var arrows = GetArrowList();
        ArrowDB[] arr;

        if (string.IsNullOrEmpty(searchBar))
        {
            arr = arrows;
        }
        else 
        {
            arr = [.. arrows.Where(x => x.Name.Contains(searchBar, StringComparison.OrdinalIgnoreCase))];
        }

        string[] arrStr = [.. arr.Select(x => x.Name)];

        if (ImGui.ListBox("Arrows", ref currentArrow, arrStr, arrStr.Length, arrows.Length))
        {
            if (Engine.Instance.Scene is Level level)
            {			
                foreach (Player player in level.Players.Cast<Player>())
				{
                    var arrow = arr[currentArrow];
                    ArrowTypes type = arrow.Type;
					player.Arrows.AddArrows([type, type]);
				}
            }
        }
    }

    private ArrowDB[] GetArrowList()
    {
        if (!dirty)
        {
            int len = 11 + ArrowsRegistry.StringToTypes.Count;
            arrowsCache = new ArrowDB[len];

            for (int i = 0; i < len; i++)
            {
                var type = (ArrowTypes)i;
                arrowsCache[i] = new ArrowDB(type, ArrowTypesToName(type));
            }
            dirty = true;
        }
        return arrowsCache!;
    }

    private record struct ArrowDB(ArrowTypes Type, string Name);

    private static string ArrowTypesToName(ArrowTypes type)
    {
        return type switch
        {
            ArrowTypes.Normal => nameof(ArrowTypes.Normal),
            ArrowTypes.Bomb => nameof(ArrowTypes.Bomb),
            ArrowTypes.Bolt => nameof(ArrowTypes.Bolt),
            ArrowTypes.SuperBomb => nameof(ArrowTypes.SuperBomb),
            ArrowTypes.Bramble => nameof(ArrowTypes.Bramble),
            ArrowTypes.Drill => nameof(ArrowTypes.Drill),
            ArrowTypes.Feather => nameof(ArrowTypes.Feather),
            ArrowTypes.Laser => nameof(ArrowTypes.Laser),
            ArrowTypes.Prism => nameof(ArrowTypes.Prism),
            ArrowTypes.Toy => nameof(ArrowTypes.Toy),
            ArrowTypes.Trigger => nameof(ArrowTypes.Trigger),
            _ => ArrowsRegistry.GetArrow(type)!.Name,
        };
    }
}

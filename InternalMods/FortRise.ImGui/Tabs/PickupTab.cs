using System;
using System.Linq;
using ImGuiNET;
using TowerFall;

namespace FortRise.ImGuiLib;

internal sealed class PickupTab : IFortRiseImGuiAPI.ITabItem
{
    public string Title => "Pickups";
    private bool dirty;
    private string searchBar = "";
    private string[] pickupCache = null!;
    private int currentPickup;

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        ImGui.InputText("Search", ref searchBar, 100);
        
        var arrows = GetPickupList();
        string[] arr;

        if (string.IsNullOrEmpty(searchBar))
        {
            arr = arrows;
        }
        else 
        {
            arr = [.. arrows.Where(x => x.Contains(searchBar, StringComparison.OrdinalIgnoreCase))];
        }

        ImGui.ListBox("Pickups", ref currentPickup, arr, arr.Length, arrows.Length);
    }

    private string[] GetPickupList()
    {
        if (!dirty)
        {
            int len = 21 + PickupsRegistry.GetAllPickups().Count;
            pickupCache = new string[len];

            for (int i = 0; i < len; i++)
            {
                var type = (Pickups)i;
                pickupCache[i] = PickupTypesToName(type);
            }
            dirty = true;
        }
        return pickupCache!;
    }

    private static string PickupTypesToName(Pickups type)
    {
        return type switch
        {
            Pickups.Arrows => "Arrows",
            Pickups.BombArrows => "BombArrows",
            Pickups.SuperBombArrows => "SuperBombArrows",
            Pickups.LaserArrows => "LaserArrows",
            Pickups.BrambleArrows => "BrambleArrows",
            Pickups.DrillArrows => "DrillArrows",
            Pickups.BoltArrows => "BoltArrows",
            Pickups.FeatherArrows => "FeatherArrows",
            Pickups.TriggerArrows => "TriggerArrows",
            Pickups.PrismArrows => "PrismArrows",
            Pickups.Shield => "Shield",
            Pickups.Wings => "Wings",
            Pickups.SpeedBoots => "SpeedBoots",
            Pickups.Mirror => "Mirror",
            Pickups.TimeOrb => "TimeOrb",
            Pickups.DarkOrb => "DarkOrb",
            Pickups.LavaOrb => "LavaOrb",
            Pickups.SpaceOrb => "SpaceOrb",
            Pickups.ChaosOrb => "ChaosOrb",
            Pickups.Bomb => "Bomb",
            Pickups.Gem => "Gem",
            _ => PickupsRegistry.GetPickup(type)!.Name
        };
    }
}

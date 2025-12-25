using FortRise;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.DarkWorldMapButton")]
public class DarkWorldMapButton : TowerFall.DarkWorldMapButton
{
    public DarkWorldMapButton(DarkWorldTowerData tower) : base(tower)
    {
    }

    [MonoModReplace]
    protected override bool GetLocked()
    {
        if (Scene is not MapScene map)
        {
            return false;
        }

        if (map.TowerSet == "TowerFall")
        {
            return !SaveData.Instance.DarkWorld.Towers[Data.ID.X].Revealed;
        }

        var entry = TowerRegistry.DarkWorldTowers[(Data as patch_TowerMapData).LevelData.GetLevelID()];
        var locked = entry.Configuration.ShowLocked?.Invoke(entry);

        return locked is { } l && l;
    }
}
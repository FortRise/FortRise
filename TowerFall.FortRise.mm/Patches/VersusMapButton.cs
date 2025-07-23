using FortRise;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.VersusMapButton")]
public class VersusMapButton : TowerFall.VersusMapButton
{
    public VersusMapButton(VersusTowerData tower) : base(tower)
    {
    }

    protected override bool GetLocked()
    {
        if (Map is null || Map.IsOfficialLevelSet())
        {
            return false;
        }
        var id = (Data as patch_TowerMapData).LevelData.GetLevelID();
        var tower = TowerRegistry.VersusTowers[id];
        bool? locked = tower.Configuration.ShowLocked?.Invoke(tower);

        return locked is {} l && l;
    }
}
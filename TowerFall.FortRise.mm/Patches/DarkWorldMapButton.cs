using MonoMod;

namespace TowerFall;

public class patch_DarkWorldMapButton : DarkWorldMapButton
{
    public patch_DarkWorldMapButton(DarkWorldTowerData tower) : base(tower)
    {
    }

    [MonoModReplace]
    protected override bool GetLocked()
    {
        if (Scene is not MapScene map)
        {
            return false;
        }

        if (map.GetLevelSet() == "TowerFall")
        {
            return !SaveData.Instance.DarkWorld.Towers[Data.ID.X].Revealed;
        }

        return false;
    }
}
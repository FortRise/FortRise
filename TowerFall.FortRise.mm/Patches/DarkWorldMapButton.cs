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
        var levelData = GameData.DarkWorldTowers[Data.ID.X];
        return levelData.GetLevelSet() == "TowerFall" && !SaveData.Instance.DarkWorld.Towers[Data.ID.X].Revealed;
    }
}
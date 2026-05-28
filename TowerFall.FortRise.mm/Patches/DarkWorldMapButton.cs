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
        var tower = GameData.DarkWorldTowers[Data.ID.X];

        if (TowerRegistry.DarkWorldTowers.TryGetValue(tower.LevelID, out var entry))
        {
            var locked = entry.Configuration.ShowLocked?.Invoke(entry);
            if (locked is {} l)
            {
                return l;
            }

            return false;
        }

        return !SaveData.Instance.DarkWorld.Towers[Data.ID.X].Revealed;
    }
}
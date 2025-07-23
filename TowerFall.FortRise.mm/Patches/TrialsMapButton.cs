using FortRise;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.TrialsMapButton")]
public class TrialsMapButton : TowerFall.TrialsMapButton
{
    public TrialsMapButton(TrialsLevelData level) : base(level)
    {
    }

    protected override bool GetLocked()
    {
        if (Map.IsOfficialLevelSet())
        {
            return false;
        }
        var tower = TowerRegistry.TrialTowers[(Data as patch_TowerMapData).LevelData.GetLevelID()];
        var locked = tower.Configuration.ShowLocked?.Invoke(tower);

        return locked is { } l && l;
    }
}
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
        if ((Data as patch_TowerMapData).LevelData.GetLevelSet() == "TowerFall")
        {
            return false;
        }

        var levelID = (Data as patch_TowerMapData).LevelData.GetLevelID();
        
        var tower = TowerRegistry.TrialTowers[levelID[0..(levelID.Length - 2)]];
        var locked = tower.Configuration.ShowLocked?.Invoke(tower);

        return locked is { } l && l;
    }
}
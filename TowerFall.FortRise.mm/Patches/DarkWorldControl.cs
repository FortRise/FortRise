#pragma warning disable CS0626
#pragma warning disable CS0108
using System.Collections;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldControl : DarkWorldControl 
{
    [PostPatchEnableTempVariant]
    public static void ActivateTempVariants(Level level, patch_DarkWorldTowerData.patch_LevelData levelData) {}


    [PostPatchDisableTempVariant]
    public static void DisableTempVariants(Level level) {}

    private extern IEnumerator orig_LevelSequence();

    private IEnumerator LevelSequence() 
    {
        patch_DarkWorldControl.DisableTempVariants(Level);
        var matchSettings = Level.Session.MatchSettings;

        patch_DarkWorldLevelSystem darkWorld = matchSettings.LevelSystem as patch_DarkWorldLevelSystem;
        var levelData = darkWorld.GetLevelData(matchSettings.DarkWorldDifficulty, Level.Session.RoundIndex);
        patch_DarkWorldControl.ActivateTempVariants(Level, levelData);
        if (patch_SaveData.AdventureActive) 
        {
            if (matchSettings.Variants.AlwaysDark)
            {
                Level.OrbLogic.DoDarkOrb();
            }
            if (matchSettings.Variants.SlowTime)
            {
                Level.OrbLogic.DoTimeOrb(delay: true);
            }
            if (matchSettings.Variants.AlwaysLava)
            {
                Level.OrbLogic.DoLavaVariant();
            }
            if (matchSettings.Variants.AlwaysScrolling)
            {
                Level.OrbLogic.StartScroll();
            }
        }

        yield return orig_LevelSequence();
    }
}
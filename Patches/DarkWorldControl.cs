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
}
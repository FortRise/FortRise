using System.Collections;
using System.Collections.Generic;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldControl : DarkWorldControl 
{
    // [MonoModIgnore]
    // [PatchDarkWorldControlNormalLevelSequence]
    // private extern IEnumerator NormalLevelSequence(DarkWorldLevelSystem darkWorld, DarkWorldTowerData.LevelData levelData, List<Pickups> treasure);

    [MonoModIgnore]
    [PatchDarkWorldControlLevelSequence]
    private extern IEnumerator LevelSequence();
}
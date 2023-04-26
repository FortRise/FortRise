using System.Collections;
using System.Collections.Generic;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldControl : DarkWorldControl 
{
    [MonoModIgnore]
    [PatchDarkWorldControlLevelSequence]
    private extern IEnumerator LevelSequence();
}
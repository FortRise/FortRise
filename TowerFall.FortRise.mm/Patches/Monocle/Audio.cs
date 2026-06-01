using System.Collections.Generic;
using System.IO;
using FortRise;
using Microsoft.Xna.Framework.Audio;

namespace Monocle;

public static class patch_Audio 
{
    // Compat together for 1.3.3.1
    public static string ORIGINAL_LOAD_PREFIX = Calc.LOADPATH + "SFX" + Path.DirectorySeparatorChar.ToString();

    internal static List<SFX> loopList;
    internal static List<SFX> pitchList;

}

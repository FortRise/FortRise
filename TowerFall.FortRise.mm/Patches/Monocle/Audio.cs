using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework.Audio;

namespace Monocle;

public static class patch_Audio 
{
    public static Dictionary<string, IAudioSystem> ModAudios = new();
    internal static IAudioSystem currentAudio;
    internal static List<SFX> loopList;

    public static bool TryAccessModAudio(string modName, out IAudioSystem system) 
    {
        if (ModAudios.TryGetValue(modName, out system)) 
        {
            currentAudio = system;
            return true;
        }
        system = null;
        return false;
    }

    public static void StopAudio(AudioStopOptions options) 
    {
        if (currentAudio != null) 
        {
            currentAudio.Stop(options);
            currentAudio = null;
        }
    }
}
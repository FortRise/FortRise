using System.Collections.Generic;
using System.IO;
using FortRise;
using Microsoft.Xna.Framework.Audio;

namespace Monocle;

public static class patch_Audio 
{
    // Compat together for 1.3.3.1
    public static string ORIGINAL_LOAD_PREFIX = Calc.LOADPATH + "SFX" + Path.DirectorySeparatorChar.ToString();
    public static Dictionary<string, IMusicSystem> AudioSystems = new();
    public static Dictionary<string, TrackInfo> TrackMap = new();
    private static IMusicSystem music1Slot;
    private static IMusicSystem music2Slot;
    internal static List<SFX> loopList;
    internal static List<SFX> pitchList;

    internal static void InitMusicSystems() 
    {
        RegisterMusicSystem(new WavMusicSystem(), ".wav");
        RegisterMusicSystem(new OggMusicSystem(), ".ogg");
        RegisterMusicSystem(new VanillaMusicSystem(), ".vanilla");
    }

    public static bool TryGetTrackMap(string name, out TrackInfo info) 
    {
        if (TrackMap.TryGetValue(name, out info))
        {
            return true;
        }

        return false;
    }

    public static void PlayMusic(IMusicSystem system, string name, bool looping) 
    {
        system.Play(name, looping);
        music2Slot = music1Slot;
        music1Slot = system;

        if (music2Slot is not null && !music2Slot.IsStopped)
        {
            CrossFadePlay(music2Slot, music1Slot);
        }
        else 
        {
            music1Slot.Timer = 1.0f;
            music1Slot.StateEffect = StateEffect.Immediate;
        }
    }

    public static void PlayMusicImmediate(IMusicSystem system, string name, bool looping) 
    {
        system.Play(name, looping);
        music2Slot = music1Slot;
        music1Slot = system;

        if (music2Slot is not null && !music2Slot.IsStopped)
        {
            music1Slot.Timer = 1f;
            music1Slot.StateEffect = StateEffect.FadeOut;
        }

        music1Slot.Timer = 0.1f;
        music1Slot.StateEffect = StateEffect.Immediate;
    }

    public static void Update()
    {
        music1Slot?.Update();
        music2Slot?.Update();
    }

    public static void RegisterMusicSystem(IMusicSystem system, string associatedExtension) 
    {
        if (AudioSystems.ContainsKey(associatedExtension)) 
        {
            Logger.Warning($"[Audio System] Conflicting system extensions '{associatedExtension}'. Replacing what comes last.");
        }
        AudioSystems[associatedExtension] = system;
    }

    public static IMusicSystem GetMusicSystemFromExtension(string trackInfoID) 
    {
        if (TryGetTrackMap(trackInfoID, out TrackInfo info))
        {
            return GetMusicSystemFromExtension(info.Resource);
        }

        return AudioSystems[".vanilla"];
    }

    public static IMusicSystem GetMusicSystemFromExtension(IResourceInfo info) 
    {
        if (info.ResourceType == typeof(RiseCore.ResourceTypeOggFile))
        {
            return AudioSystems[".ogg"];
        }

        if (info.ResourceType == typeof(RiseCore.ResourceTypeWavFile))
        {
            return AudioSystems[".wav"];
        }

        Logger.Warning($"There is no associated extension with this music path: '{info.Path}'. Can't find the AudioSystem.");
        return AudioSystems[".vanilla"];
    }

    public static void StopMusic(AudioStopOptions options) 
    {
        music1Slot?.Stop(options);
        music1Slot = null;
    }

    internal static void CrossFadePlay(IMusicSystem fadeOut, IMusicSystem fadeIn)
    {
        if (fadeOut is not null)
        {
            fadeOut.Timer = 1.0f;
            fadeOut.StateEffect = StateEffect.FadeOut;
        }

        if (fadeIn is not null)
        {
            fadeIn.Timer = 1.0f;
            fadeIn.StateEffect = StateEffect.FadeIn;
        }
    }
}

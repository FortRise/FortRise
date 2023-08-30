using System.Collections.Generic;
using System.IO;
using FortRise;
using Microsoft.Xna.Framework.Audio;

namespace Monocle;

public static class patch_Audio 
{
    public static Dictionary<string, IAudioSystem> AudioSystems = new();
    internal static IAudioSystem currentAudio;
    internal static List<SFX> loopList;

    internal static void InitAudioSystems() 
    {
        RegisterAudioSystem(new WavAudioSystem(), ".wav");
        RegisterAudioSystem(new VanillaAudioSystem(), ".vanilla");
    }

    public static void PlaySystem(IAudioSystem system, string name) 
    {
        system.Play(name);
        currentAudio = system;
    }


    public static void RegisterAudioSystem(IAudioSystem system, string associatedExtension) 
    {
        if (AudioSystems.ContainsKey(associatedExtension)) 
        {
            Logger.Warning($"[Audio System] Conflicting system extensions '{associatedExtension}'. Replacing what comes last.");
        }
        AudioSystems[associatedExtension] = system;
    }

    public static IAudioSystem GetAudioSystemFromExtension(string musicPath) 
    {
        var ext = Path.GetExtension(musicPath);
        if (string.IsNullOrEmpty(ext))
            return AudioSystems[".vanilla"];
        if (AudioSystems.TryGetValue(ext, out var audio)) 
        {
            return audio;
        }
        Logger.Warning($"There is no associated extension with this music path: '{musicPath}'. Can't find the AudioSystem.");
        return null;
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
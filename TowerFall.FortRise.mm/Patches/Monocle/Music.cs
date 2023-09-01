using FortRise;
using Microsoft.Xna.Framework.Audio;
using MonoMod;

namespace Monocle;

public static class patch_Music 
{
    private static SoundBank soundBank;
    private static WaveBank waveBank;
    internal static AudioEngine audioEngine;
    private static AudioCategory audioCategory;
    private static string currentSong;
    private static string currentCustomSong;

    public static AudioCategory AudioCategory 
    {
        get => audioCategory;
        set => audioCategory = value;
    }

    public static string CurrentCustomSong 
    {
        get => currentCustomSong;
        set => currentCustomSong = value;
    }

    public static string CurrentSong 
    {
        [MonoModReplace]
        get => currentSong;
        [MonoModReplace]
        set => currentSong = value;
    }

    [MonoModIgnore]
    internal static extern void Initialize();

    [MonoModReplace]
    public static void Play(string filepath) 
    {
        if (currentSong == filepath)
            return;

        Stop();

        if (audioEngine == null)
            return;

        PlayInternal(filepath);
    }

    private static void PlayInternal(string filepath) 
    {
        Stop();
        var audioSystem = patch_Audio.GetMusicSystemFromExtension(filepath);
        if (audioSystem != null) 
        {
            patch_Audio.PlayMusic(audioSystem, filepath);
        }
        currentSong = filepath;
    }

    [MonoModReplace]
    public static void PlayImmediate(string filepath) 
    {
        if (audioEngine == null || Music.CurrentSong == filepath) 
            return;
        
        audioCategory.Stop(AudioStopOptions.Immediate);
        patch_Audio.StopAudio(AudioStopOptions.Immediate);
        
        PlayInternal(filepath);
    }

    [MonoModReplace]
    public static void Stop() 
    {
        if (currentSong != null && SoundHelper.StoredInstance.TryGetValue(currentSong, out var instance) 
            && instance.State == SoundState.Playing)
        {
            currentSong = null;
            instance.Stop();
            return;
        }

        patch_Audio.StopAudio(AudioStopOptions.AsAuthored);
        currentSong = null;
    }

    internal static AudioEngine InternalAccessAudioEngine() 
    {
        return audioEngine;
    }

    internal static SoundBank InternalAccessSoundBank() 
    {
        return soundBank;
    }

    internal static AudioCategory InternalAccessAudioCategory() 
    {
        return audioCategory;
    }
}


public static class MusicExt 
{
    public static AudioEngine GetAudioEngine() 
    {
        return patch_Music.InternalAccessAudioEngine();
    }

    public static SoundBank GetSoundBank() 
    {
        return patch_Music.InternalAccessSoundBank();
    }

    public static AudioCategory GetAudioCategory() 
    {
        return patch_Music.InternalAccessAudioCategory(); 
    }
}
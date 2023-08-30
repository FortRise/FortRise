using System;
using System.IO;
using System.Reflection;
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
        SoundHelper.StopMusic();
        if (filepath.Contains("custom:")) 
        {
            if (SoundHelper.StoredInstance.TryGetValue(filepath, out SoundEffectInstance storedInstance)) 
            {
                SoundHelper.PlayMusic(storedInstance);
            }
            else 
            {
                SoundHelper.PathToSoundWithPrefix(filepath, "custom:", out SoundEffectInstance instance);
                SoundHelper.StoredInstance.Add(filepath, instance);
                SoundHelper.PlayMusic(instance);
            }
            currentSong = filepath;
            return;
        }

        if (audioEngine == null)
            return;

        PlayInternal(filepath);
    }

    private static void PlayInternal(string filepath) 
    {
        Stop();
        var audioSystem = patch_Audio.GetAudioSystemFromExtension(filepath);
        if (audioSystem != null) 
        {
            patch_Audio.PlaySystem(audioSystem, filepath);
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
        SoundHelper.StopMusicImmediate();
        if (filepath.Contains("custom:")) 
        {
            if (SoundHelper.StoredInstance.TryGetValue(filepath, out SoundEffectInstance storedInstance)) 
            {
                SoundHelper.PlayMusic(storedInstance);
            }
            else 
            {
                SoundHelper.PathToSoundWithPrefix(filepath, "custom:", out SoundEffectInstance instance);
                SoundHelper.StoredInstance.Add(filepath, instance);
                SoundHelper.PlayMusic(instance);
            }
            currentSong = filepath;
            return;
        }
        
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

    public static void PlayCustom(this MusicHolder content) 
    {
        string filepath = content.FilePath;
        
        if (content.MusicType == CustomMusicType.AsVanilla) 
        {
            patch_Music.CurrentSong = filepath;
            Music.Stop();
        }
        else
            patch_Music.CurrentCustomSong = filepath;
        SoundHelper.StopMusic();
        if (SoundHelper.StoredInstance.TryGetValue(filepath, out SoundEffectInstance storedInstance)) 
        {
            SoundHelper.PlayMusic(storedInstance);
        }
        else 
        {
            SoundHelper.PathToSound(filepath, out SoundEffectInstance instance);
            SoundHelper.StoredInstance.Add(filepath, instance);
            SoundHelper.PlayMusic(instance);
        }
    }

    public static void PlayImmediateCustom(this MusicHolder content) 
    {
        string filepath = content.FilePath;

        if (content.MusicType == CustomMusicType.AsVanilla) 
        {
            Music.Stop();
            patch_Music.AudioCategory.Stop(AudioStopOptions.Immediate);
            patch_Music.CurrentSong = filepath;
        }
        else
            patch_Music.CurrentCustomSong = filepath;
        SoundHelper.StopMusicImmediate();
        if (SoundHelper.StoredInstance.TryGetValue(filepath, out SoundEffectInstance storedInstance)) 
        {
            SoundHelper.PlayMusic(storedInstance);
        }
        else 
        {
            SoundHelper.PathToSound(filepath, out SoundEffectInstance instance);
            SoundHelper.StoredInstance.Add(filepath, instance);
            SoundHelper.PlayMusic(instance);
        }
        if (content.MusicType == CustomMusicType.AsVanilla)
            patch_Music.CurrentSong = filepath;
        else
            patch_Music.CurrentCustomSong = filepath;
    }

    public static void StopCustom(this MusicHolder content) 
    {
        if (patch_Music.CurrentSong != null && SoundHelper.StoredInstance.TryGetValue(patch_Music.CurrentSong, out var instance) 
            && instance.State == SoundState.Playing)
        {
            if (content.MusicType == CustomMusicType.AsVanilla)
                patch_Music.CurrentSong = null;
            else
                patch_Music.CurrentCustomSong = null;
            instance.Stop();
        }
    }
}

public enum CustomMusicType
{
    AsVanilla,
    FullCustom
}
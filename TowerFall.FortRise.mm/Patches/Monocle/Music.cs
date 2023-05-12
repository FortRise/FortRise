using FortRise;
using Microsoft.Xna.Framework.Audio;
using MonoMod;

namespace Monocle;

public static class patch_Music 
{
    private static SoundBank soundBank;
    private static AudioEngine audioEngine;
    private static AudioCategory audioCategory;
    private static string currentSong;
    private static string currentCustomSong;

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

    [MonoModReplace]
    public static void Play(string filepath) 
    {
        if (currentSong == filepath)
            return;
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
        }
        else if (audioEngine != null)
        {
            soundBank.PlayCue(filepath);
        }
        currentSong = filepath;
    }

    public static void PlayCustom(string filepath) 
    {
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
        currentCustomSong = filepath;
    }

    public static void PlayImmediateCustom(string filepath) 
    {
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
        currentCustomSong = filepath;
    }

    [MonoModReplace]
    public static void PlayImmediate(string filepath) 
    {
        if (audioEngine == null || Music.CurrentSong == filepath) 
            return;
        
        audioCategory.Stop(AudioStopOptions.Immediate);
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
        currentSong = filepath;
        soundBank.PlayCue(filepath);
    }

    public static void StopCustom() 
    {
        if (currentSong != null && SoundHelper.StoredInstance.TryGetValue(currentSong, out var instance) 
            && instance.State == SoundState.Playing)
        {
            currentCustomSong = null;
            instance.Stop();
        }
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
        if (audioEngine != null)
        {
            currentSong = null;
            audioCategory.Stop(AudioStopOptions.AsAuthored);
        }
    }
}
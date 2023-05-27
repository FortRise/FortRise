using System.IO;
using System.Reflection;
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

    public static void PlayCustom(string filepath, CustomMusicType musicType = CustomMusicType.FullCustom) 
    {
        PlayCustom(filepath, musicType);
    }

    public static void PlayCustom(string filepath, ContentAccess access, CustomMusicType musicType = CustomMusicType.FullCustom) 
    {
        switch (access) 
        {
        case ContentAccess.Content:
            filepath = Calc.LOADPATH + filepath;
            break;
        case ContentAccess.ModContent:
            var modDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            filepath = Path.Combine(modDirectory, "Content", filepath).Replace("\\", "/");
            break;
        }
        if (musicType == CustomMusicType.AsVanilla) 
        {
            currentSong = filepath;
            Music.Stop();
        }
        else
            currentCustomSong = filepath;
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

    public static void PlayImmediateCustom(string filepath, CustomMusicType musicType = CustomMusicType.FullCustom) 
    {
        PlayImmediateCustom(filepath, ContentAccess.ModContent, CustomMusicType.FullCustom);
    }

    public static void PlayImmediateCustom(string filepath, ContentAccess access, CustomMusicType musicType = CustomMusicType.FullCustom) 
    {
        switch (access) 
        {
        case ContentAccess.Content:
            filepath = Calc.LOADPATH + filepath;
            break;
        case ContentAccess.ModContent:
            var modDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            filepath = Path.Combine(modDirectory, "Content", filepath).Replace("\\", "/");
            break;
        }

        if (musicType == CustomMusicType.AsVanilla) 
        {
            Music.Stop();
            audioCategory.Stop(AudioStopOptions.Immediate);
            currentSong = filepath;
        }
        else
            currentCustomSong = filepath;
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
        if (musicType == CustomMusicType.AsVanilla)
            currentSong = filepath;
        else
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

    public static void StopCustom(CustomMusicType musicType = CustomMusicType.FullCustom) 
    {
        if (currentSong != null && SoundHelper.StoredInstance.TryGetValue(currentSong, out var instance) 
            && instance.State == SoundState.Playing)
        {
            if (musicType == CustomMusicType.AsVanilla)
                currentSong = null;
            else
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


public static class MusicExt 
{
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
}

public enum CustomMusicType
{
    AsVanilla,
    FullCustom
}
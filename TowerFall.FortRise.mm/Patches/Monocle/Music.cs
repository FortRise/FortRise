using System.Collections.Generic;
using System.IO;
using FortRise;
using Microsoft.Xna.Framework.Audio;
using MonoMod;

namespace Monocle;

public static class patch_Music 
{
    public record struct Queue(string MusicPath, bool IsLooping);
    
    private static List<Queue> musicAwaiter;
    private static SoundBank soundBank;
    private static WaveBank waveBank;
    internal static AudioEngine audioEngine;
    private static AudioCategory audioCategory;
    private static string currentSong;
    private static float masterVolume;
    private static bool fromCue;

    public static AudioCategory AudioCategory 
    {
        get => audioCategory;
        set => audioCategory = value;
    }

    public static string CurrentSong 
    {
        [MonoModReplace]
        get => currentSong;
        [MonoModReplace]
        set => currentSong = value;
    }

    [MonoModReplace]
    internal static void Initialize() 
    {
        try
        {
            musicAwaiter = new();
            var dirPath = Path.GetFullPath(Directory.GetCurrentDirectory());
            audioEngine = new AudioEngine(Path.Combine(dirPath, "Content/Music/Win/TowerFallMusic.xgs"));
            soundBank = new SoundBank(audioEngine, Path.Combine(dirPath, "Content/Music/Win/MusicSoundBank.xsb"));
            waveBank = new WaveBank(audioEngine, Path.Combine(dirPath, "Content/Music/Win/MusicWaveBank.xwb"));
            audioCategory = audioEngine.GetCategory("Music");
            audioCategory.SetVolume(2f * masterVolume);
        }
        catch
        {
            audioEngine = null;
            soundBank = null;
            waveBank = null;
        }
    }

    [MonoModReplace]
    public static void Play(string filepath)
    {
        Play(filepath, true);
    }

    [MonoModReplace]
    public static void Play(string filepath, bool looping) 
    {
        if (currentSong == filepath)
        {
            return;
        }

        if (audioEngine == null)
        {
            Stop();
            return;
        }

        PlayInternal(filepath, looping);
    }

    private static void PlayInternal(string filepath, bool looping) 
    {
        if (!fromCue)
        {
            Stop();
        }
        var audioSystem = patch_Audio.GetMusicSystemFromExtension(filepath);
        if (audioSystem != null) 
        {
            fromCue = audioSystem.GetType() == typeof(VanillaMusicSystem);
            patch_Audio.PlayMusic(audioSystem, filepath, looping);
        }
        currentSong = filepath;
    }

    [MonoModReplace]
    public static void PlayImmediate(string filepath)
    {
        PlayImmediate(filepath, true);
    }

    [MonoModReplace]
    public static void PlayImmediate(string filepath, bool looping) 
    {
        if (audioEngine == null || Music.CurrentSong == filepath) 
            return;
        
        audioCategory.Stop(AudioStopOptions.Immediate);
        patch_Audio.StopMusic(AudioStopOptions.Immediate);
        
        PlayInternal(filepath, looping);
    }

    [MonoModReplace]
    public static void Stop() 
    {
        patch_Audio.StopMusic(AudioStopOptions.AsAuthored);
        currentSong = null;
    }

    public static void PlayNext(string filepath, bool looping)
    {
        musicAwaiter.Add(new Queue(filepath, looping));
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

    [MonoModReplace]
    internal static void Update()
    {
        if (audioEngine is null)
        {
            return;
        }
        audioEngine.Update();

        if (musicAwaiter.Count == 0)
        {
            return;
        }

        if (currentSong == null)
        {
            return;
        }

        if (fromCue)
        {
            var cue = soundBank.GetCue(currentSong);
            if (cue.IsStopped)
            {
                var newSong = musicAwaiter[0];
                musicAwaiter.Remove(newSong);
                PlayImmediate(newSong.MusicPath, newSong.IsLooping);
            }

            return;
        }

        var currentSystem = patch_Audio.GetMusicSystemFromExtension(currentSong);
        if (currentSystem.IsStopped)
        {
            var newSong = musicAwaiter[0];
            musicAwaiter.Remove(newSong);
            PlayImmediate(newSong.MusicPath, newSong.IsLooping);
        }
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
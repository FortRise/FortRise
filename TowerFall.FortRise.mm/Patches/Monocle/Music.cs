using System;
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

    public static Dictionary<string, IMusicSystem> AudioSystems;
    public static Dictionary<string, TrackInfo> TrackMap = new();
    private static IMusicSystem music1Slot;
    private static IMusicSystem music2Slot;

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
            AudioSystems = new Dictionary<string, IMusicSystem>()
            {
                {".wav", new WavMusicSystem()},
                {".ogg", new OggMusicSystem()},
                {".vanilla", new VanillaMusicSystem()}
            };
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

    private static void PlayInternal(string filepath, bool looping, bool immediate = false) 
    {
        //if (!fromCue)
        //{
        //    Stop();
        //}
        var audioSystem = GetMusicSystemFromExtension(filepath);
        if (audioSystem != null) 
        {
            fromCue = audioSystem.GetType() == typeof(VanillaMusicSystem);
            if (immediate)
            {
                PlayMusicImmediate(audioSystem, filepath, looping);
            }
            else 
            {
                PlayMusic(audioSystem, filepath, looping);
            }
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
        {
            return;
        }
        
        audioCategory.Stop(AudioStopOptions.Immediate);
        StopMusic(AudioStopOptions.Immediate);
        
        PlayInternal(filepath, looping, true);
    }

    [MonoModReplace]
    public static void Stop() 
    {
        StopMusic(AudioStopOptions.AsAuthored);
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

        music1Slot?.Update();
        music2Slot?.Update();

        if (musicAwaiter.Count == 0 || currentSong == null)
        {
            return;
        }

        if (fromCue)
        {
            using var cue = soundBank.GetCue(currentSong);
            if (cue.IsStopped)
            {
                var newSong = musicAwaiter[0];
                musicAwaiter.Remove(newSong);
                PlayImmediate(newSong.MusicPath, newSong.IsLooping);
            }

            return;
        }

        var currentSystem = GetMusicSystemFromExtension(currentSong);
        if (currentSystem.IsStopped)
        {
            var newSong = musicAwaiter[0];
            musicAwaiter.Remove(newSong);
            PlayImmediate(newSong.MusicPath, newSong.IsLooping);
        }
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
        music2Slot = music1Slot;
        music1Slot = system;

        system.Play(name, looping);

        if (music2Slot == music1Slot && fromCue)
        {
            return;
        }


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
            music2Slot.Timer = 1f;
            music2Slot.StateEffect = StateEffect.FadeOut;
        }

        music1Slot.Timer = 0.1f;
        music1Slot.StateEffect = StateEffect.Immediate;
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


public static class MusicExt 
{
    extension(Music)
    {
        public static AudioEngine AudioEngine => patch_Music.InternalAccessAudioEngine();
        public static SoundBank SoundBank => patch_Music.InternalAccessSoundBank();
        public static AudioCategory AudioCategory => patch_Music.InternalAccessAudioCategory();
    }

    [Obsolete("Use Music.AudioEngine property instead")]
    public static AudioEngine GetAudioEngine() => Music.AudioEngine;

    [Obsolete("Use Music.SoundBank property instead")]
    public static SoundBank GetSoundBank() => Music.SoundBank;

    [Obsolete("Use Music.AudioCategory property instead")]
    public static AudioCategory GetAudioCategory() => Music.AudioCategory;
    
}

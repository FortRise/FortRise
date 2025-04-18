using System.IO;
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
    private static float masterVolume;

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
    internal static void Initialize() 
    {
        try
        {
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
        if (currentSong == filepath)
            return;

        Stop();

        if (audioEngine == null)
            return;

        PlayInternal(filepath);
    }

    public static void Play(TrackInfo info) 
    {
        if (currentSong == info.ResourcePath)
            return;
        
        Stop();

        if (audioEngine == null)
            return;

        var audioSystem = patch_Audio.GetMusicSystemFromExtension(info.ResourcePath);
        if (audioSystem != null) 
        {
            patch_Audio.PlayMusic(audioSystem, info);
        }
        currentSong = info.ResourcePath;
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
        patch_Audio.StopMusic(AudioStopOptions.Immediate);
        
        PlayInternal(filepath);
    }

    public static void PlayImmediate(TrackInfo info) 
    {
        if (currentSong == info.ResourcePath)
            return;
        
        audioCategory.Stop(AudioStopOptions.Immediate);
        patch_Audio.StopMusic(AudioStopOptions.Immediate);

        if (audioEngine == null)
            return;

        var audioSystem = patch_Audio.GetMusicSystemFromExtension(info.ResourcePath);
        if (audioSystem != null) 
        {
            patch_Audio.PlayMusic(audioSystem, info);
        }
        currentSong = info.ResourcePath;
    }

    [MonoModReplace]
    public static void Stop() 
    {
        patch_Audio.StopMusic(AudioStopOptions.AsAuthored);
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

    [MonoModReplace]
    internal static void Update()
    {
        audioEngine?.Update();
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
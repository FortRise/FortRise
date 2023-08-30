using System.IO;
using Microsoft.Xna.Framework.Audio;
using Monocle;

namespace FortRise;

public class VanillaAudioSystem : IAudioSystem
{
    private SoundBank soundBank;
    private AudioCategory audioCategory;
    public VanillaAudioSystem() 
    {
        soundBank = MusicExt.GetSoundBank();
        audioCategory = MusicExt.GetAudioCategory();
    }

    public void Add(string name, Stream stream) {}

    public void Pause()
    {
        audioCategory.Pause();
    }

    public void Play(string name)
    {
        if (MusicExt.GetAudioEngine() == null)
            return;
        soundBank.PlayCue(name);
    }

    public void Resume()
    {
        audioCategory.Resume();
    }

    public void Stop(AudioStopOptions options)
    {
        if (MusicExt.GetAudioEngine() == null)
            return;
        audioCategory.Stop(options);
    }
}
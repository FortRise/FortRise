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

    public void Add(string name, Stream stream)
    {
        throw new System.NotImplementedException();
    }

    public void Play(string name)
    {
        if (MusicExt.GetAudioEngine() == null)
            return;
        soundBank.PlayCue(name);
    }

    public void Stop(AudioStopOptions options)
    {
        if (MusicExt.GetAudioEngine() == null)
            return;
        audioCategory.Stop(options);
    }
}
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Monocle;

namespace FortRise;

public class XactAudioSystem : IAudioSystem
{
    public SoundBank Sound;
    public WaveBank Wave;
    public AudioEngine Engine;
    private Cue currentCue;

    public XactAudioSystem(AudioEngine engine) 
    {
        Engine = engine;
    }

    public XactAudioSystem() 
    {
        Engine = MusicExt.GetAudioEngine();
    }

    public void Play(string name)
    {
        currentCue = Sound.GetCue(name);
        currentCue.Play();
    }

    public void Stop(AudioStopOptions options)
    {
        if (currentCue != null) 
        {
            currentCue.Stop(options);
            currentCue = null;
        }
    }

    public void Add(string name, Stream stream)
    {
        throw new System.NotImplementedException();
    }
}
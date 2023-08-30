using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Monocle;

namespace FortRise;

public class WavAudioSystem : IAudioSystem
{
    public Dictionary<string, SoundEffect> SFXMap = new();
    private SoundEffectInstance current;

    public WavAudioSystem() 
    {
    }

    public void Add(string name, Stream stream)
    {
        SFXMap.Add(name, SoundEffect.FromStream(stream));
    }

    public void Play(string name)
    {
        Stop(AudioStopOptions.Immediate);

        current = SFXMap[name].CreateInstance();
        current.Volume = Music.MasterVolume;
        current.IsLooped = true;
        current.Play();
    }

    public void Stop(AudioStopOptions options)
    {
        if (current != null) 
        {
            current.Stop(true);
            current.Dispose();
            current = null;
        }
    }
}
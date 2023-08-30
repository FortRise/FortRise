using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace FortRise;

public interface IAudioSystem 
{
    void Play(string name);
    void Resume();
    void Pause();
    void Stop(AudioStopOptions options);

    void Add(string name, Stream stream);
}

public static class AudioSystemExt 
{
    public static XactAudioSystem ToXact(this IAudioSystem system) 
    {
        return system as XactAudioSystem;
    }
}
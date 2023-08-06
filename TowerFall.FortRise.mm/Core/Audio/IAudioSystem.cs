using Microsoft.Xna.Framework.Audio;

namespace FortRise;

public interface IAudioSystem 
{
    void Play(string name);

    void Stop(AudioStopOptions options);
}

public static class AudioSystemExt 
{
    public static XactAudioSystem ToXact(this IAudioSystem system) 
    {
        return system as XactAudioSystem;
    }
}
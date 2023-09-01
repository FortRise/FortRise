using Microsoft.Xna.Framework.Audio;
using Monocle;

namespace FortRise;


public class OggMusicSystem : IMusicSystem
{
    private AudioTrack current;

    public void Pause()
    {
        current?.Pause();
    }

    public void Play(string name)
    {
        current = patch_Audio.TrackMap[name].Create();
        current.Looping = true;
        current.Play();
    }

    public void Resume()
    {
        current?.Resume();
    }

    public void Seek(uint seekFrames)
    {
        current?.Seek(seekFrames);
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
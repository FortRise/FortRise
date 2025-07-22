using Microsoft.Xna.Framework.Audio;

namespace FortRise;

public interface IMusicSystem 
{
    public bool IsStopped { get; }

    void Play(string name, bool looping);
    void Resume();
    void Pause();
    void Stop(AudioStopOptions options);
    void Seek(uint seekFrames);
}
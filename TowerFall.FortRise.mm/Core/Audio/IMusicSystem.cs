using Microsoft.Xna.Framework.Audio;

namespace FortRise;

public interface IMusicSystem 
{
    bool IsStopped { get; }
    float Timer { get; set; }

    StateEffect StateEffect { get; set; }

    void Play(string name, bool looping);
    void Resume();
    void Pause();
    void Stop(AudioStopOptions options);
    void Seek(uint seekFrames);

    void Update();
}

public enum StateEffect
{
    None,
    FadeIn,
    FadeOut,
    Immediate
}

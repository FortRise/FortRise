using Microsoft.Xna.Framework.Audio;
using Monocle;

namespace FortRise;

public class VanillaMusicSystem : IMusicSystem
{
    public bool IsStopped
    {
        get
        {
            if (current is null)
            {
                return true;
            }
            return soundBank.GetCue(current).IsStopped;
        }
    }

    public float Timer { get; set; }

    public StateEffect StateEffect { get; set; }

    private SoundBank soundBank;
    private AudioCategory audioCategory;
    private string current;
    
    public VanillaMusicSystem()
    {
        soundBank = MusicExt.GetSoundBank();
        audioCategory = MusicExt.GetAudioCategory();
    }

    public void Pause()
    {
        audioCategory.Pause();
    }

    public void Play(string name, bool looping) // looping depends on FACT itself
    {
        if (MusicExt.GetAudioEngine() == null)
        {
            return;
        }

        soundBank.PlayCue(name);
        current = name;
    }

    public void Resume()
    {
        audioCategory.Resume();
    }

    public void Seek(uint seekFrames)
    {
        Logger.Log("[FACT Music System] Seek is not supported!");
    }

    public void Stop(AudioStopOptions options)
    {
        if (MusicExt.GetAudioEngine() == null)
            return;
        audioCategory.Stop(options);
    }

    public void Update()
    {
        Timer -= Engine.DeltaTime;
    }
}

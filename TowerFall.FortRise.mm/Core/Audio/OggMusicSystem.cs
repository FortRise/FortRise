using Microsoft.Xna.Framework.Audio;
using Monocle;

namespace FortRise;


public class OggMusicSystem : IMusicSystem
{
    private AudioTrack current;

    public bool IsStopped
    {
        get
        {
            if (current is not null)
            {
                return current.IsStopped;
            }

            return true;
        }
    }

    public float Timer { get; set; }

    public StateEffect StateEffect { get; set; }

    public void Pause()
    {
        current?.Pause();
    }

    public void Play(string name, bool looping)
    {
        Stop(AudioStopOptions.Immediate);

        if (patch_Audio.TryGetTrackMap(name, out var info)) 
        {
            current = info.Create();
            current.Looping = looping;
            current.Play();
            return;
        }
        Logger.Error($"[OGG Music System] No audio file named '{name}' exists on the Music Folder"); 
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

    public void Update()
    {
        if (current is null)
        {
            return;
        }
        if (Timer <= 0)
        {
            return;
        }

        Timer -= Engine.DeltaTime;

        switch (StateEffect)
        {
            case StateEffect.FadeIn:
                if (Timer <= 0)
                {
                    current.Volume = Music.MasterVolume;
                    StateEffect = StateEffect.None;
                    return;
                }
                current.Volume = Music.MasterVolume * (1f - Timer);
                break;
            case StateEffect.FadeOut:
                if (Timer <= 0)
                {
                    current.Volume = 0;
                    if (!current.IsStopped)
                    {
                        Stop(AudioStopOptions.Immediate);
                    }
                    return;
                }

                current.Volume = Music.MasterVolume * Timer;
                break;
            case StateEffect.Immediate:
                current.Volume = Music.MasterVolume;
                break;
            default:
                break;
        }
    }
}

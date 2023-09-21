using System;
using Microsoft.Xna.Framework.Audio;
using Monocle;

namespace FortRise;

public abstract class AudioTrack : IDisposable
{
    protected SoundEffectInstance soundEffect;

    public virtual bool Looping 
    { 
        get => soundEffect.IsLooped;
        set => soundEffect.IsLooped = value;
    }

    public void Play() 
    {
        soundEffect.Volume = Music.MasterVolume;
        soundEffect.Play();
    }

    public void Stop(bool immediate = true) 
    {
        soundEffect.Stop(immediate);
    }

    public void Pause() 
    {
        soundEffect.Pause();
    }

    public void Resume() 
    {
        soundEffect.Resume();
    }

    public abstract void Seek(uint sampleFrame);

    public virtual void CreateSoundEffect(int sampleRate, int channels) 
    {
        var dynamicSoundEffectInstance = new DynamicSoundEffectInstance((int)sampleRate, (AudioChannels)channels);
        dynamicSoundEffectInstance.BufferNeeded += OnBufferNeeded;
        dynamicSoundEffectInstance.SubmitFloatBufferEXT(new float[0]);
        soundEffect = dynamicSoundEffectInstance;
    }

    private void OnBufferNeeded(object sender, EventArgs e)
    {
        if (soundEffect is DynamicSoundEffectInstance instance) 
        {
            while (instance.PendingBufferCount < 3) 
            {
                instance.SubmitFloatBufferEXT(CreateBuffer(3 * 1024));
            }
        }
    }

    public abstract float[] CreateBuffer(int sample);
    

    public virtual void Dispose()
    {
        if (soundEffect is DynamicSoundEffectInstance instance) 
            instance.BufferNeeded -= OnBufferNeeded;
        
        soundEffect.Dispose();
        soundEffect = null;
    }
}

using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace FortRise;

public class WavAudioTrack : AudioTrack
{
    public WavAudioTrack(Stream stream) 
    {
        var sfx = SoundEffect.FromStream(stream);
        stream.Close();
        soundEffect = sfx.CreateInstance();
    }

    public override void Dispose()
    {
        soundEffect.Dispose();
        soundEffect = null;
    }

    public override float[] CreateBuffer(int countSample)
    {
        return null;
    }

    public override void Seek(uint sampleFrame)
    {
        Logger.Warning("[WAV AUDIO TRACK] Not implemented or not supported!");
    }
}
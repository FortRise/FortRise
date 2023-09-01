using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FortRise;

public class OggAudioTrack : AudioTrack, IDisposable
{
    private IntPtr handle;
    private IntPtr fileDataPtr;
    private uint loopStart;
    private uint loopEnd;
    private uint sampleCount;

    private bool oggLooping;

    public override bool Looping { get => oggLooping; set => oggLooping = value; }

    public OggAudioTrack(Stream stream) 
    {
        byte[] data = new byte[stream.Length];
        int remaining = data.Length; 
        while (remaining != 0) 
        { 
            remaining -= stream.Read(data, data.Length - remaining, remaining);
        }
        stream.Close();
        var size = data.Length;
        fileDataPtr = Marshal.AllocHGlobal(size);
        Marshal.Copy(data, 0, fileDataPtr, size);

        handle = FAudio.stb_vorbis_open_memory(fileDataPtr, size, out int error, IntPtr.Zero);
        if (error == 0) 
        {
            var info = FAudio.stb_vorbis_get_info(handle);
            CreateSoundEffect((int)info.sample_rate, info.channels);
            sampleCount = FAudio.stb_vorbis_stream_length_in_samples(handle);
            FindLoop();
            if (loopEnd == 0)
                loopEnd = sampleCount;
        }
    }

    public override void Dispose()
    {
        Marshal.FreeHGlobal(fileDataPtr);
        FAudio.stb_vorbis_close(handle);
        base.Dispose();
    }

    public override float[] CreateBuffer()
    {
        var info = FAudio.stb_vorbis_get_info(handle);
        float[] buffer;
        var lengthInFloats = FAudio.stb_vorbis_stream_length_in_samples(handle);
        if (oggLooping) 
        {
            int currentSample = FAudio.stb_vorbis_get_sample_offset(handle);
            int samplesLeft = (int)(loopEnd - currentSample);
            if (samplesLeft > 0) 
            {
                buffer = new float[Math.Min(lengthInFloats, samplesLeft) * info.channels];
                FAudio.stb_vorbis_get_samples_float_interleaved(handle, info.channels, buffer, buffer.Length);
            }
            else 
            {
                buffer = new float[(lengthInFloats - loopStart) * info.channels];
                int count = FAudio.stb_vorbis_get_samples_float_interleaved(handle, info.channels, buffer, buffer.Length);
                Seek(loopStart);
                FAudio.stb_vorbis_get_samples_float_interleaved(handle, info.channels, buffer, buffer.Length - count);
            }
        }
        else 
        {
            buffer = new float[lengthInFloats * info.channels];
            FAudio.stb_vorbis_get_samples_float_interleaved(handle, info.channels, buffer, buffer.Length);
        }

        return buffer;
    }

    private void FindLoop()
    {
        loopStart = Decode("LOOPSTART=");
        loopEnd = Decode("LOOPEND=");

        uint Decode(string field) 
        {
            uint value = 0;

            FAudio.stb_vorbis_comment commment = FAudio.stb_vorbis_get_comment(handle);
            for (int i = 0; i < commment.comment_list_length; i++) 
            {
                nint pointer = Marshal.ReadIntPtr(commment.comment_list, i * Marshal.SizeOf<IntPtr>());
                string s = Marshal.PtrToStringAnsi(pointer);
                if (s.StartsWith(field)) 
                {
                    UInt32.TryParse(s.Substring(field.Length), out value);
                }
            }

            return value;
        }
    }

    public override void Seek(uint sampleFrame)
    {
        if (sampleFrame < 0)
            sampleFrame = 0;
        else if (sampleFrame > sampleCount)
            sampleFrame = sampleCount;
        FAudio.stb_vorbis_seek(handle, sampleFrame);
    }
}

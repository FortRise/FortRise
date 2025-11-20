using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FortRise;

public class OggAudioTrack : AudioTrack, IDisposable
{
    private readonly IntPtr handle;
    private readonly unsafe void *fileDataPtr;
    private uint loopStart;
    private uint loopEnd;
    private uint sampleCount;
    private int channels;

    private bool oggLooping;

    public override bool Looping { get => oggLooping; set => oggLooping = value; }

    public unsafe OggAudioTrack(Stream stream) 
    {
        fileDataPtr = NativeMemory.Alloc((nuint)stream.Length);
        var dataSpan = new Span<byte>(fileDataPtr, (int)stream.Length);
        stream.ReadExactly(dataSpan);
        stream.Close();

        int size = dataSpan.Length;

        handle = FAudio.stb_vorbis_open_memory((IntPtr)fileDataPtr, size, out int error, IntPtr.Zero);

        if (error == 0) 
        {
            var info = FAudio.stb_vorbis_get_info(handle);
            channels = info.channels;
            CreateSoundEffect((int)info.sample_rate, info.channels);
            sampleCount = FAudio.stb_vorbis_stream_length_in_samples(handle);
            FindLoop();
            if (loopEnd == 0)
            {
                loopEnd = sampleCount;
            }
        }
    }

    public override unsafe void Dispose()
    {
        NativeMemory.Free(fileDataPtr);

        FAudio.stb_vorbis_close(handle);
        base.Dispose();
        GC.SuppressFinalize(this);
    }
    
    public unsafe override float[] CreateBuffer(int countSample)
    {
        float[] buffer = new float[countSample * channels];
        int currentSample = FAudio.stb_vorbis_get_sample_offset(handle);

        int sample = FAudio.stb_vorbis_get_samples_float_interleaved(handle, channels, buffer, buffer.Length);

        if (oggLooping) 
        {
            int samplesLeft = (int)(loopEnd - currentSample);
            if (samplesLeft < 0)
            {
                samplesLeft = 0;
            }

            if (samplesLeft < countSample) 
            {
                if (sample < countSample) 
                {
                    Logger.Warning($"[OGG Decode] Asked for {samplesLeft} received {sample} samples prior to loop");
                }

                Seek(loopStart);
                int bufferBase = samplesLeft * channels;
                fixed (float* p = &buffer[bufferBase]) 
                {
                    sample = FAudio.stb_vorbis_get_samples_float_interleaved(handle, channels, (IntPtr)p, buffer.Length - bufferBase);
                }
                sample += samplesLeft;
            }
        }

        if (sample > 0 && sample != countSample)
        {
            Logger.Warning($"[OGG Decode] Wanted {countSample} but actual sample count is {sample}");
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
                    _ = uint.TryParse(s.AsSpan(field.Length), out value);
                }
            }

            return value;
        }
    }

    public override void Seek(uint sampleFrame)
    {
        if (sampleFrame < 0)
        {
            sampleFrame = 0;
        }
        else if (sampleFrame > sampleCount)
        {
            sampleFrame = sampleCount;
        }

        _ = FAudio.stb_vorbis_seek(handle, sampleFrame);
    }
}

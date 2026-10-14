using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FortRise;

public class WavAudioTrack : AudioTrack
{
    private readonly unsafe void *fileDataPtr;
    private readonly int dataOffset;
    private readonly int dataLength;
    private readonly int bitsPerSample;

    private uint loopStart;
    private uint loopEnd;
    private uint sampleCount;
    private int channels;
    private int sampleRate;
    private int currentBytePosition;

    private bool looping;

    public override bool Looping { get => looping; set => looping = value; }

    public unsafe WavAudioTrack(Stream stream) 
    {
        long length = stream.Length;
        fileDataPtr = NativeMemory.Alloc((nuint)length);

        var dataSpan = new Span<byte>(fileDataPtr, (int)length);
        stream.ReadExactly(dataSpan);
        stream.Close();

        ParseHeader(dataSpan, out sampleRate, out channels, out bitsPerSample, out dataOffset, out dataLength);

        int bytesPerSample = bitsPerSample / 8;
        int blockAlign = channels * bytesPerSample;

        sampleCount = (uint)(dataLength / blockAlign);

        loopStart = 0;
        loopEnd = sampleCount;
        currentBytePosition = dataOffset;

        CreateSoundEffect(sampleRate, channels);
    }

    public override unsafe void Dispose()
    {
        NativeMemory.Free(fileDataPtr);
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public override unsafe float[] CreateBuffer(int countSample)
    {
        int bytesPerSample = bitsPerSample / 8;
        int blockAlign = channels * bytesPerSample;
        int requestedBytes = countSample * blockAlign;

        var buffer = new float[countSample * channels];

        int bytesToRead = Math.Min(requestedBytes, dataOffset + dataLength - currentBytePosition);

        if (bytesToRead > 0)
        {
            byte *srcPtr = (byte*)fileDataPtr + currentBytePosition;
            ConvertPcmToFloat(srcPtr, buffer, bytesToRead, bitsPerSample);
            currentBytePosition += bytesToRead;
        }

        int samplesRead = bytesToRead / blockAlign;

        if (looping && samplesRead < countSample)
        {
            Seek(loopStart);

            int samplesLeftToRead = countSample - samplesRead;
            int wrapBytesToRead = Math.Min(samplesLeftToRead * blockAlign, dataLength);

            if (wrapBytesToRead > 0)
            {
                byte *srcPtr = (byte*)fileDataPtr + dataOffset;

                int bufferBase = samplesRead * channels;

                float[] wrapBuffer = new float[samplesLeftToRead * channels];
                ConvertPcmToFloat(srcPtr, wrapBuffer, wrapBytesToRead, bitsPerSample);
                Array.Copy(wrapBuffer, 0, buffer, bufferBase, wrapBuffer.Length);

                currentBytePosition = dataOffset + wrapBytesToRead;
                samplesRead += wrapBytesToRead / blockAlign;
            }
        }

        if (samplesRead > 0 && samplesRead != countSample)
        {
            Logger.Warning($"[WAV Decode] Wanted {countSample} samples but actual sample count is {samplesRead}");
        }

        return buffer;
    }

    public override void Seek(uint sampleFrame)
    {
        if (sampleFrame > sampleCount)
        {
            sampleFrame = sampleCount;
        }

        int bytesPerSample = bitsPerSample / 8;
        int blockAlign = channels * bytesPerSample;
        currentBytePosition = dataOffset + (int)(sampleFrame * blockAlign);
    }

    private static unsafe void ConvertPcmToFloat(byte *src, float[] dest, int byteCount, int bits)
    {
        if (bits == 16)
        {
            int sampleCount = byteCount / 2;
            short *shortPtr = (short*)src;

            for (int i = 0; i < sampleCount; i += 1)
            {
                dest[i] = shortPtr[i] / (float)(short.MaxValue + 1);
            }
        }
        else if (bits == 8)
        {
            int sampleCount = byteCount;
            for (int i = 0; i < sampleCount; i += 1)
            {
                dest[i] = (src[i] - (float)(sbyte.MaxValue + 1)) / (sbyte.MaxValue + 1);
            }
        }
        else 
        {
            throw new NotSupportedException($"{bits}-bit PCM WAV files are not supported.");
        }
    }

    private static void ParseHeader(
        Span<byte> header, 
        out int sampleRate,
        out int channels,
        out int bitsPerSample,
        out int dataOffset,
        out int dataLength) 
    {
        if (header[0] != 'R' || header[1] != 'I' || header[2] != 'F' || header[3] != 'F' ||
            header[8] != 'W' || header[9] != 'A' || header[10] != 'V' || header[11] != 'E')
        {
            throw new InvalidDataException("Invalid WAV file format.");
        }

        sampleRate = 0;
        channels = 0;
        bitsPerSample = 0;
        dataOffset = 0;
        dataLength = 0;

        int position = 12;
        while (position < header.Length - 8)
        {
            var chunkIdSpan = header.Slice(position, 4);
            int chunkSize = BitConverter.ToInt32(header.Slice(position + 4, 4));

            position += 8;

            if (chunkIdSpan[0] == 'f' && chunkIdSpan[1] == 'm' &&
                chunkIdSpan[2] == 't' && chunkIdSpan[3] == ' ')
            {
                int audioFormat = BitConverter.ToInt16(header.Slice(position, 2));
                if (audioFormat != 1)
                {
                    throw new NotSupportedException($"Only compressed PCM WAV files are supported. (format: {audioFormat})");
                }

                channels = BitConverter.ToInt16(header.Slice(position + 2, 2));
                sampleRate = BitConverter.ToInt32(header.Slice(position + 4, 4));
                bitsPerSample = BitConverter.ToInt16(header.Slice(position + 14, 2));
            }
            else if (chunkIdSpan[0] == 'd' && chunkIdSpan[1] == 'a' && chunkIdSpan[2] == 't' && chunkIdSpan[3] == 'a')
            {
                dataOffset = position;
                dataLength = chunkSize;
                break;
            }

            position += chunkSize;
        }

        if (dataOffset == 0 || sampleRate == 0)
        {
            throw new InvalidDataException("WAV file is missing, required 'fmt' or 'data' chunks.");
        }
    }
}

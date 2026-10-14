/*
  MIT License
  Copyright © 2006 The Mono.Xna Team
  All rights reserved.

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FortRise;

internal static class XWBExtract
{
    private struct ExtractedXWBData 
    {
        public int Index;
        public int Codec;
        public int Channels;
        public int Rate;
        public int Alignment;
        public byte[] Data;
    }

    public static void Extract(string xwbInput, string xwbOutput)
    {
        string[] titles = [
            "FrostfangKeep", 
            "GameOver", 
            "KingsCourt", 
            "Mirage", 
            "Moonstone", 
            "SacredGround", 
            "SunkenCity",
            "TheArchives", 
            "Thornwood",
            "Title",
            "TowerForge",
            "TwilightSpire", 
            "UnlockMoonstone",
            "UnlockSunkenCity",
            "UnlockTowerForge", 
            "VictoryBlue",
            "VictoryGreen",
            "VictoryOrange",
            "VictoryPink",
            "VictoryTeam",
            "VictoryWhite",
            "VictoryYellow",
            "WhiteArcherReveal", 
            "Ascension", 
            "Backfire", 
            "Flight", 
            "Boss",
            "UnlockTwilightSpire", 
            "VictoryPurple", 
            "UnlockAscension", 
            "AltTitle", 
            "VictoryCyan",
            "BattleCyan", 
            "Gauntlet", 
            "TheAmaranth", 
            "Dreadwood",
            "Darkfang", 
            "Cataclysm", 
            "VictoryRed",
            "VictoryKyle",
            "DarkBoss", 
            "VictoryOrangeAlt",
            "VictoryWhiteAlt", 
            "TheTale", 
            "ChipTitle" 
        ];

        using var stream = File.OpenRead(xwbInput);
        var reader = new BinaryReader(stream);

        int format = 0;
        int playRegionOffset = 0;
        int playRegionLength = 0;

        reader.ReadBytes(4);

        int version = reader.ReadInt32();

        if (version != 46)
        {
            throw new NotSupportedException("XWB Version not equal to 46 is unsupported.");
        }

        reader.ReadInt32(); // skip this

        var segmentOffsets = new int[5];
        var segmentLengths = new int[5];

        for (int i = 0; i < 5; i += 1)
        {
            segmentOffsets[i] = reader.ReadInt32();
            segmentLengths[i] = reader.ReadInt32();
        }

        reader.BaseStream.Seek(segmentOffsets[0], SeekOrigin.Begin);

        int flags = reader.ReadInt32();
        int entryCount = reader.ReadInt32();

        reader.ReadBytes(64); // skips name

        int entryMetadataElementSize = reader.ReadInt32();
        reader.ReadInt32(); // skips entryNameElementSize
        reader.ReadInt32(); // skips alignment
        int offset = segmentOffsets[1];

        if ((flags & 0x00020000) != 0) 
        {
            throw new NotSupportedException("Compact Wavebanks are not supported.");
        }

        int playRegionSegmentOffset = segmentOffsets[4];

        var list = new List<ExtractedXWBData>(45);

        for (int i = 0; i < entryCount; i += 1)
        {
            // we would like to skip some part of this process
            reader.BaseStream.Position = offset;

            if (entryMetadataElementSize >= 4)
            {
                reader.ReadInt32();
            }

            if (entryMetadataElementSize >= 8)
            {
                format = reader.ReadInt32();
            }

            if (entryMetadataElementSize >= 12)
            {
                playRegionOffset = reader.ReadInt32();
            }

            if (entryMetadataElementSize >= 16)
            {
                playRegionLength = reader.ReadInt32();
            }

            if (entryMetadataElementSize >= 20)
            {
                reader.ReadInt32(); // skips loopRegionOffset
            }

            if (entryMetadataElementSize >= 24)
            {
                reader.ReadInt32(); // skips loopRegionLength
            }

            offset += entryMetadataElementSize;
            playRegionOffset += playRegionSegmentOffset;

            // decode 

            int codec = format & ((1 << 2) - 1);
            int channels = (format >> 2) & ((1 << 3) - 1);
            int rate = (format >> (2 + 3)) & ((1 << 18) - 1);
            int alignment = (format >> (2 + 3 + 18)) & ((1 << 8) - 1);

            // this is where supposedly XACT creates SoundEffect, 
            // but we're going to extract it instead

            reader.BaseStream.Seek(playRegionOffset, SeekOrigin.Begin);
            byte[] audioData = reader.ReadBytes(playRegionLength);

            list.Add(new ExtractedXWBData() 
            {
                Index = i,
                Codec = codec,
                Channels = channels,
                Rate = rate,
                Alignment = alignment,
                Data = audioData
            });
        }

        Parallel.ForEach(list, (data) => 
        {
            int codec = data.Codec;
            int channels = data.Channels;
            int rate = data.Rate;
            int i = data.Index;
            int alignment = data.Alignment;
            var audioData = data.Data; 

            if (codec == 0x0) // PCM
            {
                // supposedly that TowerFall only uses PCM
               
                using var tempBuffer = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(tempBuffer);
                writer.Write([(byte)'R', (byte)'I', (byte)'F', (byte)'F']);
                writer.Write(audioData.Length + 36);
                writer.Write([(byte)'W', (byte)'A', (byte)'V', (byte)'E']);
                writer.Write([(byte)'f', (byte)'m', (byte)'t', (byte)' ']);
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)channels);
                writer.Write(rate);

                int bitsPerSample = 16;
                int blockAlign = bitsPerSample / 8 * channels;
                
                writer.Write(rate * blockAlign);
                writer.Write((short)blockAlign);
                writer.Write((short)bitsPerSample);
                writer.Write([(byte)'d', (byte)'a', (byte)'t', (byte)'a']);
                writer.Write(audioData.Length);

                using var outputFs = File.Create(Path.Combine(xwbOutput, $"music_{i}.wav"));

                outputFs.Write(tempBuffer.ToArray());
                outputFs.Write(audioData);
            }
            else if (codec == 2)
            {
                // minor inconvenience
                using (var ms = new MemoryStream(audioData))
                {
                    var audioDataReader = new BinaryReader(ms);
                    audioData = ADPCMTOPCM.MSADPCM_TO_PCM(audioDataReader, (short)channels, (short)alignment);
                }

                using var tempBuffer = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(tempBuffer);
                writer.Write([(byte)'R', (byte)'I', (byte)'F', (byte)'F']);
                writer.Write(audioData.Length + 36);
                writer.Write([(byte)'W', (byte)'A', (byte)'V', (byte)'E']);
                writer.Write([(byte)'f', (byte)'m', (byte)'t', (byte)' ']);
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)channels);
                writer.Write(rate);

                writer.Write(rate * 4);
                writer.Write((short)4);
                writer.Write((short)16);
                writer.Write([(byte)'d', (byte)'a', (byte)'t', (byte)'a']);
                writer.Write(audioData.Length);

                using var outputFs = File.Create(Path.Combine(xwbOutput, $"{titles[i]}.wav"));

                outputFs.Write(tempBuffer.ToArray());
                outputFs.Write(audioData);
            }
            else 
            {
                throw new NotSupportedException($"Other codec format is not supported: {codec}");
            }
        });
    }
}

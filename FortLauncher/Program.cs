using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using FortRise;
using Mono.Cecil;
using MonoMod;
using YYProject.XXHash;

namespace FortLauncher;

internal class Program 
{
    private static readonly HashAlgorithm ChecksumHasher = XXHash64.Create();

    private static SemanticVersion Version = new SemanticVersion("5.0.0-beta.1");

    public static int Main(string[] args)
    {
        Console.WriteLine($"[FortRise] Version: {Version}");

        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string exePath = Path.GetFullPath(Path.Combine(baseDirectory, "..", "TowerFall", "TowerFall.exe"));
        string patchFile = Path.GetFullPath("TowerFall.Patch.dll");
        string mmFile = Path.GetFullPath("TowerFall.FortRise.mm.dll");


        bool shouldSkip = false;
        string mmSumStr = null;
        if (File.Exists(mmFile))
        {
            Stream mmStream = File.OpenRead(mmFile);
            var mmSum = GetChecksum(ref mmStream).ToHexadecimalString();

            // check if the sum of TowerFall.Patch.dll exists
            bool sumExists;
            if (sumExists = File.Exists(mmFile + ".sum"))
            {
                ReadOnlySpan<char> sumSum = File.ReadAllText(mmFile + ".sum").Trim();
                Console.WriteLine(mmSum.ToString() + " == " + sumSum.ToString());

                if (mmSum.SequenceEqual(sumSum))
                {
                    shouldSkip = true;
                    Console.WriteLine("[FortRise] Checksum matched, skipping patch.");
                }
            }

            mmSumStr = mmSum.ToString();
        }

        var arglist = args.ToList();
        arglist.Add("--version");
        arglist.Add(Version.ToString());

        FortRiseHandler handler = new FortRiseHandler(baseDirectory, arglist);

        if (!shouldSkip)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (FileStream fs = File.OpenRead(exePath))
                {
                    ModuleDefinition module = ModuleDefinition.ReadModule(fs);
                    if (Environment.Is64BitProcess)
                    {
                        Console.WriteLine("[FortRise] Converting 32-bit to 64-bit.");
                        // remove 32 bit flags from TowerFall
                        module.Attributes &= ~(ModuleAttributes.Required32Bit | ModuleAttributes.Preferred32Bit);
                    }
                    module.Write(stream);
                }

                if (!handler.TryPatch(stream, patchFile))
                {
                    return -1;
                }

                using (FileStream fs = File.OpenRead(patchFile))
                {
                    handler.GenerateHooks(fs, patchFile);
                }
            }
        }

        if (!shouldSkip && !string.IsNullOrEmpty(mmSumStr))
        {
            File.WriteAllText(mmFile + ".sum", mmSumStr);
        }

        handler.Run(exePath, patchFile);

        return 0;
    }

    private static ReadOnlySpan<byte> GetChecksum(ref Stream stream)
    {
        if (!stream.CanSeek)
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            stream.Dispose();
            stream = ms;
            stream.Seek(0, SeekOrigin.Begin);
        }

        long pos = stream.Position;
        stream.Seek(0, SeekOrigin.Begin);
        ReadOnlySpan<byte> hash = ChecksumHasher.ComputeHash(stream);
        stream.Seek(pos, SeekOrigin.Begin);
        return hash;
    }
}

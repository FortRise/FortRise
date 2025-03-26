using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using FortRise;
using Mono.Cecil;
using MonoMod;
using YYProject.XXHash;
using SDL3;
using FortLauncher.IO;

namespace FortLauncher;

internal class Program 
{
    private static readonly HashAlgorithm ChecksumHasher = XXHash64.Create();

    private static SemanticVersion Version = new SemanticVersion("5.0.0-beta.2");

    public static int Main(string[] args)
    {
        Console.WriteLine($"[FortRise] Version: {Version}");

        bool canOverride = File.Exists("launch_override.json");

        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string exePath;
        LaunchOverride launchOverride = default;
        if (!canOverride)
        {
            exePath = Path.GetFullPath(Path.Combine(baseDirectory, "..", "TowerFall", "TowerFall.exe"));
        }
        else 
        {
            string jsonOverride = File.ReadAllText("launch_override.json");
            launchOverride = JsonSerializer.Deserialize<LaunchOverride>(jsonOverride, LaunchOverrideContext.Default.LaunchOverride);
            if (string.IsNullOrEmpty(launchOverride.GamePath))
            {
                exePath = Path.GetFullPath(Path.Combine(baseDirectory, "..", "TowerFall", "TowerFall.exe"));
            }
            else 
            {
                exePath = launchOverride.GamePath;
            }
        }

        // Check if the game is not present
        if (!File.Exists(exePath))
        {
            SDL.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_VIDEO);
            bool overrided = false;
            unsafe 
            {
                using RawString yes = "yes";
                using RawString no = "no";

                Span<SDL.SDL_MessageBoxButtonData> buttons = [
                    new SDL.SDL_MessageBoxButtonData() 
                    {
                        flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
                        buttonID = 0,
                        text = yes
                    },
                    new SDL.SDL_MessageBoxButtonData() 
                    {
                        flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT,
                        buttonID = 1,
                        text = no
                    }
                ];

                var data = new SDL.SDL_MessageBoxData();
                using RawString title = "Manual Path Override Required!";
                using RawString message = """
                TowerFall path cannot be found with this relative path: '../TowerFall/TowerFall.exe', 
                Will need a manual path override, would you like to select a proper TowerFall executable?
                """;

                data.title = title;
                data.message = message;
                fixed (SDL.SDL_MessageBoxButtonData* ptr = buttons)
                {
                    data.numbuttons = 2;
                    data.buttons = ptr;
                    if (SDL.SDL_ShowMessageBox(ref data, out int id)) 
                    {
                        if (id == 0)
                        {
                            FileDialog.OpenFile(null, new Property() 
                            {
                                Title = "Select a TowerFall executable",
                                Filter = new DialogFilter("TowerFall executable", "exe")
                            });

                            while (FileDialog.IsOpened)
                            {
                                SDL.SDL_PumpEvents();
                            }
                        }
                    }
                }
            }

            string path = FileDialog.ResultPath;

            // we might need a good way to filter out the files, but hardcoding should do for now
            if (!string.IsNullOrEmpty(path) && path.EndsWith("TowerFall.exe"))
            {
                exePath = path;
                overrided = true;
            }
            else 
            {
                SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", "File is not 'TowerFall.exe'", IntPtr.Zero);
            }

            SDL.SDL_Quit();

            if (!overrided)
            {
                SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", "TowerFall not found!", IntPtr.Zero);
                return -1;
            }

            launchOverride.GamePath = exePath;
            string json = JsonSerializer.Serialize(launchOverride, LaunchOverrideContext.Default.LaunchOverride);
            File.WriteAllText("launch_override.json", json);
        }

        return Launch(args, baseDirectory, exePath);
    }

    private static int Launch(string[] args, string baseDirectory, string exePath)
    {
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

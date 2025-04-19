using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using FortRise;
using Mono.Cecil;
using YYProject.XXHash;
using SDL3;
using FortLauncher.IO;
using System.Collections.Generic;

namespace FortLauncher;

internal class Program 
{
    private static readonly HashAlgorithm ChecksumHasher = XXHash64.Create();
    private static readonly SemanticVersion Version = new SemanticVersion("5.0.0-beta.5");

    public static int Main(string[] args)
    {
        Environment.SetEnvironmentVariable("MONOMOD_DISABLE_TRACE_LOG", "1");
        Console.WriteLine($"[FortRise] Version: {Version}");

        bool canOverride = File.Exists("launch_override.json");

        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string? exePath;
        LaunchOverride launchOverride = default;
        if (!canOverride)
        {
            exePath = LocateExecutable(baseDirectory);
        }
        else 
        {
            string jsonOverride = File.ReadAllText("launch_override.json");
            launchOverride = JsonSerializer.Deserialize(jsonOverride, LaunchOverrideContext.Default.LaunchOverride);
            if (string.IsNullOrEmpty(launchOverride.GamePath))
            {
                exePath = LocateExecutable(baseDirectory);
            }
            else 
            {
                exePath = launchOverride.GamePath;
            }
        }

        // Check if the game is not present
        if (exePath is null || !File.Exists(exePath))
        {
            exePath = null;
            SDL.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_VIDEO);
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

            string? path = FileDialog.ResultPath;

            // we might need a good way to filter out the files, but hardcoding should do for now
            if (!string.IsNullOrEmpty(path) && path.EndsWith("TowerFall.exe"))
            {
                exePath = path;
            }
            else 
            {
                SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", "File is not 'TowerFall.exe'", IntPtr.Zero);
            }

            SDL.SDL_Quit();

            if (exePath is null)
            {
                SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", "TowerFall not found!", IntPtr.Zero);
                return -1;
            }

            launchOverride.GamePath = exePath;
            string json = JsonSerializer.Serialize(launchOverride, LaunchOverrideContext.Default.LaunchOverride);
            File.WriteAllText("launch_override.json", json);
        }

        Console.WriteLine($"[FortRise] Game Path: {exePath}");

        if (CheckLegacyFortRiseInstalled(exePath))
        {
            SDL.SDL_ShowSimpleMessageBox(
                SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", 
                "TowerFall is still patched with legacy FortRise version, unpatch it first.",
                IntPtr.Zero
            );
            return -1;
        }

        return Launch(args, baseDirectory, exePath);
    }

    private static bool CheckLegacyFortRiseInstalled(string exePath)
    {
        var txt = Path.Combine(Path.GetDirectoryName(exePath)!, "PatchVersion.txt");
        return File.Exists(txt);
    }

    private static int Launch(string[] args, string baseDirectory, string exePath)
    {
        string patchFile = Path.GetFullPath("TowerFall.Patch.dll");
        string mmFile = Path.GetFullPath("TowerFall.FortRise.mm.dll");
		string steamworksPath = Path.Combine(Path.GetDirectoryName(exePath)!, "Steamworks.NET.dll");

		bool isSteam = File.Exists(steamworksPath);

        bool shouldSkip = false;
        string? mmSumStr = null;
        if (File.Exists(mmFile))
        {
            Stream mmStream = File.OpenRead(mmFile);
            var mmSum = GetChecksum(ref mmStream).ToHexadecimalString();

            // check if the sum of TowerFall.Patch.dll exists
            if (File.Exists(mmFile + ".sum"))
            {
                ReadOnlySpan<char> sumSum = File.ReadAllText(mmFile + ".sum").Trim();

                if (mmSum.SequenceEqual(sumSum))
                {
                    shouldSkip = true;
                    Console.WriteLine("[FortRise] Checksum matched, skipping patch.");
                }
            }

            mmSumStr = mmSum.ToString();
        }

        var argList = new List<string>();

        foreach (var arg in args)
        {
            if (arg == "--enable-trace")
            {
                Environment.SetEnvironmentVariable("MONOMOD_DISABLE_TRACE_LOG", "0");
            }
            argList.Add(arg);
        }
        argList.Add("--version");
        argList.Add(Version.ToString());

        FortRiseHandler handler = new FortRiseHandler(baseDirectory, argList);

        if (!shouldSkip)
        {
			if (isSteam)
			{
				using MemoryStream steamworksStream = Remove32BitFlags(steamworksPath);
				File.WriteAllBytes(Path.Combine(baseDirectory, "Steamworks.NET.dll"), steamworksStream.ToArray());
			}
			using Stream tfStream = Remove32BitFlags(exePath);

			if (!handler.TryPatch(tfStream, patchFile))
			{
				return -1;
			}

			using (FileStream fs = File.OpenRead(patchFile))
			{
				handler.GenerateHooks(fs, patchFile);
			}
        }

        if (!shouldSkip && !string.IsNullOrEmpty(mmSumStr))
        {
            File.WriteAllText(mmFile + ".sum", mmSumStr);
        }

        handler.Run(exePath, patchFile);
        Environment.SetEnvironmentVariable("MONOMOD_DISABLE_TRACE_LOG", "");
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

	private static MemoryStream Remove32BitFlags(string modulePath)
	{
		MemoryStream memoryStream = new MemoryStream();
		using (FileStream fs = File.OpenRead(modulePath))
		{
			ModuleDefinition module = ModuleDefinition.ReadModule(fs);
			if (Environment.Is64BitProcess)
			{
				Console.WriteLine($"[FortRise] Removing 32 bit flags from '{Path.GetFileName(modulePath)}'");
				// remove 32 bit flags from TowerFall
				module.Attributes &= ~(ModuleAttributes.Required32Bit | ModuleAttributes.Preferred32Bit);
			}
			module.Write(memoryStream);
		}

		return memoryStream;
	}

    private static string? LocateExecutable(string baseDirectory)
    {
        string? path;
        path = Path.GetFullPath(Path.Combine(baseDirectory, "..", "TowerFall", "TowerFall.exe"));
        if (File.Exists(path))
        {
            return path;
        }

        // second attempt
        path = Path.GetFullPath(Path.Combine(baseDirectory, "..", "TowerFall.exe"));
        if (File.Exists(path))
        {
            return path;
        }

        // nope
        return null;
    }
}

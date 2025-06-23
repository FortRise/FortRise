using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;

namespace FortLauncher;

public class FortRiseHandler(string fortriseCWD, List<string> args, ILogger logger, ILoggerFactory factory)
{
    public string[] Args = args.ToArray();
    private string cwd = fortriseCWD;
    private ILogger logger = logger;
    private ILoggerFactory factory = factory;

    public void Run(string exePath, string patchFile)
    {
        var asm = LoadAssembly(patchFile);
        Directory.SetCurrentDirectory(Path.GetFullPath(Path.GetDirectoryName(exePath)!));

        // run the game
        if (asm.EntryPoint is not null)
        {
            var riseCore = asm.GetType("FortRise.RiseCore")?.GetMethod("LauncherPipe", BindingFlags.NonPublic | BindingFlags.Static);

            if (riseCore != null)
            {
                riseCore.Invoke(null, [logger, factory]);
                logger.LogInformation("Running the game!");
                asm.EntryPoint.Invoke(null, BindingFlags.DoNotWrapExceptions, null, [Args], null);
                return;
            }
        }

        logger.LogCritical("Failed to run the game.");
    }

    public void GenerateHooks(Stream stream, string patchFile)
    {
        Environment.SetEnvironmentVariable("MONOMOD_HOOKGEN_PRIVATE", "1");
        string mmhookPath = Path.Combine(Path.GetDirectoryName(patchFile)!, "MMHOOK_TowerFall.dll");
        using (var modder = new MonoModder()
        {
            Input = stream,
            OutputPath = mmhookPath,
            ReadingMode = ReadingMode.Deferred,
            LogVerboseEnabled = false
        })
        {
            modder.Read();

            modder.MapDependencies();

            if (File.Exists(mmhookPath))
            {
                modder.Log($"[HookGen] Clearing {mmhookPath}");
                File.Delete(mmhookPath);
            }

            modder.Log("[HookGen] Starting HookGenerator for compatibility reason");
            var gen = new HookGenerator(modder, Path.GetFileName(mmhookPath));
            using (var mOut = gen.OutputModule)
            {
                gen.Generate();
                mOut.Write(mmhookPath);
            }

            modder.Log("[HookGen] Done.");
        }
        Environment.SetEnvironmentVariable("MONOMOD_HOOKGEN_PRIVATE", null);
    }

    public bool TryPatch(Stream stream, string patchFile)
    {
        Environment.SetEnvironmentVariable("MONOMOD_DEPENDENCY_MISSING_THROW", "0");

        try 
        {
            using (var modder = new FortRiseMonoModder()
            {
                Input = stream,
                OutputPath = patchFile,
                LogVerboseEnabled = false
            })
            {
                modder.Read();

                modder.Log("[Main] Scanning for TowerFall.FortRise.mm.dll.");
                modder.ReadMod(Path.GetFullPath("TowerFall.FortRise.mm.dll"));

                modder.MapDependencies();

                modder.Log("[Main] modder.AutoPatch()");
                modder.AutoPatch();

                modder.Write();

                modder.Log("[Main] Done.");
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical("Patch failed, exception: {ex}", ex);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.ReadKey();
            }

            return false;
        }

        return true;
    }

    private Assembly LoadAssembly(string assembly)
    {
        var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(assembly);

        AssemblyLoadContext.Default.Resolving += (context, assemblyName) => 
        {
            string path = Path.Combine(cwd, assemblyName.Name + ".dll");

            if (!File.Exists(path))
            {
                return null;
            }
            return context.LoadFromAssemblyPath(path);
        };

        AssemblyLoadContext.Default.ResolvingUnmanagedDll += (asm, name) => 
        {
            string unmanagedFolders = string.Empty;
            string dllFormat = ".dll";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                unmanagedFolders = "lib64";
                dllFormat = ".so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                unmanagedFolders = "osx";
                dllFormat = ".dylib";
            }

            if (unmanagedFolders == string.Empty)
            {
                if (NativeLibrary.TryLoad(name + ".dll", out nint handle))
                {
                    return handle;
                }
            }
            else 
            {
                var file = Path.GetFullPath(Path.Combine(unmanagedFolders, "lib" + name + dllFormat));
                if (NativeLibrary.TryLoad(file, out nint handle))
                {
                    return handle;
                }
            }

            return IntPtr.Zero;
        };

        return asm;
    }
}
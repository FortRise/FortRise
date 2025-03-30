using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;

namespace FortLauncher;

public class FortRiseHandler(string fortriseCWD, List<string> args)
{
    public string[] Args = args.ToArray();
    private string cwd = fortriseCWD;

    public void Run(string exePath, string patchFile) 
    {
        var asm = LoadAssembly(patchFile);
        Directory.SetCurrentDirectory(Path.GetFullPath(Path.GetDirectoryName(exePath)));

        // run the game
        asm.EntryPoint.Invoke(null, BindingFlags.DoNotWrapExceptions, null, [Args], null);
    }

    public void GenerateHooks(Stream stream, string patchFile)
    {
        Environment.SetEnvironmentVariable("MONOMOD_HOOKGEN_PRIVATE", "1");
        string mmhookPath = Path.Combine(Path.GetDirectoryName(patchFile), "MMHOOK_TowerFall.dll");
        using (var modder = new MonoModder()
        {
            Input = stream,
            OutputPath = mmhookPath,
            ReadingMode = ReadingMode.Deferred
        })
        {
            modder.Read();

            modder.MapDependencies();

            if (File.Exists(mmhookPath))
            {
                modder.Log($"[HookGen] Clearing {mmhookPath}");
                File.Delete(mmhookPath);
            }

            modder.Log("[HookGen] Starting HookGenerator");
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
            using (MonoModder modder = new MonoModder()
            {
                Input = stream,
                OutputPath = patchFile,
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
            Console.WriteLine(ex);
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

        return asm;
    }
}
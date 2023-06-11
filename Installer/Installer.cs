using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Reflection;

// #if PLATFORM_OSX
// using MonoMod;
// using MonoMod.RuntimeDetour.HookGen;
// using Mono.Cecil;
// #endif

#if ANSI
using Spectre.Console;
#endif

namespace FortRise.Installer;

public static class Installer
{
#if PLATFORM_OSX
    public static Assembly? AsmMonoMod;
    public static Assembly? AsmHookGen;
#endif
    public const string TowerFallVersion = "1.3.3.3";

// For MacOS and Linux, you should use the Netstandard2.0 version of MonoMod which supports both Net framework and Net Core
    private static readonly string[] fileDependencies = {
        "FNA.dll", "FNA.dll.config",
#if !PLATFORM_OSX
        "MonoMod.RuntimeDetour.HookGen.exe",
#endif
        "MonoMod.exe",
        "MonoMod.xml", "0Harmony.dll",
        "MonoMod.Utils.dll", "MonoMod.Utils.xml", 
        "MonoMod.RuntimeDetour.HookGen.xml",
        "TowerFall.FortRise.mm.pdb",
        "MonoMod.RuntimeDetour.dll", "MonoMod.RuntimeDetour.xml",
        "Mono.Cecil.dll", "Mono.Cecil.Mdb.dll", "Mono.Cecil.Pdb.dll",
        "TeuJson.dll", "MonoMod.ILHelpers.dll", "MonoMod.Backports.dll"
    };

    private static readonly string[] fnaLibs = {
        "FAudio.dll", "FNA3D.dll", "libtheorafile.dll", "MojoShader.dll",
        "SDL2_image.dll", "SDL2.dll"
    };

    private static readonly string modFile = "TowerFall.FortRise.mm.dll";

    public static async Task Install(string path) 
    {
        var patchVersion = Path.Combine(path, "PatchVersion.txt");
        bool shouldProceed = true;
        if (File.Exists(patchVersion)) 
        {
            using var fs = File.OpenRead(patchVersion);
            using var sr = new StreamReader(fs);
            var tfVersion = sr.ReadLine()?.Split(':')[1].Trim();
            var pVersion = sr.ReadLine()?.Split(':')[1].Trim();
            if (pVersion == Program.Version) 
            {
                shouldProceed = false;
            }
        }
        if (!shouldProceed) 
        {
#if ANSI
            if (!AnsiConsole.Confirm("[green]TowerFall has already been patched[/], [underline]do you want to patch again?[/]", false))
                return;
#else
            Console.WriteLine("TowerFall has already been patched, but you decided to patch again.");
#endif
        }

        var fortOrigPath = Path.Combine(path, "fortOrig");

        if (File.Exists(Path.Combine(fortOrigPath, "TowerFall.exe"))) 
        {
            File.Copy(Path.Combine(fortOrigPath, "TowerFall.exe"), Path.Combine(path, "TowerFall.exe"), true);
        }

        Underline("Moving original TowerFall into fortOrig folder");
        if (!Directory.Exists(fortOrigPath))
            Directory.CreateDirectory(fortOrigPath);
        
        if (!File.Exists(Path.Combine(fortOrigPath, "TowerFall.exe")))
            File.Copy(Path.Combine(path, "TowerFall.exe"), Path.Combine(fortOrigPath, "TowerFall.exe"));

        if (!File.Exists(Path.Combine(fortOrigPath, "TowerFall.exe")))
        {
            ThrowError("[underline][red]Copying failed[/][/]");           
            return;
        }
        var libPath = "";
        Underline("Moving the mod into TowerFall directory");

        var fortRiseDll = Path.Combine(libPath, modFile);
        if (!File.Exists(fortRiseDll)) 
        {
            ThrowError("[underline][red]TowerFall.FortRise.mm.dll mod file not found![/][/]");
            return;
        }
        File.Copy(fortRiseDll, Path.Combine(path, "TowerFall.FortRise.mm.dll"), true);

        Underline("Moving all of the lib files");
        foreach (var file in fileDependencies) 
        {
            var lib = Path.Combine(libPath, file);
            if (!File.Exists(lib)) 
            {
                ThrowErrorContinous($"[underline][red]{lib} file not found![/][/]");
                continue;
            }

            File.Copy(lib, Path.Combine(path, Path.GetFileName(lib)), true);
        }

        Underline("Copying needed FNA libs");
        if (!Directory.Exists(Path.Combine(libPath, "x86")))
            Directory.CreateDirectory(Path.Combine(libPath, "x86"));
        foreach (var file in fnaLibs) 
        {
            var lib = Path.Combine(libPath, "x86", file);
            if (!File.Exists(lib)) 
            {
                ThrowErrorContinous($"[underline][red]{lib} file not found![/][/]");
                continue;
            }

            File.Copy(lib, Path.Combine(path, "x86", Path.GetFileName(lib)), true);
        }

        Underline("Patching TowerFall");

#if !PLATFORM_OSX
        var monoModPath = Path.Combine(libPath, "MonoMod.exe");
        if (!File.Exists(monoModPath))
        {
            ThrowError("No MonoMod executable found in the lib path");
            return;
        }
        var process = Process.Start(monoModPath, $"\"{Path.Combine(path, "TowerFall.exe")}\"");
        await process.WaitForExitAsync();
        if (process.ExitCode != 0) 
        {
            ThrowError("[underline][red]MonoMod failed to patch the assembly[/][/]");
            UnderlineInfo("Note that the TowerFall might be patched from other modloader");
            await Task.Delay(1000);
            return;
        }
#else
        LoadAssembly(Path.Combine(path, "Mono.Cecil.dll"));
        LoadAssembly(Path.Combine(path, "Mono.Cecil.Pdb.dll"));
        LoadAssembly(Path.Combine(path, "Mono.Cecil.Mdb.dll"));
        LoadAssembly(Path.Combine(path, "MonoMod.Utils.dll"));
        LoadAssembly(Path.Combine(path, "MonoMod.RuntimeDetour.dll"));
        AsmMonoMod = LoadAssembly(Path.Combine(path, "MonoMod.exe"));
        AsmHookGen = LoadAssembly(Path.Combine(path, "MonoMod.RuntimeDetour.HookGen.exe"));

        Environment.SetEnvironmentVariable("MONOMOD_DEPENDENCY_MISSING_THROW", "0");
        int returnCode = (int) AsmMonoMod.EntryPoint.Invoke(null, new object[] { 
            new string[] { Path.Combine(path, "TowerFall.exe"), Path.Combine(path, "MONOMODDED_TowerFall.exe") } });
        if (returnCode != 0) 
        {
            ThrowError("[underline][red]MonoMod failed to patch the assembly[/][/]");
            UnderlineInfo("Note that the TowerFall might be patched from other modloader");
            await Task.Delay(1000);   
            return;
        }


        // Run MonoMod
        // try 
        // {
        //     using var modder = new MonoModder() 
        //     {
        //         InputPath = Path.Combine(path, "TowerFall.exe"),
        //         OutputPath = Path.Combine(path, "MONOMODDED_TowerFall.exe"),
        //         MissingDependencyThrow = false
        //     };
        //     modder.Read();
        //     modder.Log("[MMMain] Scanning for mods in directory.");
        //     modder.ReadMod(path);

        //     modder.MapDependencies();

        //     modder.Log("[MMMain] modder.AutoPatch();");
        //     modder.AutoPatch();

        //     modder.Write();
        //     modder.Log("[MMMain] Done.");
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e);
        //     ThrowError("[underline][red]MonoMod failed to patch the assembly[/][/]");
        //     UnderlineInfo("Note that the TowerFall might be patched from other modloader");
        //     await Task.Delay(1000);
        //     return;
        // }
#endif

        Underline("Renaming the output");

        File.Copy(Path.Combine(path, "MONOMODDED_TowerFall.exe"), Path.Combine(path, "TowerFall.exe"), true);
        File.Copy(Path.Combine(path, "MONOMODDED_TowerFall.pdb"), Path.Combine(path, "TowerFall.pdb"), true);
        File.Delete(Path.Combine(path, "MONOMODDED_TowerFall.exe"));
        File.Delete(Path.Combine(path, "MONOMODDED_TowerFall.pdb"));

#if !PLATFORM_OSX
        Underline("Running HookGen");
        var hookGenPath = Path.Combine(libPath, "MonoMod.RuntimeDetour.HookGen.exe");
        if (!File.Exists(hookGenPath))
        {
            ThrowError("No MonoMod.RuntimeDetour.HookGen executable found in the lib path");
            return;
        }
        process = Process.Start(hookGenPath, $"--private \"{Path.Combine(path, "TowerFall.exe")}\"");
        await process.WaitForExitAsync();
        if (process.ExitCode != 0) 
        {
            ThrowError("[underline][red]HookGen failed to generate hooks[/][/]");
        }
#else
        Environment.SetEnvironmentVariable("MONOMOD_DEPENDENCY_MISSING_THROW", "0");
        AsmHookGen.EntryPoint.Invoke(null, new object[] { new string[] { "--private", Path.Combine(path, "TowerFall.exe"), Path.Combine(path, "MMHOOK_TowerFall.dll") } });

        // try 
        // {
        //     var output = Path.Combine(path, "MMHOOK_TowerFall.dll");
        //     using var modder = new MonoModder() 
        //     {
        //         InputPath = Path.Combine(path, "TowerFall.exe"),
        //         OutputPath = output
        //     };
        //     modder.Read();
        //     modder.MapDependencies();

        //     if (File.Exists(output)) 
        //     {
        //         modder.Log($"[HookGen] Clearing {output}");
        //         File.Delete(output);
        //     }

        //     modder.Log("[HookGen] Starting HookGenerator");
        //     HookGenerator gen = new HookGenerator(modder, Path.GetFileName(output));
        //     ModuleDefinition mOut = gen.OutputModule;
        //     {
        //         gen.Generate();
        //         mOut.Write(output);
        //     }
        //     modder.Log("[HookGen] Done.");
        // }
        // catch (Exception e) 
        // {
        //     Console.WriteLine(e);
        //     ThrowError("[underline][red]HookGen failed to generate hooks[/][/]");
        // }
#endif
        Yellow("Finalizing");

        Underline("Writing the version file");

        bool debugMode = Program.DebugMode;
#if ANSI
        debugMode = AnsiConsole.Confirm("Do you want to run in debug mode?", false);
#else
        Console.WriteLine("Run it in Debug Mode by modifying the PatchVersion.txt file");
#endif

        var sb = new StringBuilder();
        sb.AppendLine("TF Version: " + TowerFallVersion);
        sb.AppendLine("Installer Version: " + Program.Version);
        sb.AppendLine("Debug Mode: " + debugMode);
        var text = sb.ToString();
#if PLATFORM_OSX
        File.WriteAllText(Path.Combine(path, "PatchVersion.txt"), sb.ToString());
#else
        await File.WriteAllTextAsync(Path.Combine(path, "PatchVersion.txt"), sb.ToString());
#endif

        Succeed("Installed");
    }

    public static async Task Uninstall(string path) 
    {
        var patchVersion = Path.Combine(path, "PatchVersion.txt");
        bool shouldProceed = false;
        if (File.Exists(patchVersion)) 
        {
            shouldProceed = true;
        }
        if (!shouldProceed) 
        {
            ThrowError("This TowerFall has not been patched yet.");
            return;
        }
        var fortOrigPath = Path.Combine(path, "fortOrig", "TowerFall.exe");
        Underline("Copying original TowerFall into TowerFall root folder");
        File.Copy(fortOrigPath, Path.Combine(path, "TowerFall.exe"), true);

        Underline("Deleting the libraries from the TowerFall root folder");

        foreach (var file in fileDependencies) 
        {
            var lib = Path.Combine(path, file);
            if (!File.Exists(lib)) 
            {
                continue;
            }

            File.Delete(lib);
        }

        Underline("Deleting the fna libraries from the TowerFall root folder");

        foreach (var file in fnaLibs) 
        {
            var lib = Path.Combine(path, file);
            if (!File.Exists(lib)) 
            {
                continue;
            }

            File.Delete(lib);
        }

        Underline("Deleting the mod");

        var fortRiseDll = Path.Combine(path, modFile);
        if (File.Exists(fortRiseDll)) 
        {
            File.Delete(fortRiseDll);
        }

        Underline("Deleting the hooks");

        var hookDll = Path.Combine(path, "MMHOOK_TowerFall.dll");
        if (File.Exists(hookDll)) 
        {
            File.Delete(hookDll);
        }

        Underline("Deleting the PatchVersion text file");
        
        File.Delete(Path.Combine(path, "PatchVersion.txt"));
        await Task.Delay(1000);

        Succeed("Unpatched");
    }
    private static void Yellow(string text) 
    {
#if ANSI
        AnsiConsole.MarkupLine("[yellow]Finalizing[/]");
#else
        Console.WriteLine("Finalizing");
#endif
    }

    private static void UnderlineInfo(string text) 
    {
#if ANSI
        AnsiConsole.MarkupLine($"[underline][gray]{text}[/][/]");
#else
        Console.WriteLine(text);
#endif
    }

    private static void Underline(string text) 
    {
#if ANSI
        AnsiConsole.MarkupLine($"[underline]{text}[/]");
#else
        Console.WriteLine(text);
#endif
    }

    private static void Succeed(string text) 
    {
#if ANSI
        AnsiConsole.MarkupLine($"[underline][green]{text}[/][/]");
#else
        Console.WriteLine(text);
#endif
    }


    private static void ThrowErrorContinous(string error) 
    {
#if ANSI
        AnsiConsole.MarkupLine(error);
#else
        Console.WriteLine(error);
#endif
    }

    private static void ThrowError(string error) 
    {
#if ANSI
        AnsiConsole.Prompt(new TextPrompt<string>(error).AllowEmpty());
#else
        Console.WriteLine(error);
#endif
    }

#if PLATFORM_OSX
    private static Assembly LoadAssembly(string path) 
    {
        ResolveEventHandler tmpResolver = (s, e) => {
            string asmPath = Path.Combine(Path.GetDirectoryName(path), new AssemblyName(e.Name).Name + ".dll");
            if (!File.Exists(asmPath))
                return null;
            return Assembly.LoadFrom(asmPath);
        };
        AppDomain.CurrentDomain.AssemblyResolve += tmpResolver;
        // Assembly asm = Assembly.Load(Path.GetFileNameWithoutExtension(path));
        Assembly asm = Assembly.LoadFrom(path);
        AppDomain.CurrentDomain.AssemblyResolve -= tmpResolver;
        AppDomain.CurrentDomain.TypeResolve += (s, e) => {
            return asm.GetType(e.Name) != null ? asm : null;
        };
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) => {
            return e.Name == asm.FullName || e.Name == asm.GetName().Name ? asm : null;
        };
        return asm;
    }
#endif
}
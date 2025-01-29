using System.Text;
using System.IO;
using System;
using System.Reflection;
using System.Xml;

namespace FortRise.Installer;

public class Installer : MarshalByRefObject
{
    public static Assembly AsmMonoMod;
    public static Assembly AsmHookGen;


    private static readonly string[] fileDependencies = {
        "FNA.dll", "FNA.dll.config", "FNA.pdb",
        "FNA.xml", "MonoMod.RuntimeDetour.HookGen.exe",
        "MonoMod.exe", 
        "MonoMod.xml", "0Harmony.dll",
        "MonoMod.Utils.dll", "MonoMod.Utils.xml", 
        "MonoMod.RuntimeDetour.HookGen.xml",
        "TowerFall.FortRise.mm.xml",
        "TowerFall.FortRise.mm.pdb",
        "MonoMod.RuntimeDetour.dll", "MonoMod.RuntimeDetour.xml",
        "Mono.Cecil.dll", "Mono.Cecil.Mdb.dll", "Mono.Cecil.Pdb.dll",
        "MonoMod.ILHelpers.dll", "Mono.Cecil.Rocks.dll",
        "DiscordGameSdk.dll", "DiscordGameSdk.pdb", "Fortrise.targets"
    };
    
    private static readonly string[] fileDeprecated = {
        "DotNetZip.dll", "TeuJson.dll", "MonoMod.Backports.dll", "KeraLua.dll", "NLua.dll, I18N.dll, I18N.West.dll"
    };

    private static string[] nativeLibs; 
    private static readonly string modFile = "TowerFall.FortRise.mm.dll";

    public void Install(string path) 
    {
        // Let's try to not do it at compile-time.. It's really hard to maintain that way
        string NativePath;
        Action<string> NativeCopy;
        switch (Environment.OSVersion.Platform) 
        {
        case PlatformID.MacOSX:
            NativePath = "MacOS/osx";
            NativeCopy = CopyFNAFiles_MacOS;
            nativeLibs = new string[] {
                "libFAudio.0.dylib", "libFNA3D.0.dylib",
                "libMoltenVK.dylib", "libSDL2-2.0.0.dylib", "libtheorafile.dylib",
                "libvulkan.1.dylib", "libdiscord_game_sdk.dylib", "libmono-btls-shared.dylib", "libMonoPosixHelper.dylib"
            };
            break;
        case PlatformID.Unix:
            NativePath = "lib64";
            NativeCopy = CopyFNAFiles_Linux;
            nativeLibs = new string[] {
                "libFAudio.so.0", "libFNA3D.so.0",
                "libSDL2-2.0.so.0", "libtheorafile.so", "libdiscord_game_sdk.so", "libmono-btls-shared.so", "libMonoPosixHelper.so"
            };
            break;
        default:
            NativePath = "x86";
            NativeCopy = CopyFNAFiles_Windows;
            nativeLibs = new string[] {
                "FAudio.dll", "FNA3D.dll",
                "SDL2.dll", "libtheorafile.dll", "discord_game_sdk.dll"
            };
            break;
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
            ThrowError("Copying failed");           
            return;
        }

        Underline("Moving original FNA.dll and FNA.dll.config into fortOrig folder if have one");
        if (File.Exists(Path.Combine(path, "FNA.dll")) && !File.Exists(Path.Combine(fortOrigPath, "FNA.dll")))
        {
            File.Copy(Path.Combine(path, "FNA.dll"), Path.Combine(fortOrigPath, "FNA.dll"));
        }
        if (File.Exists(Path.Combine(path, "FNA.dll.config")) && !File.Exists(Path.Combine(fortOrigPath, "FNA.dll.config")))
        {
            File.Copy(Path.Combine(path, "FNA.dll.config"), Path.Combine(fortOrigPath, "FNA.dll.config"));
        }

        var libPath = "";

        if (Environment.OSVersion.Platform == PlatformID.Win32NT) 
        {
            Underline("Supporting DInput and other SDL controllers");
            if (!File.Exists(Path.Combine(path, "gamecontrollerdb.txt")))
                File.Copy(Path.Combine(libPath, "gamecontrollerdb.txt"), Path.Combine(path, "gamecontrollerdb.txt"), true);
        }

        Underline("Moving the mod into TowerFall directory");

        var fortRiseDll = Path.Combine(libPath, modFile);
        if (!File.Exists(fortRiseDll)) 
        {
            ThrowError("TowerFall.FortRise.mm.dll mod file not found!");
            return;
        }
        File.Copy(fortRiseDll, Path.Combine(path, "TowerFall.FortRise.mm.dll"), true);

        Underline("Moving all of the lib files");
        foreach (var file in fileDependencies) 
        {
            var lib = Path.Combine(libPath, file);
            if (!File.Exists(lib)) 
            {
                ThrowErrorContinous($"{lib} file not found!");
                continue;
            }

            File.Copy(lib, Path.Combine(path, Path.GetFileName(lib)), true);
        }

        foreach (var file in fileDeprecated) 
        {
            var lib = Path.Combine(path, file);
            if (!File.Exists(lib)) 
            {
                continue;
            }

            File.Delete(lib);
        }

        Underline($"Moving all of the Native files on {NativePath}");
        NativeCopy(path);


        Underline("Generating XML Document");
        GenerateDOC(Path.Combine(libPath, "TowerFall.FortRise.mm.xml"), Path.Combine(path, "TowerFall.xml"));


        Underline("Patching TowerFall");
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
            ThrowError("MonoMod failed to patch the assembly");
            UnderlineInfo("Note that the TowerFall might be patched from other modloader");
            return;
        }

        Underline("Renaming the output");
        var towerFallExe = Path.Combine(path, "TowerFall.exe");
        var towerFallPdb = Path.Combine(path, "TowerFall.pdb");
        if (File.Exists(towerFallExe)) 
        {
            File.Delete(towerFallExe);
        }
        if (File.Exists(towerFallPdb)) 
        {
            File.Delete(towerFallPdb);
        }

        var moddedOutputExe = Path.Combine(path, "MONOMODDED_TowerFall.exe");
        var moddedOutputPdb = Path.Combine(path, "MONOMODDED_TowerFall.pdb");
        File.Move(moddedOutputExe, towerFallExe);
        File.Move(moddedOutputPdb, towerFallPdb);

        Yellow("Generating HookGen");

        Environment.SetEnvironmentVariable("MONOMOD_DEPENDENCY_MISSING_THROW", "0");
        AsmHookGen.EntryPoint.Invoke(null, new object[] { new string[] { 
                "--private", 
                Path.Combine(path, "TowerFall.exe"), 
                Path.Combine(path, "MMHOOK_TowerFall.dll") 
            } 
        });

        var patchVersion = Path.Combine(path, "PatchVersion.txt");

        Underline("Writing the version file");

        var sb = new StringBuilder();
        sb.AppendLine("Installer Version: " + "5.0.0");

        var text = sb.ToString();

        File.WriteAllText(Path.Combine(path, "PatchVersion.txt"), sb.ToString());

        Yellow("Cleaning up");
        Environment.SetEnvironmentVariable("MONOMOD_DEPENDENCY_MISSING_THROW", "");

        Succeed("Installed");
    }

    public void Uninstall(string path) 
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

        Succeed("Unpatched");
    }

    private static void GenerateDOC(string docXML, string toPath) 
    {
        var xmlDocument = new XmlDocument();
        try 
        {
            xmlDocument.Load(docXML);
        }
        catch 
        {
            ThrowErrorContinous("Failed to generate doc xml");
            return;
        }
        var xmlName = xmlDocument["doc"]?["assembly"]?["name"];
        if (xmlName == null) 
        {
            ThrowErrorContinous("Failed to generate doc xml");
            return;
        }
        xmlName.InnerText = "TowerFall";
        xmlDocument.Save(toPath);
    }

    private static void CopyNativeFiles_Windows(string path) 
    {
        Console.WriteLine("CopyNativeFiles_Windows is called");
        foreach (var fnaLib in nativeLibs) 
        {
            var lib = Path.Combine("x86", fnaLib);
            if (!File.Exists(lib)) 
            {
                ThrowErrorContinous($"{lib} file not found!");
                continue;
            }   
            var x86Path = Path.Combine(path, "x86");
            if (!Directory.Exists(x86Path)) 
                Directory.CreateDirectory(x86Path);
            File.Copy(lib, Path.Combine(x86Path, Path.GetFileName(lib)), true);
        }
    }

    private static void CopyNativeFiles_Linux(string path) 
    {
        Console.WriteLine("CopyNativeFiles_Linux is called");
        foreach (var fnaLib in nativeLibs) 
        {
            var origPath = Path.Combine(path, "lib64/orig");

            var lib64Path = Path.Combine(path, "lib64");
            if (!Directory.Exists(origPath)) 
                Directory.CreateDirectory(origPath);
            
            if (File.Exists(Path.Combine(lib64Path, Path.GetFileName(fnaLib))) && !File.Exists(Path.Combine(origPath, Path.GetFileName(fnaLib))))
                File.Copy(Path.Combine(lib64Path, Path.GetFileName(fnaLib)), origPath, true);
            
            var lib = Path.Combine("lib64", fnaLib);
            if (!File.Exists(lib)) 
            {
                ThrowErrorContinous($"{lib} file not found!");
                continue;
            }   
            File.Copy(lib, Path.Combine(lib64Path, Path.GetFileName(lib)), true);
        }
    }

    private static void CopyNativeFiles_MacOS(string path) 
    {
        Console.WriteLine("CopyNativeFiles_MACOS is called");
        var macOSPath = new DirectoryInfo(path).Parent.FullName;
        foreach (var fnaLib in nativeLibs) 
        {
            var osxPath = Path.Combine(macOSPath, "MacOS/Resources/osx");
            var origPath = Path.Combine(osxPath, "orig");
            if (!Directory.Exists(origPath)) 
                Directory.CreateDirectory(origPath);

            if (File.Exists(Path.Combine(osxPath, Path.GetFileName(fnaLib))) && !File.Exists(Path.Combine(origPath, Path.GetFileName(fnaLib))))
                File.Copy(Path.Combine(osxPath, Path.GetFileName(fnaLib)), origPath, true);
            
            var lib = Path.Combine("osx", fnaLib);
            if (!File.Exists(lib)) 
            {
                ThrowErrorContinous($"{lib} file not found!");
                continue;
            }   
            File.Copy(lib, Path.Combine(osxPath, Path.GetFileName(lib)), true);
        }
    }


    private static void Yellow(string text) 
    {
        Console.WriteLine(text);
    }

    private static void UnderlineInfo(string text) 
    {
        Console.WriteLine(text);
    }

    private static void Underline(string text) 
    {
        Console.WriteLine(text);
    }

    private static void Succeed(string text) 
    {
        Console.WriteLine(text);
    }


    private static void ThrowErrorContinous(string error) 
    {
        Console.WriteLine(error);
    }

    private static void ThrowError(string error) 
    {
        Console.WriteLine(error);
    }

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
        Assembly asm = Assembly.Load(Path.GetFileNameWithoutExtension(path));
        AppDomain.CurrentDomain.AssemblyResolve -= tmpResolver;
        AppDomain.CurrentDomain.TypeResolve += (s, e) => {
            return asm.GetType(e.Name) != null ? asm : null;
        };
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) => {
            return e.Name == asm.FullName || e.Name == asm.GetName().Name ? asm : null;
        };
        return asm;
    }
}
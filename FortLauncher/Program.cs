using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using FortRise;
using Mono.Cecil;
using MonoMod;

namespace FortLauncher;

internal class Program 
{
    private static SemanticVersion Version = new SemanticVersion("5.0.0-beta.1");

    public static int Main(string[] args)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string exePath = Path.GetFullPath(Path.Combine(baseDirectory, "..", "TowerFall", "TowerFall.exe"));
        string patchFile = Path.GetFullPath("TowerFall.Patch.dll");

        MemoryStream stream = new MemoryStream();
        using (FileStream fs = File.OpenRead(exePath))
        {
            ModuleDefinition module = ModuleDefinition.ReadModule(fs);
            if (Environment.Is64BitProcess)
            {
                // remove 32 bit flags from TowerFall
                module.Attributes &= ~(ModuleAttributes.Required32Bit | ModuleAttributes.Preferred32Bit);
            }
            module.Write(stream);
        }

        var arglist = args.ToList();
        arglist.Add("--version");
        arglist.Add(Version.ToString());

        FortRiseHandler handler = new FortRiseHandler(baseDirectory, arglist);

        if (!handler.TryPatch(stream, patchFile))
        {
            return -1;
        }

        using (FileStream fs = File.OpenRead(patchFile))
        {
            handler.GenerateHooks(fs, patchFile);
        }

        handler.Run(exePath);

        return 0;
    }
}
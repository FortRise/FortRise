using System;
using System.IO;
using Mono.Cecil;

namespace FortLauncher;

internal sealed class Remove32BitFlagsPatcher : Patcher
{
    public override PatcherScope Scope => PatcherScope.Assembly;

    public override void PatchAssembly(ModuleDefinition module)
    {
        if (Environment.Is64BitProcess)
        {
            Console.WriteLine($"[FortRise] Removing 32 bit flags from '{Path.GetFileName(module.Name)}'");
            // remove 32 bit flags from TowerFall
            module.Attributes &= ~(ModuleAttributes.Required32Bit | ModuleAttributes.Preferred32Bit);
        }
    }
}

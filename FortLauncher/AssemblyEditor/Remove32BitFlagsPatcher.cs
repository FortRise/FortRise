using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Mono.Cecil;

namespace FortLauncher;

internal sealed class Remove32BitFlagsPatcher(ILogger logger) : Patcher
{
    public override PatcherScope Scope => PatcherScope.Assembly;

    public override void PatchAssembly(ModuleDefinition module)
    {
        if (Environment.Is64BitProcess)
        {
            logger.LogInformation("Removing 32 bit flags from '{dll}'", Path.GetFileName(module.Name));
            // remove 32 bit flags from TowerFall
            module.Attributes &= ~(ModuleAttributes.Required32Bit | ModuleAttributes.Preferred32Bit);
        }
    }
}

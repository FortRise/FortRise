using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using MonoMod.Utils;

namespace FortLauncher;

internal sealed class AssemblyEditor : IDisposable
{
    private List<Patcher> patchers = [];
    private ModuleDefinition moduleDefinition;
    private Stream fs;
    private bool isPatched;

    public AssemblyEditor(string assemblyPath)
    {
        fs = File.OpenRead(assemblyPath);
        moduleDefinition = ModuleDefinition.ReadModule(fs);
    }

    public AssemblyEditor Add(Patcher patcher)
    {
        patchers.Add(patcher);
        return this;
    }

    public void Patch()
    {
        foreach (var patcher in patchers)
        {
            patcher.StartPatching(moduleDefinition);
        }

        isPatched = true;
    }

    public MemoryStream Write()
    {
        if (!isPatched)
        {
            Patch();
            isPatched = true;
        }
        MemoryStream memoryStream = new MemoryStream();
        moduleDefinition.Write(memoryStream);
        return memoryStream;
    }

    public void Dispose()
    {
        moduleDefinition.Dispose();
        fs.Dispose();
    }
}

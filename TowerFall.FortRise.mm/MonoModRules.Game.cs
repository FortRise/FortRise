using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace MonoMod;

internal static partial class MonoModRules
{

    private static void GamePatch(MonoModder modder)
    {
        var module = modder.Module;
        modder.PostProcessors += PostProcessMacros;

        TypeDefinition t_TFGame = modder.FindType("TowerFall.TFGame")?.Resolve();
        if (t_TFGame == null)
            return;
        IsTowerFall = t_TFGame.Scope == modder.Module;

        // Get the version of TowerFall

        int[] numVersions = null;
        var ctor_TFGame = t_TFGame.FindMethod(".cctor", true);
        if (ctor_TFGame != null && ctor_TFGame.HasBody)
        {
            var instrs = ctor_TFGame.Body.Instructions;
            for (int i = 0; i < instrs.Count; i++)
            {
                var instr = instrs[i];
                var ctor_Version = instr.Operand as MethodReference;
                if (instr.OpCode != OpCodes.Newobj || ctor_Version.DeclaringType?.FullName != "System.Version")
                    continue;

                numVersions = new int[ctor_Version.Parameters.Count];
                for (int j = -numVersions.Length; j < 0; j++)
                    numVersions[j + numVersions.Length] = instrs[j + i].GetInt();

                break;
            }
        }

        if (numVersions == null) {
            throw new InvalidOperationException("Unknown version of TowerFall is being patched. Operation cancelled");
        }

        var version = numVersions.Length switch {
            2 => new Version(numVersions[0], numVersions[1]),
            3 => new Version(numVersions[0], numVersions[1], numVersions[2]),
            4 => new Version(numVersions[0], numVersions[1], numVersions[2], numVersions[3]),
            _ => throw new InvalidOperationException("Unknown version of TowerFall is being patched. Operation cancelled")
        };
        var minimumVersion = new Version(1, 3, 3, 1);
        if (version.Major == 0)
        {
            version = minimumVersion;
        }

        if (version < minimumVersion)
        {
            throw new Exception($"Unsupported version of TowerFall: {version}, currently supported: {minimumVersion}");
        }
        Version = version;
        Console.WriteLine("[FortRise] TowerFall Version is: " + Version);

        if (IsTowerFall)
        {
            // Ensure that TowerFall assembly is not already modded
            // (https://github.com/MonoMod/MonoMod#how-can-i-check-if-my-assembly-has-been-modded)
            if (modder.FindType("MonoMod.WasHere") != null)
                throw new Exception("This version of TowerFall is already modded. You need a clean install of TowerFall to mod it.");
        }

        Console.WriteLine($"[FortRise] Platform Found: {PlatformDetection.OS}");

        if (IsFNA && RelinkAgainstFNA(modder))
            Console.WriteLine("[FortRise] Relinking to FNA");

        static void VisitType(TypeDefinition type) {
            // Remove readonly attribute from all static fields
            // This "fixes" https://github.com/dotnet/runtime/issues/11571, which breaks some mods
            foreach (FieldDefinition field in type.Fields)
                if ((field.Attributes & Mono.Cecil.FieldAttributes.Static) != 0)
                    field.Attributes &= ~Mono.Cecil.FieldAttributes.InitOnly;

            // Visit nested types
            foreach (TypeDefinition nestedType in type.NestedTypes)
                VisitType(nestedType);
        }

        foreach (TypeDefinition type in modder.Module.Types)
            VisitType(type);
    }
}
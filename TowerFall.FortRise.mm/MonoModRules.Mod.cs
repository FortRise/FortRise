using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace MonoMod;

internal static partial class MonoModRules 
{
    private static void ModPatch(MonoModder modder)
    {
        modder.PostProcessors += ModPostProcessor;
        Console.WriteLine("[FortRise] Mod Relinking");
        if (IsFNA && RelinkAgainstFNA(modder))
        {
            Console.WriteLine("[FortRise] Relinked to FNA");
        }
    }

    private static void ModPostProcessor(MonoModder modder) 
    {

    }
}
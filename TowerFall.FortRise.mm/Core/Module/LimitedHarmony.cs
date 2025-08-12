#nullable enable
using System.Reflection;
using HarmonyLib;

namespace FortRise;

internal sealed class LimitedHarmony(Harmony harmony) : IHarmony
{
    private Harmony Harmony { get; init; } = harmony;
    public void Patch(MethodBase original, HarmonyMethod? prefix = null, HarmonyMethod? postfix = null, HarmonyMethod? finalizer = null, HarmonyMethod? transpiler = null)
    {
        Harmony.Patch(original, prefix, postfix, transpiler, finalizer);
    }

    public void PatchAll()
    {
        Harmony.PatchAll();
    }

    public void PatchAll(Assembly assembly)
    {
        Harmony.PatchAll(assembly);
    }

    public void ReversePatch(MethodBase original, HarmonyMethod standin, MethodInfo transpiler)
    {
        Harmony.ReversePatch(original, standin, transpiler);
    }

    public void Unpatch(MethodBase original, MethodInfo patch)
    {
        Harmony.Unpatch(original, patch);
    }

    public void UnpatchAll() 
    {
        Harmony.UnpatchAll();
    }
}

#nullable enable
using System.Reflection;
using HarmonyLib;

namespace FortRise;

public interface IHarmony
{
    void Patch(MethodBase original, HarmonyMethod? prefix = null, HarmonyMethod? postfix = null, HarmonyMethod? finalizer = null, HarmonyMethod? transpiler = null);
    void PatchAll();
    void PatchAll(Assembly assembly);
    void Unpatch(MethodBase original, MethodInfo patch);
}

internal sealed class LimitedHarmony(Harmony harmony) : IHarmony
{
    private Harmony Harmony { get; init; } = harmony;
    public void Patch(MethodBase original, HarmonyMethod? prefix = null, HarmonyMethod? postfix = null, HarmonyMethod? finalizer = null, HarmonyMethod? transpiler = null)
    {
        Harmony.Patch(original, prefix, postfix, transpiler, finalizer, null);
    }

    public void PatchAll()
    {
        Harmony.PatchAll();
    }

    public void PatchAll(Assembly assembly)
    {
        Harmony.PatchAll(assembly);
    }

    public void Unpatch(MethodBase original, MethodInfo patch)
    {
        Harmony.Unpatch(original, patch);
    }
}
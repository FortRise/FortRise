#nullable enable
using System.Reflection;
using HarmonyLib;

namespace FortRise;

public interface IHarmony
{
    void Patch(MethodBase original, HarmonyMethod? prefix = null, HarmonyMethod? postfix = null, HarmonyMethod? finalizer = null, HarmonyMethod? transpiler = null);
    void PatchAll();
    void PatchAll(Assembly assembly);
    void ReversePatch(MethodBase original, HarmonyMethod standin, MethodInfo transpiler);
    void Unpatch(MethodBase original, MethodInfo patch);
    internal void UnpatchAll();
}

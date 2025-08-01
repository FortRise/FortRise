#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using HarmonyLib;
using Jint;
using Jint.Native;
using MonoMod.Utils;

namespace FortRise;

internal sealed partial class LuaHarmony(IHarmony harmony)
{
    private IHarmony harmony = harmony;
    private static string methodNamePipe = null!;
    private static JsValue jsValuePipe = null!;

    private static MethodInfo jsFuncCall =
        typeof(Jint.JsValueExtensions).GetMethod("Call", [typeof(JsValue)])!;

    public void Patch(string typeName, string methodName, JsValue jsValue)
    {
        string name = typeName;

        var type = AccessTools.TypeByName(name);

        var t = AccessTools.DeclaredMethod(type, methodName);
        var func = jsValue.AsFunctionInstance();

        string dmdType = Environment.GetEnvironmentVariable("MONOMOD_DMDType")!;
        Environment.SetEnvironmentVariable("MONOMOD_DMDType", "Cecil");

        try
        {
            methodNamePipe = methodName;
            jsValuePipe = func;
            harmony.Patch(t, new HarmonyMethod(Prefix));
        }
        finally
        {
            Environment.SetEnvironmentVariable("MONOMOD_DMDType", dmdType);
        }
    }

    public static DynamicMethod Prefix(MethodBase original)
    {
        DynamicMethod definition = new DynamicMethod(
            name: methodNamePipe + Guid.CreateVersion7().ToString(),
            returnType: null,
            parameterTypes: [],
            m: typeof(LuaHarmony).Module,
            skipVisibility: true
        );
        var il = definition.GetILGenerator();
        il.EmitNewReference(jsValuePipe, out _);
        il.Emit(OpCodes.Call, jsFuncCall);
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Ret);

        return definition;
    }

    private static void NewRef(in int __result)
    {
        var inj = new Injections();
        inj.__result = __result;

        TestPrefix(inj);
    }

    private static void TestPrefix(in Injections injections)
    {

    }

    private ref struct Injections
    {
        public object __instance;
        public ref object __result;
    }
}

internal sealed class LimitedHarmony(Harmony harmony) : IHarmony
{
    private Harmony Harmony { get; init; } = harmony;
    public void Patch(MethodBase original, HarmonyMethod? prefix = null, HarmonyMethod? postfix = null, HarmonyMethod? finalizer = null, HarmonyMethod? transpiler = null)
    {
        Harmony.Patch(original, prefix, postfix, transpiler, finalizer);
    }

    public void Patch(string typeName, string methodName, HarmonyMethod? prefix = null, HarmonyMethod? postfix = null, HarmonyMethod? finalizer = null, HarmonyMethod? transpiler = null)
    {
        var type = AccessTools.TypeByName(typeName);
        var t = AccessTools.DeclaredMethod(type, methodName);
        Harmony.Patch(t, prefix, postfix, transpiler, finalizer);
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
}
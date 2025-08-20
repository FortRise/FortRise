using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.InlineRT;
using MonoMod.Utils;

namespace MonoMod;

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchGlobalPostfix))]
internal class GlobalPostFixAttribute : Attribute
{
    public string TypeName;
    public string MethodName;
    public GlobalPostFixAttribute(string typeName, string methodName, bool isStatic = false)
    {
        TypeName = typeName;
        MethodName = methodName;
    }
}
[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchGlobalPreFix))]
internal class GlobalPreFixAttribute : Attribute
{
    public string TypeName;
    public string MethodName;
    public GlobalPreFixAttribute(string typeName, string methodName, bool isStatic = false)
    {
        TypeName = typeName;
        MethodName = methodName;
    }
}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchPrefix))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class PrefixAttribute : Attribute
{
    public string MethodName;

    public PrefixAttribute(string targetMethod)
    {
        MethodName = targetMethod;
    }
}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchPostfix))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class PostfixAttribute : Attribute
{
    public string MethodName;

    public PostfixAttribute(string targetMethod)
    {
        MethodName = targetMethod;
    }
}


[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldLevelSelectOverlayCtor))]
internal class PatchDarkWorldLevelSelectOverlayCtor : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchSDL2ToSDL3))]
internal class PatchSDL2ToSDL3 : Attribute {}


[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchFlags))]
internal class PatchFlags : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.ObsoletePatch))]
internal class ObsoletePatch : Attribute
{
    public ObsoletePatch(string arg) {}
}

[MonoModCustomMethodAttribute(nameof(MonoModRules.SetsRequiredMembersMethod))]
internal class SetsRequiredMembersMethod : Attribute {}

internal static partial class MonoModRules
{
    private static bool IsTowerFall;
    private static bool IsWindows;
    private static bool IsSteam;
    private static Version Version;
    private static bool IsMod;
    private static bool IsFNA = true;
    public static ModuleDefinition RulesModule;

    static MonoModRules()
    {
        MonoModRule.Modder.MissingDependencyThrow = false;

        if (MonoModRule.Modder.WriterParameters.WriteSymbols)
        {
            MonoModRule.Modder.WriterParameters.SymbolWriterProvider = new PortablePdbWriterProvider();
        }

        IsWindows = PlatformDetection.OS == OSKind.Windows;
        MonoModRule.Flag.Set("OS:Windows", IsWindows);
        MonoModRule.Flag.Set("OS:NotWindows", !IsWindows);
        var execModName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        RulesModule = MonoModRule.Modder.DependencyMap.Keys.FirstOrDefault(mod =>
            execModName == mod.Name.Substring(0, mod.Name.Length - 4) + ".MonoModRules [MMILRT, ID:" + MonoModRulesManager.GetId(MonoModRule.Modder) + "]"
        );

        MonoModRule.Modder.PostProcessors += PostProcessor;
        IsMod = RulesModule == null || !MonoModRule.Modder.Mods.Contains(RulesModule);

        if (IsMod)
        {
            ModPatch(MonoModRule.Modder);
        }
        else
        {
            bool hasSteamworks = false;
            foreach (var name in MonoModRule.Modder.Module.AssemblyReferences)
            {
                if (name.Name.Contains("Steamworks"))
                {
                    hasSteamworks = true;
                    IsSteam = true;
                }
            }
            if (hasSteamworks)
            {
                Console.WriteLine("[FortRise] Running on a Steam Launcher");
            }
            else
            {
                Console.WriteLine("[FortRise] Running TowerFall without a Launcher");
            }

            MonoModRule.Flag.Set("Steamworks", hasSteamworks);
            MonoModRule.Flag.Set("NoLauncher", !hasSteamworks);

            if (MonoModRule.Modder.FindType("TowerFall.TFGame")?.SafeResolve()?.Scope == MonoModRule.Modder.Module)
            {
                GamePatch(MonoModRule.Modder);
            }
        }

        MonoModRule.Modder.PostProcessors += _ => RulesModule = null;
    }

    public static System.Reflection.AssemblyName GetRulesAssemblyRef(string name)
    {
        System.Reflection.AssemblyName asmName = null;
        foreach (var asm in System.Reflection.Assembly.GetExecutingAssembly().GetReferencedAssemblies())
        {
            if (asm.Name.Equals(name))
            {
                asmName = asm;
                break;
            }
        }
        return asmName;
    }

    public static bool ReplaceAssemblyRefs(MonoModder modder, System.Reflection.AssemblyName newRef)
    {
        // Check if the module has a reference affected by the filter
        bool proceed0 = false;
        foreach (var asm in modder.Module.AssemblyReferences)
        {
            if (asm.Name.StartsWith("Microsoft.Xna.Framework"))
            {
                proceed0 = true;
                break;
            }
        }
        if (!proceed0)
            return false;

        // Add new dependency and map it, if it not already exist
        bool hasNewRef = false;
        foreach (var asm in modder.Module.AssemblyReferences)
        {
            if (asm.Name == newRef.Name)
            {
                hasNewRef = true;
                break;
            }
        }
        if (!hasNewRef)
        {
            AssemblyNameReference asmRef = new AssemblyNameReference(newRef.Name, newRef.Version);
            // modder.Module.AssemblyReferences.Add(asmRef);
            modder.MapDependency(modder.Module, asmRef);
            modder.Log("[FortRise] Adding assembly reference to " + asmRef.FullName);
        }

        // Replace old references
        ModuleDefinition newModule = null;
        foreach (var module in modder.DependencyMap[modder.Module])
        {
            if (module.Assembly.Name.Name == newRef.Name)
            {
                newModule = module;
                break;
            }
        }

        for (int i = 0; i < modder.Module.AssemblyReferences.Count; i++)
        {
            AssemblyNameReference asmRef = modder.Module.AssemblyReferences[i];
            if (!asmRef.Name.StartsWith("Microsoft.Xna.Framework"))
                continue;

            // Remove dependency
            modder.Module.AssemblyReferences.RemoveAt(i--);
            var listToRemove = new List<ModuleDefinition>();
            foreach (var mod in modder.DependencyMap[modder.Module])
            {
                if (mod.Assembly.FullName == asmRef.FullName)
                {
                    listToRemove.Add(mod);
                }
            }
            foreach (var item in listToRemove)
            {
                modder.DependencyMap[modder.Module].Remove(item);
            }
            modder.RelinkModuleMap[asmRef.Name] = newModule;
            modder.Log("[FortRise] Replacing assembly reference " + asmRef.FullName + " -> " + newRef.FullName);
        }

        return !hasNewRef;
    }


    private static bool RelinkAgainstFNA(MonoModder modder)
    {
        try
        {
            // Check if the module references either XNA or FNA
            bool proceed = false;
            foreach (var asm in modder.Module.AssemblyReferences)
            {
                if (asm.Name == "FNA" || asm.Name.StartsWith("Microsoft.Xna.Framework"))
                {
                    proceed = true;
                    break;
                }
            }
            if (!proceed)
                return false;
            // if (!modder.Module.AssemblyReferences.Any(asmRef => asmRef.Name == "FNA" || asmRef.Name.StartsWith("Microsoft.Xna.Framework")))
            //     return false;

            // Replace XNA assembly references with FNA ones
            bool didReplaceXNA = ReplaceAssemblyRefs(MonoModRule.Modder, GetRulesAssemblyRef("FNA"));

            // Ensure that FNA.dll can be loaded
            if (MonoModRule.Modder.FindType("Microsoft.Xna.Framework.Game")?.SafeResolve() == null)
                throw new Exception("Failed to resolve Microsoft.Xna.Framework.Game");

            return didReplaceXNA;
        }
        catch
        {
            Console.WriteLine("[FortRise] Cannot be Relinked to FNA");
            return false;
        }
    }

    public static void SetsRequiredMembersMethod(ILContext ctx, CustomAttribute attrib)
    {
        var obsoleteAttributeRef = ctx.Module.ImportReference(
            typeof(SetsRequiredMembersAttribute)
        .GetConstructor([]));
        var setsRequiredMember = new CustomAttribute(obsoleteAttributeRef);
        ctx.Method.CustomAttributes.Add(setsRequiredMember);
    }

    public static void ObsoletePatch(ILContext ctx, CustomAttribute attrib)
    {
        string strArg = attrib.ConstructorArguments[0].Value as string;
        var typeRef = ctx.Module.ImportReference(
            typeof(System.String));
        var obsoleteAttributeRef = ctx.Module.ImportReference(
            typeof(System.ObsoleteAttribute)
        .GetConstructor(new Type[1] { typeof(System.String) }));
        var obsolete = new CustomAttribute(obsoleteAttributeRef);
        obsolete.ConstructorArguments.Add(new CustomAttributeArgument(typeRef, strArg));
        ctx.Method.CustomAttributes.Add(obsolete);
    }

    public static void PatchGlobalPostfix(ILContext ctx, CustomAttribute attrib)
    {
        string typeName = (string)attrib.ConstructorArguments[0].Value;
        string methodName = (string)attrib.ConstructorArguments[1].Value;
        bool isStatic = (bool)attrib.ConstructorArguments[2].Value;

        var method = ctx.Module.GetType(typeName).FindMethod(methodName);

        var cursor = new ILCursor(ctx);
        while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchRet())) { }
        if (!isStatic)
            cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, method);
    }

    public static void PatchGlobalPreFix(ILContext ctx, CustomAttribute attrib)
    {
        string typeName = (string)attrib.ConstructorArguments[0].Value;
        string methodName = (string)attrib.ConstructorArguments[1].Value;
        bool isStatic = (bool)attrib.ConstructorArguments[2].Value;

        var method = ctx.Module.GetType(typeName).FindMethod(methodName);

        var cursor = new ILCursor(ctx);
        if (!isStatic)
            cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, method);
    }

    public static void PatchFlags(ILContext ctx, CustomAttribute attrib)
    {
        var IsWindows = ctx.Module.GetType("FortRise.RiseCore").FindProperty("IsWindows").SetMethod;
        var IsSteam = ctx.Module.GetType("FortRise.RiseCore").FindProperty("IsSteam").SetMethod;
        var cursor = new ILCursor(ctx);

        if (MonoModRules.IsWindows)
            cursor.Emit(OpCodes.Ldc_I4_1);
        else
            cursor.Emit(OpCodes.Ldc_I4_0);

        cursor.Emit(OpCodes.Call, IsWindows);

        if (MonoModRules.IsSteam)
            cursor.Emit(OpCodes.Ldc_I4_1);
        else
            cursor.Emit(OpCodes.Ldc_I4_0);
        cursor.Emit(OpCodes.Call, IsSteam);
    }

    public static void PatchDarkWorldLevelSelectOverlayCtor(ILContext ctx, CustomAttribute attrib)
    {
        var TowerFall_MapScene = ctx.Module.Assembly.MainModule.GetType("TowerFall", "MapScene");
        var Selection = TowerFall_MapScene.FindField("Selection");

        var TowerFall_MapButton = ctx.Module.Assembly.MainModule.GetType("TowerFall", "MapButton");
        var get_Data = TowerFall_MapButton.FindMethod("TowerFall.TowerMapData get_Data()", false);

        var cursor = new ILCursor(ctx);
        var label = ctx.DefineLabel();
        cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.DarkWorldLevelSelectOverlay", "drawStatsLerp"));

        cursor.Emit(OpCodes.Ldarg_1);
        cursor.Emit(OpCodes.Ldfld, Selection);
        cursor.Emit(OpCodes.Callvirt, get_Data);

        cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.DarkWorldLevelSelectOverlay", "statsID"));
        cursor.MarkLabel(label);
        cursor.GotoPrev(MoveType.After, instr => instr.MatchCallvirt(get_Data));
        cursor.Emit(OpCodes.Brfalse_S, label);
    }

    public static void PostProcessMacros(MonoModder modder) { }

    // https://github.com/EverestAPI/Everest/blob/f4545220fe22ed3f752e358741befe9cc7546234/Celeste.Mod.mm/MonoModRules.cs
    // This is to fix the Enumerators can't be decompiled
    public static void FixEnumeratorDecompile(MonoModder modder, TypeDefinition type)
    {
        foreach (MethodDefinition method in type.Methods)
        {
            new ILContext(method).Invoke(il =>
            {
                ILCursor cursor = new ILCursor(il);
                while (cursor.TryGotoNext(instr => instr.MatchCallvirt(out MethodReference m) &&
                    (m.Name is "System.Collections.IEnumerable.GetEnumerator" or "System.IDisposable.Dispose" ||
                        m.Name.StartsWith("<>m__Finally")))
                )
                {
                    cursor.Next.OpCode = OpCodes.Call;
                }
            });
        }
    }

    /*
    The game is made on Net Framework 4.0, which means it doesn't have IteratorStateMachine.
    By doing this, we can easily hook up the enumerators via GetStateMachineTarget() from MonoMod when using hooks.
    */
    private static void AddIteratorStateMachineAttribute(MonoModder modder, MethodDefinition method)
    {
        var moveNext = method.GetEnumeratorMoveNext();
        if (moveNext == null)
            return;

        var typeRef = modder.Module.ImportReference(
            typeof(System.Type));
        var impl = modder.Module.ImportReference(
            typeof(System.Runtime.CompilerServices.IteratorStateMachineAttribute)
            .GetConstructor(new Type[1] { typeof(Type) }));
        var customAttribute = new CustomAttribute(impl);
        var targetType = moveNext.DeclaringType;
        customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(typeRef, targetType));
        method.CustomAttributes.Add(customAttribute);
    }

    private static void PostProcessor(MonoModder modder)
    {
        foreach (var type in modder.Module.Types)
        {
            PostProcessType(modder, type);
        }
    }

    private static void PostProcessType(MonoModder modder, TypeDefinition type)
    {
        if (type.IsCompilerGeneratedEnumerator() && !IsMod)
        {
            FixEnumeratorDecompile(modder, type);
        }
        foreach (MethodDefinition method in type.Methods)
        {
            method.FixShortLongOps();

            if (!method.HasCustomAttribute("System.Runtime.CompilerServices.IteratorStateMachineAttribute") && !IsMod)
            {
                AddIteratorStateMachineAttribute(modder, method);
            }
        }
        foreach (TypeDefinition nested in type.NestedTypes)
        {
            PostProcessType(modder, nested);
        }
    }

    public static void PatchSDL2ToSDL3(ILContext ctx, CustomAttribute attrib)
    {
        var cursor = new ILCursor(ctx);
        // screw it, just read the module.
        var fna = ModuleDefinition.ReadModule("FNA.dll");

        var sdlRef = fna.GetType("SDL3.SDL");
        var SDL_GetPlatform = ctx.Module.ImportReference(sdlRef.Resolve().FindMethod("System.String SDL_GetPlatform()"));
        while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallOrCallvirt("SDL2.SDL", "SDL_GetPlatform")))
        {
            cursor.Next.Operand = SDL_GetPlatform;
        }
    }


    public record struct MethodParamIdentity(string Name, int Index);
    public record struct ParameterArgument(string Name, int Index, bool ByRef);


    public static void PatchPrefix(ILContext prefixCtx, CustomAttribute attrib)
    {
        Dictionary<string, MethodParamIdentity> identities = new Dictionary<string, MethodParamIdentity>();
        string methodName = (string)attrib.ConstructorArguments[0].Value;
        var type = prefixCtx.Method.DeclaringType;

        var origMethod = prefixCtx.Method.DeclaringType.FindMethod(methodName);

        bool isStatic = origMethod.IsStatic;
        bool isUserStatic = prefixCtx.Method.IsStatic;

        var parameters = origMethod.Parameters;
        for (int i = 0; i < parameters.Count; i++)
        {
            string name = parameters[i].Name;
            int index = i;
            if (!isStatic)
            {
                index += 1;
            }
            identities.Add(name, new MethodParamIdentity(name, index));
        }

        bool canCallOriginalOrNot = prefixCtx.Method.ReturnType.FullName == prefixCtx.Module.TypeSystem.Boolean.FullName;

        VariableDefinition exists = null;

        var ctx = new ILContext(origMethod);

        var cursor = new ILCursor(ctx);
        var label = cursor.MarkLabel();

        cursor.GotoNext(MoveType.Before, instr => instr.MatchRet());
        var retLabel = cursor.MarkLabel();
        cursor.Goto(0);

        if (canCallOriginalOrNot)
        {
            var t = ctx.Module.TypeSystem.Boolean;
            exists = new VariableDefinition(t);
            cursor.IL.Body.Variables.Add(exists);
        }

        if (!isUserStatic)
        {
            cursor.Emit(OpCodes.Ldarg_0);
        }
        var varParam = CompareAndBuildParameter(identities, prefixCtx.Method);
        for (int i = 0; i < varParam.Count; i++)
        {
            var p = varParam[i];
            if (p.Name == "__result")
            {
                throw new Exception("__result is not supported in 'prefix' operation.");
            }
            if (p.ByRef)
            {
                cursor.Emit(OpCodes.Ldarga, (ushort)p.Index);
                continue;
            }
            cursor.Emit(OpCodes.Ldarg, (ushort)p.Index);
        }

        if (isUserStatic)
        {
            cursor.Emit(OpCodes.Call, prefixCtx.Method);
        }
        else
        {
            cursor.Emit(OpCodes.Callvirt, prefixCtx.Method);
        }

        if (exists != null)
        {
            cursor.Emit(OpCodes.Stloc, exists);
            if (retLabel != null)
            {
                cursor.Emit(OpCodes.Ldloc, exists);
                cursor.Emit(OpCodes.Brfalse, retLabel);
            }
        }
    }

    public static void PatchPostfix(ILContext postfixCtx, CustomAttribute attrib)
    {
        Dictionary<string, MethodParamIdentity> identities = new Dictionary<string, MethodParamIdentity>();
        string methodName = (string)attrib.ConstructorArguments[0].Value;

        var origMethod = postfixCtx.Method.DeclaringType.FindMethod(methodName);

        bool isStatic = origMethod.IsStatic;
        bool isUserStatic = postfixCtx.Method.IsStatic;

        var parameters = origMethod.Parameters;
        for (int i = 0; i < parameters.Count; i++)
        {
            string name = parameters[i].Name;
            int index = i;
            if (!isStatic)
            {
                index += 1;
            }
            identities.Add(name, new MethodParamIdentity(name, index));
        }

        var ctx = new ILContext(origMethod);

        var cursor = new ILCursor(ctx);

        int current = cursor.Instrs.Count - 1;

        cursor.Index = current;
        // idk if this is a good idea, but we need to retarget the labels that is referencing the ret
        // to make sure that the label is correct

        var lastNext = cursor.Next;

        if (!isUserStatic)
        {
            cursor.Emit(OpCodes.Ldarg_0);
        }

        VariableDefinition retVal = null;
        if (origMethod.ReturnType.FullName == postfixCtx.Module.TypeSystem.Void.FullName)
        {
            var varParamVoid = CompareAndBuildParameter(identities, postfixCtx.Method);
            for (int i = 0; i < varParamVoid.Count; i++)
            {
                var p = varParamVoid[i];
                if (p.Name == "__result")
                {
                    throw new Exception("__result is not supported in 'postfix' without a return type.");
                }
                cursor.Emit(OpCodes.Ldarg, (ushort)p.Index);
            }

            if (isUserStatic)
            {
                cursor.Emit(OpCodes.Call, postfixCtx.Method);
            }
            else
            {
                cursor.Emit(OpCodes.Callvirt, postfixCtx.Method);
            }
        }
        else
        {
            retVal = new VariableDefinition(ctx.Module.ImportReference(origMethod.ReturnType));
            cursor.IL.Body.Variables.Add(retVal);

            cursor.Emit(OpCodes.Stloc, retVal);

            var varParam = CompareAndBuildParameter(identities, postfixCtx.Method);
            for (int i = 0; i < varParam.Count; i++)
            {
                var p = varParam[i];
                if (p.Name == "__result")
                {
                    if (p.ByRef)
                    {
                        cursor.Emit(OpCodes.Ldloca, retVal);
                    }
                    else
                    {
                        cursor.Emit(OpCodes.Ldloc, retVal);
                    }
                }
                else
                {
                    if (p.ByRef)
                    {
                        cursor.Emit(OpCodes.Ldarga, (ushort)p.Index);
                    }
                    else
                    {
                        cursor.Emit(OpCodes.Ldarg, (ushort)p.Index);
                    }
                }
            }

            if (isUserStatic)
            {
                cursor.Emit(OpCodes.Call, postfixCtx.Method);
            }
            else
            {
                cursor.Emit(OpCodes.Callvirt, postfixCtx.Method);
            }
        }
        cursor.Remove();

        cursor.Instrs.Insert(current, lastNext);
        lastNext.OpCode = OpCodes.Nop;
        if (ctx.Method.ReturnType.FullName != "System.Void" && retVal != null)
        {
            cursor.Emit(OpCodes.Ldloc, retVal);
        }
        
        cursor.Emit(OpCodes.Ret);
    }

    private static List<ParameterArgument> CompareAndBuildParameter(Dictionary<string, MethodParamIdentity> identities, MethodDefinition method)
    {
        var list = new List<ParameterArgument>();
        var parameters = method.Parameters;
        for (int i = 0; i < parameters.Count; i++)
        {
            var p = parameters[i];
            string name = p.Name!;
            bool byRef = p.ParameterType.IsByReference;
            if (identities.TryGetValue(parameters[i].Name!, out MethodParamIdentity val))
            {
                list.Add(new ParameterArgument(name, val.Index, byRef));
                continue;
            }

            if (name == "__instance")
            {
                list.Add(new ParameterArgument("__instance", 0, byRef));
                continue;
            }

            if (name == "__result")
            {
                list.Add(new ParameterArgument("__result", 0, byRef));
                continue;
            }

            if (name == "__exception")
            {
                list.Add(new ParameterArgument("__exception", 0, byRef));
                continue;
            }

            throw new Exception($"Parameter name: '{parameters[i].Name}' does not exists on method '{method.Name}'");
        }

        return list;
    }
}

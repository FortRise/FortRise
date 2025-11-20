using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Utils;
using MethodBody = Mono.Cecil.Cil.MethodBody;
using MethodImplAttributes = Mono.Cecil.MethodImplAttributes;

namespace FortLauncher;

public sealed class FortRiseMonoModder : MonoModder
{
    public void AddReferenceIfMissing(AssemblyName asmName) {
        if (!Module.AssemblyReferences.Any(asmRef => asmRef.Name == asmName.Name)) {
            Module.AssemblyReferences.Add(AssemblyNameReference.Parse(asmName.FullName));
        }
    }

    public void AddReferenceIfMissing(string name) => 
        AddReferenceIfMissing(
            Assembly.GetExecutingAssembly()
            .GetReferencedAssemblies()
            .First(asmName => asmName.Name == name));

    public override void MapDependencies()
    {
        base.MapDependencies();
        AddReferenceIfMissing("System.Runtime");
    }

    public override void PatchRefsInMethod(MethodDefinition method)
    {
        base.PatchRefsInMethod(method);
        // Inlining can cause problems on modding, so we need to make sure that it won't inline in some cases
        if (!method.FullName.Contains("FortRise")
            && !method.FullName.Contains("XXHash") &&
            (method.ImplAttributes & MethodImplAttributes.AggressiveInlining) == 0 &&
            method.Body is MethodBody body &&
            CanInlineLegacyCode(body))
        {
            method.ImplAttributes |= MethodImplAttributes.NoInlining;
        }

        // Resolve uninstantiated generic typeref/def tokens
        if (method.DeclaringType.HasGenericParameters && method.Body != null)
        {
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                Instruction instr = method.Body.Instructions[i];

                if (instr.OpCode == OpCodes.Ldtoken)
                {
                    continue;
                }

                if (instr.Operand is TypeReference typeRef && typeRef.SafeResolve() == method.DeclaringType && !typeRef.IsGenericInstance)
                {
                    GenericInstanceType instanceType = new GenericInstanceType(typeRef);
                    instanceType.GenericArguments.AddRange(method.DeclaringType.GenericParameters);
                    instr.Operand = instanceType;
                    continue;
                }

                if (instr.Operand is MemberReference memRef && instr.Operand is not TypeReference 
                    && memRef.DeclaringType.SafeResolve() == method.DeclaringType && !memRef.DeclaringType.IsGenericInstance)
                {
                    GenericInstanceType instanceType = new GenericInstanceType(memRef.DeclaringType);
                    instanceType.GenericArguments.AddRange(method.DeclaringType.GenericParameters);
                    instr.Operand = instanceType;
                }
            }
        }
    }

    private bool CanInlineLegacyCode(MethodBody body) {
        const int INLINE_LENGTH_LIMIT = 20; // mono/mini/method-to-ir.c

        // Methods exceeding a certain size aren't inlined
        if (body.CodeSize >= INLINE_LENGTH_LIMIT)
            return false;

        return true;
    }
}
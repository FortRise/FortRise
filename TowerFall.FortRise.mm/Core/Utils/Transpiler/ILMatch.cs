#nullable enable
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Mono.Cecil;
using MonoMod.Utils;

namespace FortRise.Transpiler;

public static class ILMatch
{
    public static InstructionMatcher Match(OpCode opcode) =>
        new InstructionMatcher((instr) => instr.opcode == opcode);

    public static InstructionMatcher Match(OpCode opcode, object? operand) =>
        new InstructionMatcher((instr) => instr.opcode == opcode && instr.operand == operand);

    public static InstructionMatcher Exact(string str) =>
        new InstructionMatcher((instr) => instr.ToString() == str);

    public static InstructionMatcher Contains(string contains) =>
        new InstructionMatcher((instr) => instr.ToString().Contains(contains));

    public static InstructionMatcher Contains(string contains, StringComparison comparison) =>
        new InstructionMatcher((instr) => instr.ToString().Contains(contains, comparison));

    public static InstructionMatcher LdcI4(int num) =>
        new InstructionMatcher(
            (instr) =>
            {
                switch (num)
                {
                    case 0 when instr.opcode == OpCodes.Ldc_I4_0:
                        return true;
                    case 1 when instr.opcode == OpCodes.Ldc_I4_1:
                        return true;
                    case 2 when instr.opcode == OpCodes.Ldc_I4_2:
                        return true;
                    case 3 when instr.opcode == OpCodes.Ldc_I4_3:
                        return true;
                    case 4 when instr.opcode == OpCodes.Ldc_I4_4:
                        return true;
                    case 5 when instr.opcode == OpCodes.Ldc_I4_5:
                        return true;
                    case 6 when instr.opcode == OpCodes.Ldc_I4_6:
                        return true;
                    case 7 when instr.opcode == OpCodes.Ldc_I4_7:
                        return true;
                    case 8 when instr.opcode == OpCodes.Ldc_I4_8:
                        return true;
                    case -1 when instr.opcode == OpCodes.Ldc_I4_M1:
                        return true;
                    default:
                        return (instr.opcode == OpCodes.Ldc_I4 && (int)instr.operand == num)
                            || (
                                num < byte.MaxValue
                                && instr.opcode == OpCodes.Ldc_I4_S
                                && (
                                    (instr.operand is int v && v == num)
                                    || (instr.operand is sbyte b && b == num)
                                )
                            );
                }
            }
        );

    public static InstructionMatcher LdcR4(float num) =>
        new InstructionMatcher(
            (instr) =>
            {
                return instr.opcode == OpCodes.Ldc_R4 && (float)instr.operand == num;
            }
        );

    public static InstructionMatcher LdcR8(double num) =>
        new InstructionMatcher(
            (instr) =>
            {
                return instr.opcode == OpCodes.Ldc_R8 && (double)instr.operand == num;
            }
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool StlocMatch(CodeInstruction instr) =>
        instr.opcode == OpCodes.Stloc_S
        || instr.opcode == OpCodes.Stloc
        || instr.opcode == OpCodes.Stloc_0
        || instr.opcode == OpCodes.Stloc_1
        || instr.opcode == OpCodes.Stloc_2
        || instr.opcode == OpCodes.Stloc_3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LdlocMatch(CodeInstruction instr) =>
        instr.opcode == OpCodes.Ldloc_S
        || instr.opcode == OpCodes.Ldloc
        || instr.opcode == OpCodes.Ldloc_0
        || instr.opcode == OpCodes.Ldloc_1
        || instr.opcode == OpCodes.Ldloc_2
        || instr.opcode == OpCodes.Ldloc_3;

    private static bool LdlocaMatch(CodeInstruction instr) =>
        instr.opcode == OpCodes.Ldloca_S || instr.opcode == OpCodes.Ldloca;

    public static InstructionMatcher Ldarg()
    {
        return new InstructionMatcher((instr) => instr.IsLdarg());
    }

    public static InstructionMatcher Ldarg(int n)
    {
        return new InstructionMatcher((instr) => instr.IsLdarg(n));
    }

    public static InstructionMatcher Ldarga()
    {
        return new InstructionMatcher((instr) => instr.IsLdarga());
    }

    public static InstructionMatcher Ldarga(int n)
    {
        return new InstructionMatcher((instr) => instr.IsLdarga(n));
    }

    public static InstructionMatcher Stloc()
    {
        return new InstructionMatcher((instr) => StlocMatch(instr));
    }

    public static InstructionMatcher Stloc(int index)
    {
        return new InstructionMatcher(
            (instr) =>
            {
                return StlocMatch(instr)
                    && instr.TryGetLocalIndex(out int localIndex)
                    && localIndex == index;
            }
        );
    }

    public static InstructionMatcher Ldloc()
    {
        return new InstructionMatcher((instr) => LdlocMatch(instr));
    }

    public static InstructionMatcher Ldloc(int index)
    {
        return new InstructionMatcher(
            (instr) =>
            {
                return LdlocMatch(instr)
                    && instr.TryGetLocalIndex(out int localIndex)
                    && localIndex == index;
            }
        );
    }

    public static InstructionMatcher LdlocS()
    {
        return new InstructionMatcher((instr) => instr.opcode == OpCodes.Ldloc_S);
    }

    public static InstructionMatcher LdlocS(int index)
    {
        return new InstructionMatcher(
            (instr) =>
            {
                return instr.opcode == OpCodes.Ldloc_S
                    && instr.TryGetLocalIndex(out int localIndex)
                    && localIndex == index;
            }
        );
    }

    public static InstructionMatcher Ldloca()
    {
        return new InstructionMatcher((instr) => LdlocaMatch(instr));
    }

    public static InstructionMatcher Ldloca(int index)
    {
        return new InstructionMatcher(
            (instr) =>
            {
                return LdlocaMatch(instr)
                    && instr.TryGetLocalIndex(out int localIndex)
                    && localIndex == index;
            }
        );
    }

    public static InstructionMatcher LdlocaS()
    {
        return new InstructionMatcher((instr) => instr.opcode == OpCodes.Ldloca_S);
    }

    public static InstructionMatcher LdlocaS(int index)
    {
        return new InstructionMatcher(
            (instr) =>
            {
                return instr.opcode == OpCodes.Ldloca_S
                    && instr.TryGetLocalIndex(out int localIndex)
                    && localIndex == index;
            }
        );
    }

    public static InstructionMatcher Ldnull()
    {
        return new InstructionMatcher((instr) => instr.opcode == OpCodes.Ldnull);
    }

    public static InstructionMatcher Stsfld(string fieldName) =>
        new InstructionMatcher(
            (instr) =>
                instr.opcode == OpCodes.Stsfld && (instr.operand as FieldInfo)?.Name == fieldName
        );

    public static InstructionMatcher Stsfld<T>() =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as FieldInfo;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Stsfld) && operand.FieldType.Name == typeName;
            }
        );

    public static InstructionMatcher Stsfld<T>(string fieldName) =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as FieldInfo;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Stsfld)
                    && operand.FieldType.Name == typeName
                    && operand.Name == fieldName;
            }
        );

    public static InstructionMatcher Stfld(string fieldName) =>
        new InstructionMatcher(
            (instr) =>
                instr.opcode == OpCodes.Stfld && (instr.operand as FieldInfo)?.Name == fieldName
        );

    public static InstructionMatcher Stfld<T>() =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as FieldInfo;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Stfld) && operand.FieldType.Name == typeName;
            }
        );

    public static InstructionMatcher Stfld<T>(string fieldName) =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as FieldInfo;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Stfld)
                    && operand.FieldType.Name == typeName
                    && operand.Name == fieldName;
            }
        );

    // public bool MatchContainsAndExtract(string contains, out object? operand)
    // {
    //     if (MatchContains(contains))
    //     {
    //         operand = Instruction.operand;
    //         return true;
    //     }
    //     operand = null;
    //     return false;
    // }

    public static InstructionMatcher Ldsfld(string fieldName) =>
        new InstructionMatcher(
            (instr) =>
                (instr.opcode == OpCodes.Ldsfld) && (instr.operand as FieldInfo)?.Name == fieldName
        );

    public static InstructionMatcher Ldsfld<T>() =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as FieldInfo;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Ldsfld) && operand.FieldType.Name == typeName;
            }
        );

    public static InstructionMatcher Ldsfld<T>(string fieldName) =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as FieldInfo;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Ldsfld)
                    && operand.Name == fieldName
                    && operand.FieldType.Name == typeName;
            }
        );

    public static InstructionMatcher Ldfld(string fieldName) =>
        new InstructionMatcher(
            (instr) =>
                (instr.opcode == OpCodes.Ldfld) && (instr.operand as FieldInfo)?.Name == fieldName
        );

    public static InstructionMatcher Ldfld<T>() =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as FieldInfo;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Ldfld) && operand.FieldType.Name == typeName;
            }
        );

    public static InstructionMatcher Ldfld<T>(string fieldName) =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as FieldInfo;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Ldfld)
                    && operand.Name == fieldName
                    && operand.FieldType.Name == typeName;
            }
        );

    public static InstructionMatcher Ldflda(string fieldName) =>
        new InstructionMatcher(
            (instr) =>
                (instr.opcode == OpCodes.Ldflda) && (instr.operand as FieldInfo)?.Name == fieldName
        );

    public static InstructionMatcher Ldflda<T>() =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as FieldInfo;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Ldflda) && operand.FieldType.Name == typeName;
            }
        );

    public static InstructionMatcher Ldflda<T>(string fieldName) =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as FieldInfo;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Ldflda)
                    && operand.Name == fieldName
                    && operand.FieldType.Name == typeName;
            }
        );

    public static InstructionMatcher Ldstr() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Ldstr 
        );
    
    public static InstructionMatcher Ldstr(string text) =>
        new InstructionMatcher(
            (instr) =>
                instr.opcode == OpCodes.Ldstr && instr.operand.ToString() == text
        );

    public static InstructionMatcher Call(string methodName) =>
        new InstructionMatcher(
            (instr) =>
                instr.opcode == OpCodes.Call && (instr.operand as MethodBase)?.Name == methodName
        );

    public static InstructionMatcher Call<T>(string methodName) =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as MethodBase;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Call)
                    && operand.Name == methodName
                    && operand.DeclaringType?.Name == typeName;
            }
        );

    public static InstructionMatcher Callvirt() =>
        new InstructionMatcher((instr) => instr.opcode == OpCodes.Callvirt);

    public static InstructionMatcher Callvirt(string methodName) =>
        new InstructionMatcher(
            (instr) =>
                instr.opcode == OpCodes.Callvirt
                && (instr.operand as MethodBase)?.Name == methodName
        );

    public static InstructionMatcher Callvirt<T>(string methodName) =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as MethodBase;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Callvirt)
                    && operand.Name == methodName
                    && operand.DeclaringType?.Name == typeName;
            }
        );

    public static InstructionMatcher CallOrCallvirt(string methodName) =>
        new InstructionMatcher(
            (instr) =>
                (instr.opcode == OpCodes.Callvirt || instr.opcode == OpCodes.Callvirt)
                && (instr.operand as MethodBase)?.Name == methodName
        );

    public static InstructionMatcher CallOrCallvirt<T>(string methodName) =>
        new InstructionMatcher(
            (instr) =>
            {
                string typeName = typeof(T).Name;
                var operand = instr.operand as MethodBase;
                if (operand is null)
                {
                    return false;
                }

                return (instr.opcode == OpCodes.Callvirt || instr.opcode == OpCodes.Callvirt)
                    && operand.Name == methodName
                    && operand.DeclaringType?.Name == typeName;
            }
        );

    public static InstructionMatcher Isinst() =>
        new InstructionMatcher((instr) => instr.opcode == OpCodes.Isinst);

    public static InstructionMatcher Isinst<T>() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Isinst && (instr.operand as Type) == typeof(T)
        );

    public static InstructionMatcher Initobj(string objName) =>
        new InstructionMatcher(
            (instr) =>
                instr.opcode == OpCodes.Initobj
                && (instr.operand as Type)?.Name == objName
        );

    public static InstructionMatcher Initobj<T>() =>
        new InstructionMatcher(
            (instr) =>
                instr.opcode == OpCodes.Initobj
                && (instr.operand as Type) == typeof(T)
        );

    public static InstructionMatcher Initobj(Type type) =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Initobj && (instr.operand as Type) == type
        );

    public static InstructionMatcher Newobj(string objName) =>
        new InstructionMatcher(
            (instr) =>
                instr.opcode == OpCodes.Newobj
                && (instr.operand as ConstructorInfo)?.DeclaringType?.Name == objName
        );

    public static InstructionMatcher Newobj<T>() =>
        new InstructionMatcher(
            (instr) =>
                instr.opcode == OpCodes.Newobj
                && (instr.operand as ConstructorInfo)?.DeclaringType == typeof(T)
        );

    public static InstructionMatcher Newobj(ConstructorInfo info) =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Newobj && (instr.operand as ConstructorInfo) == info
        );

     public static InstructionMatcher Throw() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Throw
        );

     public static InstructionMatcher Break() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Break
        );

     public static InstructionMatcher Br() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Br
        );

     public static InstructionMatcher Br_S() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Br_S
        );

     public static InstructionMatcher Brfalse() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Brfalse
        );

     public static InstructionMatcher Brfalse_S() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Brfalse_S
        );

     public static InstructionMatcher Brtrue() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Brtrue
        );

     public static InstructionMatcher Brtrue_S() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Brtrue_S
        );

     public static InstructionMatcher Cgt() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Cgt 
        );

     public static InstructionMatcher CgtUn() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Cgt_Un
        );

     public static InstructionMatcher Ceq() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Ceq
        );

     public static InstructionMatcher Clt() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Clt
        );

     public static InstructionMatcher CltUn() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Clt_Un
        );

     public static InstructionMatcher Pop() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Pop
        );

     public static InstructionMatcher Nop() =>
        new InstructionMatcher(
            (instr) => instr.opcode == OpCodes.Nop
        );
}

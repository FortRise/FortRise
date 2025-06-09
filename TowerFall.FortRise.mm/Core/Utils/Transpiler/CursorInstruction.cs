#nullable enable
using System;
using System.Reflection.Emit;
using HarmonyLib;
using Mono.Cecil;
using MonoMod.Utils;

namespace FortRise.Transpiler;

public record struct CursorInstruction(CodeInstruction Instruction) 
{
    public bool Match(OpCode opcode)
    {
        return Instruction.opcode == opcode;
    }

    public bool Match(OpCode opcode, object? operand)
    {
        return Instruction.opcode == opcode && Instruction.operand == operand;
    }

    public bool MatchLdcI4(int num)
    {
        return Instruction.opcode.ToString() == $"ldc.i4.{num}";
    }

    public bool Match(string instructionStr)
    {
        return Instruction.ToString() == instructionStr;
    }

    public bool MatchCall<T>(string name)
    {
        if (Instruction.opcode == OpCodes.Call && Instruction.operand is MethodReference r && r.DeclaringType.Is(typeof(T)) && r.Name == name)
        {
            return true;
        }

        return false;
    }

    public bool MatchCallvirt<T>(string name)
    {
        if (Instruction.opcode == OpCodes.Callvirt && Instruction.operand is MethodReference r && r.DeclaringType.Is(typeof(T)) && r.Name == name)
        {
            return true;
        }

        return false;
    }

    public bool MatchLdfld<T>(string name)
    {
        if (Instruction.opcode == OpCodes.Ldfld && Instruction.operand is FieldReference r && r.DeclaringType.Is(typeof(T)) && r.Name == name)
        {
            return true;
        }

        return false;
    }

    public bool MatchStfld<T>(string name)
    {
        if (Instruction.opcode == OpCodes.Stfld && Instruction.operand is FieldReference r && r.DeclaringType.Is(typeof(T)) && r.Name == name)
        {
            return true;
        }

        return false;
    }

    public bool MatchContains(string contains)
    {
        return Instruction.ToString().Contains(contains);
    }

    public bool MatchExtract(OpCode opcode, out object? operand)
    {
        if (Instruction.opcode == opcode) 
        {
            operand = Instruction.operand;
            return true;
        }
        operand = null;
        return false;
    }
}

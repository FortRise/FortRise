using System.Reflection.Emit;
using HarmonyLib;

namespace FortRise.Transpiler;

public static class CodeInstructionExtensions
{
    public static bool TryGetLocalIndex(this CodeInstruction instruction, out int localIndex)
    {
        if (instruction.opcode == OpCodes.Ldloc_0 || instruction.opcode == OpCodes.Stloc_0)
        {
            localIndex = 0;
            return true;
        }
        if (instruction.opcode == OpCodes.Ldloc_1 || instruction.opcode == OpCodes.Stloc_1)
        {
            localIndex = 1;
            return true;
        }
        if (instruction.opcode == OpCodes.Ldloc_2 || instruction.opcode == OpCodes.Stloc_2)
        {
            localIndex = 2;
            return true;
        }
        if (instruction.opcode == OpCodes.Ldloc_3 || instruction.opcode == OpCodes.Stloc_3)
        {
            localIndex = 3;
            return true;
        }
        if (instruction.opcode == OpCodes.Ldloc || instruction.opcode == OpCodes.Ldloc_S || instruction.opcode == OpCodes.Ldloca || instruction.opcode == OpCodes.Ldloca_S || instruction.opcode == OpCodes.Stloc || instruction.opcode == OpCodes.Stloc_S)
        {
            return TryGetOperandLocalIndex(instruction.operand, out localIndex);
        }

        localIndex = default;
        return false;
    }

    private static bool TryGetOperandLocalIndex(object operand, out int localIndex)
    {
        switch (operand)
        {
            case LocalBuilder local:
                localIndex = local.LocalIndex;
                return true;
            case int integer:
                localIndex = integer;
                return true;
            case sbyte signedByte:
                localIndex = signedByte;
                return true;
            default:
                localIndex = 0;
                return false;
        }
    }
}
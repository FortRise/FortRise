#nullable enable
using System;
using HarmonyLib;

namespace FortRise.Transpiler;

public struct InstructionMatcher(Func<CodeInstruction, bool> predicate)
{
    private Func<CodeInstruction, bool> predicate = predicate;

    public bool Check(CodeInstruction instr) => predicate(instr);
}

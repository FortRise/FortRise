#nullable enable
using System;
using System.Collections.Generic;
using HarmonyLib;

namespace FortRise.Transpiler;

public struct InstructionMatcher
{
    public readonly IReadOnlyList<Action<CodeInstruction>> Commands => commands;
    public Func<CodeInstruction, bool> Predicate { get; }
    private List<Action<CodeInstruction>> commands = [];

    public InstructionMatcher(Func<CodeInstruction, bool> predicate)
    {
        Predicate = predicate;
    }

    public bool Check(CodeInstruction instr)
    {
        bool pred = Predicate(instr);
        foreach (var c in commands)
        {
            c(instr);
        }
        return pred;
    }

    public InstructionMatcher AddCommand(Action<CodeInstruction> command)
    {
        commands.Add(command);
        return this;
    }
}

public static class InstructionMatcherExtension
{
    extension(InstructionMatcher matcher)
    {
        public InstructionMatcher TryGetLocalIndex(out BoxReference<int> operand)
        {
            BoxReference<int> refs = new BoxReference<int>();
            operand = refs;
            return matcher.AddCommand((instr) =>
            {
                instr.TryGetLocalIndex(out int operand);
                refs.Value = operand;
            });
        }
    }
}

public class BoxReference<T>
where T : struct
{
    public T? Value;
}
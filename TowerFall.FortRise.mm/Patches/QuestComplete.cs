using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public class patch_QuestComplete : QuestComplete
    {
        public patch_QuestComplete(QuestRoundLogic quest) : base(quest)
        {
        }

        [MonoModIgnore]
        [PatchQuestCompleteSequence]
        public extern IEnumerator Sequence();
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestCompleteSequence))]
    public class PatchQuestCompleteSequence : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchQuestCompleteSequence(MethodDefinition method, CustomAttribute attrib) 
        {
            MethodDefinition complete = method.GetEnumeratorMoveNext();

            new ILContext(complete).Invoke(ctx => {
                var this_4 = complete.DeclaringType.FindField("<>4__this");
                var time = this_4.FieldType.Resolve().FindField("time");
                var quest = this_4.FieldType.Resolve().FindField("quest");
                var TFGame_PlayerAmount = ctx.Module.GetType("TowerFall.TFGame").FindMethod("System.Int32 get_PlayerAmount()");
                var eventHook = ctx.Module.GetType("FortRise.RiseCore/Events");
                var invoked = eventHook.FindMethod("System.Void InvokeQuestComplete_Result(TowerFall.QuestRoundLogic,System.Int32,System.Int64,System.Boolean)");

                var num = IsWindows ? "9" : "14";
                var noDeaths = complete.DeclaringType.FindField("<noDeaths>5__" + num);
                // Linux or maybe MacOS has different instructions
                int instrNumToRemove = !IsWindows ? 40 : Version switch {
                    { Major: 1, Minor: 3, Build: 3, Revision: 3 } => 12 + 16,
                    _ => 13 + 18
                };

                var cursor = new ILCursor(ctx);
                Func<Mono.Cecil.Cil.Instruction, bool>[] inst = !IsWindows 
                    ? new Func<Mono.Cecil.Cil.Instruction, bool>[2] { 
                        instr => instr.MatchLdarg(0),
                        instr => instr.MatchLdsfld("TowerFall.SaveData", "Instance")
                    }
                    : new Func<Mono.Cecil.Cil.Instruction, bool>[1] {
                    instr => instr.MatchLdsfld("TowerFall.SaveData", "Instance")
                };

                cursor.GotoNext(inst);

                cursor.RemoveRange(instrNumToRemove);

                // this.quest
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, this_4);
                cursor.Emit(OpCodes.Ldfld, quest);

                // TFGame.PlayerAmount
                cursor.Emit(OpCodes.Call, TFGame_PlayerAmount);

                // // this.time
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, this_4);
                cursor.Emit(OpCodes.Ldfld, time);

                // // noDeaths
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, noDeaths);

                // // Invoke the event
                cursor.Emit(OpCodes.Call, invoked);
            });
        }
    }
}
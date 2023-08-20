using System;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public class patch_QuestRoundLogic : QuestRoundLogic
    {
        public patch_QuestRoundLogic(Session session) : base(session)
        {
        }

        [MonoModIgnore]
        [PatchQuestRoundLogicOnPlayerDeath]
        public extern override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex);

        [MonoModIgnore]
        [PatchQuestRoundLogicOnLevelLoadFinish]
        public extern override void OnLevelLoadFinish();

        [MonoModIgnore]
        [PatchQuestRoundLogicRegisterEnemyKills]
        public extern void RegisterEnemyKill(Vector2 at, int killerIndex, int points);
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestRoundLogicOnPlayerDeath))]
    public class PatchQuestRoundLogicOnPlayerDeath : Attribute {}

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestRoundLogicOnLevelLoadFinish))]
    public class PatchQuestRoundLogicOnLevelLoadFinish : Attribute {}

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestRoundLogicRegisterEnemyKills))]
    public class PatchQuestRoundLogicRegisterEnemyKills : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchQuestRoundLogicRegisterEnemyKills(ILContext ctx, CustomAttribute attrib) 
        {
            var OnQuestRegisterEnemyKills = ctx.Module.GetType("FortRise.RiseCore/Events").FindMethod(
                "System.Void Invoke_OnQuestRegisterEnemyKills(TowerFall.QuestRoundLogic,Microsoft.Xna.Framework.Vector2,System.Int32,System.Int32)"
            );

            var cursor = new ILCursor(ctx);
            ILLabel label = null;
            cursor.GotoNext(instr => instr.MatchBrfalse(out label));

            cursor.GotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt("TowerFall.QuestGauntletCounter", "Decrement"));
            cursor.MarkLabel(label);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.Emit(OpCodes.Ldarg_3);
            cursor.Emit(OpCodes.Call, OnQuestRegisterEnemyKills);
        }

        public static void PatchQuestRoundLogicOnLevelLoadFinish(ILContext ctx, CustomAttribute attrib) 
        {
            var eventHook = ctx.Module.GetType("FortRise.RiseCore/Events");
            var invoked = eventHook.FindMethod("System.Void InvokeQuestRoundLogic_LevelLoadFinish(TowerFall.QuestRoundLogic)");
            var cursor = new ILCursor(ctx);
            cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("TowerFall.SaveData", "Instance"));
            cursor.Next.OpCode = OpCodes.Ldarg_0;
            cursor.Next.Operand = null;
            cursor.GotoNext();
            cursor.RemoveRange(15);

            cursor.Emit(OpCodes.Call, invoked);
        }

        public static void PatchQuestRoundLogicOnPlayerDeath(ILContext ctx, CustomAttribute attrib) 
        {
            var eventHook = ctx.Module.GetType("FortRise.RiseCore/Events");
            var invoked = eventHook.FindMethod("System.Void InvokeQuestRoundLogic_PlayerDeath(TowerFall.QuestRoundLogic)");
            var cursor = new ILCursor(ctx);
            cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("TowerFall.SaveData", "Instance"));
            cursor.RemoveRange(16);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, invoked);
        }
    }
}
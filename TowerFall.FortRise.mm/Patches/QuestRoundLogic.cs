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
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestRoundLogicOnPlayerDeath))]
    public class PatchQuestRoundLogicOnPlayerDeath : Attribute {}

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestRoundLogicOnLevelLoadFinish))]
    public class PatchQuestRoundLogicOnLevelLoadFinish : Attribute {}

    internal static partial class MonoModRules 
    {
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
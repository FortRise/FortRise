using System;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public partial class patch_QuestSpawnPortal : QuestSpawnPortal
    {
        public patch_QuestSpawnPortal(Vector2 position, Vector2[] nodes) : base(position, nodes)
        {
        }


        [MonoModIgnore]
        private extern bool Disappear();

        [PatchQuestSpawnPortalFinishSpawn]
        [MonoModIgnore]
        private extern void FinishSpawn(Sprite<int> sprite);
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestSpawnPortalFinishSpawn))]
    internal class PatchQuestSpawnPortalFinishSpawn : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchQuestSpawnPortalFinishSpawn(ILContext ctx, CustomAttribute attrib) 
        {
            var LevelEntity = ctx.Module.GetType("TowerFall.LevelEntity");
            var get_level = LevelEntity.FindMethod("TowerFall.Level get_Level()");
            var Entity = ctx.Module.GetType("Monocle.Entity");
            var Position = Entity.FindField("Position");
            var RiseCore = ctx.Module.GetType("FortRise.RiseCore/Events");
            var InvokeEvent = RiseCore.FindMethod(
                "System.Void InvokeQuestSpawnPortal_FinishSpawn(System.String,Microsoft.Xna.Framework.Vector2,TowerFall.Facing,TowerFall.Level)");

            var label = ctx.DefineLabel();
            var cursor = new ILCursor(ctx);
            cursor.GotoNext(instr => instr.MatchLdfld("TowerFall.QuestSpawnPortal", "addCounter"));
            cursor.GotoPrev();
            cursor.MarkLabel(label);
            cursor.Index = 0;

            cursor.GotoNext(MoveType.After, instr => instr.MatchLdstr("Unknown enemy type: "));
            cursor.Prev.OpCode = OpCodes.Ldloc_1;

            cursor.RemoveRange(4);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, Position);
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, get_level);
            cursor.Emit(OpCodes.Call, InvokeEvent);
            cursor.Emit(OpCodes.Br, label);
        }
    }
}
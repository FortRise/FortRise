using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall
{
    public class patch_Lava : Lava
    {
        public patch_Lava(LavaControl control, LavaSide side) : base(control, side)
        {
        }

        [MonoModIgnore]
        [PatchLavaUpdate]
        public extern override void Update();
    }
}

namespace MonoMod
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchLavaUpdate))]
    public class PatchLavaUpdate : Attribute { }

    internal static partial class MonoModRules
    {
        public static void PatchLavaUpdate(ILContext ctx, CustomAttribute attrib)
        {
            var invokeTouchedLava = ctx.Module.GetType("TowerFall.Arrow").FindMethod("InvokeHitLava");
            var cursor = new ILCursor(ctx);
            cursor.GotoNext(MoveType.Before, instr => instr.MatchCallvirt("TowerFall.Arrow", "HitLava"));
            cursor.Emit(OpCodes.Dup);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, invokeTouchedLava);
        }
    }
}

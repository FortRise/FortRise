using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall
{
    public class patch_AmaranthBoss : AmaranthBoss
    {
        public patch_AmaranthBoss(int difficulty) : base(difficulty)
        {
        }

        [PatchAmaranthBossDeadCoroutine]
        [MonoModIgnore]
        private extern IEnumerator DeadCoroutine();

        private bool hasPlats(List<Platform> plats) => plats.Count != 0;
    }
}


namespace MonoMod
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchAmaranthBossDeadCoroutine))]
    public class PatchAmaranthBossDeadCoroutine : Attribute {}


    internal static partial class MonoModRules 
    {
        public static void PatchAmaranthBossDeadCoroutine(MethodDefinition method, CustomAttribute attrib) 
        {
            MethodDefinition complete = method.GetEnumeratorMoveNext();
            new ILContext(complete).Invoke(ctx => {
                var f__4this = ctx.Method.DeclaringType.FindField("<>4__this");
                var hasPlats = f__4this.FieldType.Resolve().FindMethod("System.Boolean hasPlats(System.Collections.Generic.List`1<TowerFall.Platform>)");
                var cursor = new ILCursor(ctx);
                var label = ctx.DefineLabel();
                FieldReference val = null;

                cursor.GotoNext(MoveType.After, 
                    instr => instr.MatchCallOrCallvirt("Monocle.Layer", "GetList"),
                    instr => instr.MatchStfld(out val)
                );

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f__4this);
                cursor.Emit(OpCodes.Ldloc_3);
                cursor.Emit(OpCodes.Ldfld, val);
                cursor.Emit(OpCodes.Callvirt, hasPlats);
                cursor.Emit(OpCodes.Brfalse_S, label);

                cursor.GotoNext(MoveType.After, 
                    instr => instr.MatchCallOrCallvirt("Monocle.Entity", "Add"),
                    instr => instr.MatchPop());
                cursor.MarkLabel(label);
            });
        }
    }
}
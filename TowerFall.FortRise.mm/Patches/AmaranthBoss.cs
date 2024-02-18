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

        private bool hasPlats(List<RotatePlatform> plats) => plats.Count != 0;
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
                var hasPlats = f__4this.FieldType.Resolve().FindMethod("System.Boolean hasPlats(System.Collections.Generic.List`1<TowerFall.RotatePlatform>)");
                var cursor = new ILCursor(ctx);
                var label = ctx.DefineLabel();
                FieldReference val = null;

                cursor.GotoNext(MoveType.After, 
                    instr => instr.MatchCallOrCallvirt("Monocle.Layer", "GetList"),
                    instr => instr.MatchStfld(out val)
                );
                OpCode opCode;
                if (IsWindows) 
                {
                    if (Version <= new Version(1, 3, 3, 1)) 
                    {
                        opCode = OpCodes.Ldloc_2;
                    }
                    else 
                    {
                        opCode = OpCodes.Ldloc_3;
                    }
                }
                else 
                {
                    opCode = OpCodes.Ldloc_1;
                }

                // (this)
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f__4this);
                // (, plats)
                cursor.Emit(opCode);
                cursor.Emit(OpCodes.Ldfld, val);
                // hasPlats(this, plats)
                cursor.Emit(OpCodes.Callvirt, hasPlats);
                // if (hasPlats(this, plats))
                cursor.Emit(OpCodes.Brfalse_S, label);

                cursor.GotoNext(MoveType.After, 
                    instr => instr.MatchCallOrCallvirt("Monocle.Entity", "Add"),
                    instr => instr.MatchPop());
                cursor.MarkLabel(label);
            });
        }
    }
}
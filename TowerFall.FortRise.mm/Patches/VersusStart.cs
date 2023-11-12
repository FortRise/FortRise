using System;
using System.Collections;
using FortRise;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall
{
    public class patch_VersusStart : VersusStart
    {
        private Session session;
        public patch_VersusStart(Session session) : base(session)
        {
        }

        [MonoModIgnore]
        [PatchVersusStartSessionIntroSequence]
        private extern IEnumerator SessionIntroSequence();

        private OutlineText CustomModeText(object displayField) 
        {
            var customGameMode = (session.MatchSettings as patch_MatchSettings).CurrentCustomGameMode;
            if (customGameMode == null)
                return null;
            var outlineText = new OutlineText(TFGame.Font, customGameMode.Name.ToUpperInvariant());
            outlineText.Color = Color.Transparent;
            outlineText.OutlineColor = Color.Transparent;
            displayField.DynSetData("modeColor", customGameMode.NameColor);
            Add(outlineText);
            return outlineText;
        }
    }
}


namespace MonoMod
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchVersusStartSessionIntroSequence))]
    internal class PatchVersusStartSessionIntroSequence : Attribute {}
    internal static partial class MonoModRules 
    {
        public static void PatchVersusStartSessionIntroSequence(MethodDefinition method, CustomAttribute attrib) 
        {
            var moveNext = method.GetEnumeratorMoveNext();
            new ILContext(moveNext).Invoke(ctx => {
                var localsName = IsWindows ? "<>8__1" : "CS$<>8__locals1";
                var this8__1 = ctx.Method.DeclaringType.FindField(localsName);
                var this__4 = ctx.Method.DeclaringType.FindField("<>4__this");
                var CustomModeText = this__4.FieldType.Resolve().FindMethod("Monocle.OutlineText CustomModeText(System.Object)");
                var cursor = new ILCursor(ctx);

                cursor.GotoNext(instr => instr.MatchLdnull());
                cursor.GotoNext(instr => instr.MatchLdnull());

                // Let's replace null with 'this'
                cursor.Next.OpCode = OpCodes.Ldarg_0;
                cursor.GotoNext();
                cursor.Emit(OpCodes.Ldfld, this__4);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, this8__1);
                cursor.Emit(OpCodes.Call, CustomModeText);
            });
        }
    }
}
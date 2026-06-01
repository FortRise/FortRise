using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall.Patching
{
    [MonoModPatch("TowerFall.VersusPlayerMatchResults")]
    public class VersusPlayerMatchResults : TowerFall.VersusPlayerMatchResults
    {
        public VersusPlayerMatchResults(Session session, VersusMatchResults matchResults, int playerIndex, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards) : base(session, matchResults, playerIndex, tweenFrom, tweenTo, awards)
        {
        }

        [MonoModIgnore]
        [MonoModConstructor]
        [PatchVersusPlayerMatchResultsCtor]
        public extern void ctor(Session session, VersusMatchResults matchResults, int playerIndex, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards);

        [MonoModIgnore]
        [PatchVersusPlayerMatchResultsSequence]
        private extern IEnumerator Sequence();
    }
}

namespace MonoMod
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchVersusPlayerMatchResultsCtor))]
    internal class PatchVersusPlayerMatchResultsCtor : Attribute;

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchVersusPlayerMatchResultsSequence))]
    internal class PatchVersusPlayerMatchResultsSequence : Attribute;

    internal static partial class MonoModRules
    {
        public static void PatchVersusPlayerMatchResultsSequence(MethodDefinition method, CustomAttribute attribute)
        {
            MethodDefinition seq = method.GetEnumeratorMoveNext();

            new ILContext(seq).Invoke(ctx =>
            {
                var cursor = new ILCursor(ctx);

                while (cursor.TryGotoNext(
                        instr => instr.MatchLdarg0(),
                        instr => instr.MatchLdfld(out _),
                        instr => instr.MatchLdfld("TowerFall.VersusPlayerMatchResults", "portrait"),
                        instr => instr.MatchCallOrCallvirt("Monocle.GraphicsComponent", "get_Height")))
                {
                    cursor.RemoveRange(4);

                    cursor.Emit(OpCodes.Ldc_R4, (float)50);
                }
            });
        }

        public static void PatchVersusPlayerMatchResultsCtor(ILContext ctx, CustomAttribute attrib)
        {
            var cursor = new ILCursor(ctx);

            cursor.GotoNext( 
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld("TowerFall.VersusPlayerMatchResults", "portrait"),
                instr => instr.MatchCallOrCallvirt("Monocle.GraphicsComponent", "get_Height")
            );

            cursor.RemoveRange(3);

            cursor.Emit(OpCodes.Ldc_R4, (float)50);

            cursor.GotoNext( 
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld("TowerFall.VersusPlayerMatchResults", "portrait"),
                instr => instr.MatchCallOrCallvirt("Monocle.GraphicsComponent", "get_Width")
            );

            cursor.RemoveRange(3);

            cursor.Emit(OpCodes.Ldc_R4, (float)50);

            cursor.GotoNext( 
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld("TowerFall.VersusPlayerMatchResults", "portrait"),
                instr => instr.MatchCallOrCallvirt("Monocle.GraphicsComponent", "get_Height")
            );

            cursor.RemoveRange(3);

            cursor.Emit(OpCodes.Ldc_R4, (float)50);

            cursor.GotoNext( 
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld("TowerFall.VersusPlayerMatchResults", "portrait"),
                instr => instr.MatchCallOrCallvirt("Monocle.GraphicsComponent", "get_Width")
            );

            cursor.RemoveRange(3);

            cursor.Emit(OpCodes.Ldc_R4, (float)50);

            cursor.GotoNext( 
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld("TowerFall.VersusPlayerMatchResults", "portrait"),
                instr => instr.MatchCallOrCallvirt("Monocle.GraphicsComponent", "get_Height")
            );

            cursor.RemoveRange(3);

            cursor.Emit(OpCodes.Ldc_R4, (float)50);
        }
    }
}
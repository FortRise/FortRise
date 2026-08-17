using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;

namespace TowerFall.Patching
{
    [MonoModPatch("TowerFall.DarkWorldPlayerResults")]
    public class DarkWorldPlayerResults : TowerFall.DarkWorldPlayerResults
    {
        public DarkWorldPlayerResults(DarkWorldSessionState stats, int playerIndex, Vector2 from, Vector2 to) : base(stats, playerIndex, from, to)
        {
        }

        [MonoModIgnore]
        [MonoModConstructor]
        [PatchDarkWorldPlayerResultsCtor]
        public extern void ctor(DarkWorldSessionState stats, int playerIndex, Vector2 from, Vector2 to);

        [MonoModIgnore]
        [PatchDarkWorldPlayerResultsSequence]
        public extern IEnumerator Sequence();
    }
}


namespace MonoMod
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldPlayerResultsCtor))]
    internal class PatchDarkWorldPlayerResultsCtor : Attribute;

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldPlayerResultsSequence))]
    internal class PatchDarkWorldPlayerResultsSequence : Attribute;

    internal static partial class MonoModRules
    {
        public static void PatchDarkWorldPlayerResultsSequence(MethodDefinition method, CustomAttribute attribute)
        {
            MethodDefinition seq = method.GetEnumeratorMoveNext();

            new ILContext(seq).Invoke(ctx =>
            {
                var cursor = new ILCursor(ctx);

                while (cursor.TryGotoNext(
                        instr => instr.MatchLdarg0(),
                        instr => instr.MatchLdfld(out _),
                        instr => instr.MatchLdfld("TowerFall.DarkWorldPlayerResults", "portrait"),
                        instr => instr.MatchCallOrCallvirt("Monocle.GraphicsComponent", "get_Height")))
                {
                    cursor.RemoveRange(4);

                    cursor.Emit(OpCodes.Ldc_R4, (float)50);
                }
            });
        }

        public static void PatchDarkWorldPlayerResultsCtor(ILContext ctx, CustomAttribute attrib)
        {
            var cursor = new ILCursor(ctx);

            cursor.GotoNext( 
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld("TowerFall.DarkWorldPlayerResults", "portrait"),
                instr => instr.MatchCallOrCallvirt("Monocle.GraphicsComponent", "get_Height")
            );

            cursor.RemoveRange(3);

            cursor.Emit(OpCodes.Ldc_R4, (float)50);
        }
    }
}

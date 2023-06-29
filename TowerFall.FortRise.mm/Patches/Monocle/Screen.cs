using System;
using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Monocle 
{
    public class patch_Screen : Screen
    {
        public patch_Screen(Engine engine, int width, int height, float scale) : base(engine, width, height, scale)
        {
        }

        [MonoModIgnore]
        [PatchScreenResize]
        public extern void Resize(int width, int height, float scale);
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchScreenResize))]
    internal class PatchScreenResize : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchScreenResize(ILContext ctx, CustomAttribute attrib) 
        {
            var height = ctx.Method.DeclaringType.FindField("height");
            var cursor = new ILCursor(ctx);

            cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("Monocle.Screen", "width"));
            if (cursor.TryGotoNext(instr => instr.MatchStfld("Monocle.Screen", "width"))) 
            {
                cursor.Next.Operand = height;
            }
        }
    }
}
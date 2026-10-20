using System;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;

namespace TowerFall.Patching
{
    [MonoModPatch("TowerFall.DefaultHat")]
    public class DefaultHat : TowerFall.DefaultHat
    {
        public DefaultHat(ArcherData archerData, Allegiance allegiance, Vector2 position, bool flipped, Arrow arrow, int ownerIndex) : base(archerData, allegiance, position, flipped, arrow, ownerIndex)
        {
        }

        [MonoModIgnore]
        [PatchDefaultHatUpdate]
        public extern override void Update();
    }
}


namespace MonoMod
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDefaultHatUpdate))]
    public class PatchDefaultHatUpdate : Attribute {}


    internal static partial class MonoModRules
    {
        public static void PatchDefaultHatUpdate(ILContext ctx, CustomAttribute attrib)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(instr => instr.MatchLdcR4(-0.08726647f)))
            {
                cursor.Next.Operand = -0.08726646f;
            }

            if (cursor.TryGotoNext(instr => instr.MatchLdcR4(0.08726647f)))
            {
                cursor.Next.Operand = 0.08726646f;
            }
        }
    }
}


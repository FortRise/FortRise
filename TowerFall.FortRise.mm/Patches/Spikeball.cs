using System;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;

namespace TowerFall.Patching 
{
    [MonoModPatch("TowerFall.Spikeball")]
    public class SpikeBall : TowerFall.Spikeball
    {
        public SpikeBall(Vector2 position, Vector2 pivot, bool exploding) : base(position, pivot, exploding)
        {
        }

        [MonoModIgnore]
        [PatchSpikeballUpdate]
        public extern override void Update();
    }
}


namespace MonoMod
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchSpikeballUpdate))]
    public class PatchSpikeballUpdate : Attribute {}


    internal static partial class MonoModRules 
    {
        public static void PatchSpikeballUpdate(ILContext ctx, CustomAttribute attrib) 
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(instr => instr.MatchLdcR4(0.00555555569f)))
            {
                cursor.Next.Operand = 0.005555555f;
            }
        }
    }
}


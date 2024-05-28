using System;
using FortRise;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public class patch_Player : Player
    {
        private WrapHitbox shieldHitbox;

        public patch_Player(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, HatStates hatState, bool frozen, bool flash, bool indicator) : base(playerIndex, position, allegiance, teamColor, inventory, hatState, frozen, flash, indicator)
        {
        }

        public extern void orig_Added();

        public override void Added()
        {
            orig_Added();
            RiseCore.Events.Player.Invoke_OnSpawn(this, PlayerIndex);
        }

        public extern PlayerCorpse orig_Die(DeathCause deathCause, int killerIndex, bool brambled = false, bool laser = false);

        public PlayerCorpse Die(DeathCause deathCause, int killerIndex, bool brambled = false, bool laser = false) 
        {
            PlayerCorpse corpsed = Die(deathCause, killerIndex, brambled, laser);
            RiseCore.Events.Player.Invoke_OnPlayerDie(this, PlayerIndex, deathCause, killerIndex, brambled, laser);
            return corpsed;
        }

        [MonoModIgnore]
        [PatchPlayerDebugRender]
        public extern override void DebugRender();
        

        internal void Internal_DebugRender()
        {
            if (!HasShield)
                return;
            
            Collider = shieldHitbox;
            shieldHitbox.Render(Color.Purple);
        }
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchPlayerDebugRender))]
    public class PatchPlayerDebugRender : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchPlayerDebugRender(ILContext ctx, CustomAttribute attrib) 
        {
            // if (!ctx.Module.TryGetTypeReference("Microsoft.Xna.Framework.Color", out TypeReference typed)) 
            // {
            //     Console.WriteLine("Failed!");
            //     return;
            // }
            // var XNAColor = ctx.Module.ImportReference(typed).Resolve();
            // var m_get_Purple_Unimported = XNAColor.FindMethod("Microsoft.Xna.Framework.Color get_Purple()");
            // var m_get_Purple = ctx.Module.ImportReference(m_get_Purple_Unimported);

            var Player = ctx.Module.GetType("TowerFall.Player");

            var f_shieldHitbox = Player.FindField("shieldHitbox");
            var m_get_HasShield = Player.FindMethod("System.Boolean get_HasShield()");
            var m_Internal_DebugRender = Player.FindMethod("System.Void Internal_DebugRender()");

            var Entity = ctx.Module.GetType("Monocle.Entity");
            var m_set_Collider = Entity.FindMethod("System.Void set_Collider(Monocle.Collider)");

            var Collider = ctx.Module.GetType("Monocle.Collider");
            var m_Render = Collider.FindMethod("System.Void Render(Microsoft.Xna.Framework.Color)");

            var cursor = new ILCursor(ctx);

            var label = ctx.DefineLabel();

            cursor.GotoNext(MoveType.After, 
                instr => instr.MatchCall("Microsoft.Xna.Framework.Color", "Microsoft.Xna.Framework.Color get_Lime()"),
                instr => instr.MatchCallvirt("Monocle.Collider", "System.Void Render(Microsoft.Xna.Framework.Color)"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, m_Internal_DebugRender);

            // TODO Used this as an alternative for method calls.
            // We cannot do it right now, as MonoMod Relinker won't relink types that I've recently imported here.

            // if (HasShield)
            // cursor.Emit(OpCodes.Ldarg_0);
            // cursor.Emit(OpCodes.Callvirt, m_get_HasShield);
            // cursor.Emit(OpCodes.Brfalse_S, label);

            // // base.Collider = this.shieldHitbox
            // cursor.Emit(OpCodes.Ldarg_0);
            // cursor.Emit(OpCodes.Ldarg_0);
            // cursor.Emit(OpCodes.Ldfld, f_shieldHitbox);
            // cursor.Emit(OpCodes.Call, m_set_Collider);

            // // this.shieldHitbox.Render(Color.Purple)
            // cursor.Emit(OpCodes.Ldarg_0);
            // cursor.Emit(OpCodes.Ldfld, f_shieldHitbox);
            // cursor.Emit(OpCodes.Call, m_get_Purple);
            // cursor.Emit(OpCodes.Callvirt, m_Render);

            // // Marked: base.Collider = collider
            // cursor.MarkLabel(label);
        }
    }
}
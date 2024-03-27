using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public class patch_TrialsStart : TrialsStart
    {
        public patch_TrialsStart(Session session) : base(session)
        {
        }

        [MonoModIgnore]
        [PatchTrialsStartSequence]
        private extern IEnumerator Sequence();
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchTrialsStartSequence))]
    public class PatchTrialsStartSequence : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchTrialsStartSequence(MethodDefinition method, CustomAttribute attrib) 
        {
            MethodDefinition complete = method.GetEnumeratorMoveNext();

            new ILContext(complete).Invoke(ctx => {
                var op_Equality = ctx.Module.ImportReference(ctx.Module.TypeSystem.String.Resolve().FindMethod("System.Boolean op_Equality(System.String,System.String)"));

                var f__4this = ctx.Method.DeclaringType.FindField("<>4__this");
                var HUD = ctx.Module.GetType("TowerFall.HUD");
                var get_Level = HUD.FindMethod("TowerFall.Level get_Level()");
                var Level = ctx.Module.GetType("TowerFall.Level");
                var get_Session = Level.FindMethod("TowerFall.Session get_Session()");
                var Session = ctx.Module.GetType("TowerFall.Session");
                var GetLevelSet = ctx.Module.GetType("TowerFall.SessionExt").FindMethod("System.String GetLevelSet(TowerFall.Session)");

                var MapButton = ctx.Module.GetType("TowerFall.MapButton");
                var InitAdventureTrialsStartLevelGraphics = MapButton.FindMethod("Monocle.Image[] InitAdventureTrialsStartLevelGraphics(Microsoft.Xna.Framework.Point,System.String)");

                var cursor = new ILCursor(ctx);

                cursor.GotoNext(MoveType.Before, instr => instr.MatchCallOrCallvirt("TowerFall.MapButton", "InitTrialsStartLevelGraphics"));
                cursor.Next.Operand = InitAdventureTrialsStartLevelGraphics;

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f__4this);
                cursor.Emit(OpCodes.Callvirt, get_Level);
                cursor.Emit(OpCodes.Callvirt, get_Session);
                cursor.Emit(OpCodes.Call, GetLevelSet);
            });
        }
    }
}
using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public partial class patch_DarkWorldComplete : DarkWorldComplete
    {
        public patch_DarkWorldComplete(Session session) : base(session)
        {
        }

        // The most error prone patch
        [MonoModIgnore]
        [PatchDarkWorldCompleteSequence]
        private extern IEnumerator Sequence();
    }
}

namespace MonoMod 
{

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldCompleteSequence))]
    internal class PatchDarkWorldCompleteSequence : Attribute {}

    internal static partial class MonoModRules 
    {

        public static void PatchDarkWorldCompleteSequence(MethodDefinition method, CustomAttribute attribute) 
        {
            MethodDefinition complete = method.GetEnumeratorMoveNext();

            new ILContext(complete).Invoke(ctx => {
                var eventHook = ctx.Module.GetType("FortRise.RiseCore/Events");
                var invoked = eventHook.FindMethod("System.Void InvokeDarkWorldComplete_Result(System.Int32,TowerFall.DarkWorldDifficulties,System.Int32,System.Int64,System.Int32,System.Int32,System.Int32,System.String)");
                var SaveData = ctx.Module.GetType("TowerFall", "SaveData");
                var deaths = IsWindows 
                    ? complete.DeclaringType.FindField("<deaths>5__2") 
                    : complete.DeclaringType.FindField("<deaths>5__1b");
                var this_4 = complete.DeclaringType.FindField("<>4__this");

                var session = method.DeclaringType.FindField("session");
                var matchSettings = session.FieldType.Resolve().FindField("MatchSettings");
                var levelSet = session.FieldType.Resolve().FindField("LevelSet");

                var darkWorldState = session.FieldType.Resolve().FindField("DarkWorldState");
                var time = darkWorldState.FieldType.Resolve().FindField("Time");
                var continues = darkWorldState.FieldType.Resolve().FindField("Continues");

                var Variants = matchSettings.FieldType.Resolve().FindField("Variants");
                var GetCoopCurses = Variants.FieldType.Resolve().FindMethod("System.Int32 GetCoOpCurses()");
                var DarkWorldDifficulty = matchSettings.FieldType.Resolve().FindField("DarkWorldDifficulty");
                var TFGame_PlayerAmount = ctx.Module.GetType("TowerFall.TFGame").FindMethod("System.Int32 get_PlayerAmount()");

                var levelSystem = matchSettings.FieldType.Resolve().FindField("LevelSystem");
                var get_ID = levelSystem.FieldType.Resolve().FindMethod("Microsoft.Xna.Framework.Point get_ID()");
                var X = ctx.Module.ImportReference(get_ID.ReturnType.Resolve().FindField("X"));

                var loc_matchSettings = new VariableDefinition(matchSettings.FieldType);
                var loc_darkworldstate = new VariableDefinition(ctx.Module.GetType("TowerFall.DarkWorldSessionState"));
                var loc_LevelSet = new VariableDefinition(ctx.Module.TypeSystem.String);
                ctx.Body.Variables.Add(loc_matchSettings);
                ctx.Body.Variables.Add(loc_darkworldstate);
                ctx.Body.Variables.Add(loc_LevelSet);

                var cursor = new ILCursor(ctx);

                cursor.GotoNext(instr => instr.MatchLdsfld("TowerFall.SaveData", "Instance"));
                // This part of instructions will replace one method call from the DarkWorldTowerStats into a hook
                
                // Check for TF Version since it does have a different instructions
                // Please confirm if you have an issue with 1.3.3.2, I will fix this later on

                // Linux or maybe MacOS has different instructions
                int instrNumToRemove = !IsWindows ? 41 : Version switch {
                    { Major: 1, Minor: 3, Build: 3, Revision: 3} => 31,
                    _ => 36
                };

                cursor.RemoveRange(instrNumToRemove);

                /* matchSettings */
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, this_4);
                cursor.Emit(OpCodes.Ldfld, session);
                cursor.Emit(OpCodes.Ldfld, matchSettings);
                cursor.Emit(OpCodes.Stloc_S, loc_matchSettings);

                /* darkWorldState */
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, this_4);
                cursor.Emit(OpCodes.Ldfld, session);
                cursor.Emit(OpCodes.Ldfld, darkWorldState);
                cursor.Emit(OpCodes.Stloc_S, loc_darkworldstate);

                /* levelSet */
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, this_4);
                cursor.Emit(OpCodes.Ldfld, session);
                cursor.Emit(OpCodes.Ldfld, levelSet);
                cursor.Emit(OpCodes.Stloc_S, loc_LevelSet);

                /* Emit necessary code to call the InvokeDarkWorldComplete_Result hook */
                cursor.Emit(OpCodes.Ldloc_S, loc_matchSettings);
                cursor.Emit(OpCodes.Ldfld, levelSystem);
                cursor.Emit(OpCodes.Callvirt, get_ID);
                cursor.Emit(OpCodes.Ldfld, X);
                cursor.Emit(OpCodes.Ldloc_S, loc_matchSettings);
                cursor.Emit(OpCodes.Ldfld, DarkWorldDifficulty);
                cursor.Emit(OpCodes.Call, TFGame_PlayerAmount);
                cursor.Emit(OpCodes.Ldloc_S, loc_darkworldstate);
                cursor.Emit(OpCodes.Ldfld, time);
                cursor.Emit(OpCodes.Ldloc_S, loc_darkworldstate);
                cursor.Emit(OpCodes.Ldfld, continues);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, deaths);
                cursor.Emit(OpCodes.Ldloc_S, loc_matchSettings);
                cursor.Emit(OpCodes.Ldfld, Variants);
                cursor.Emit(OpCodes.Callvirt, GetCoopCurses);
                cursor.Emit(OpCodes.Ldloc_S, loc_LevelSet);
                cursor.Emit(OpCodes.Call, invoked);
            });
        }
    }
}

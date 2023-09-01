using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using FortRise;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public class patch_QuestControl : QuestControl 
    {
        [MonoModLinkTo("TowerFall.HUD", "System.Void Added()")]
        [MonoModIgnore]
        public void base_Added() 
        {
            base.Added();
        }
        public override void Added()
        {
            base_Added();
            LoadSpawns();
            XmlDocument xmlDocument;
            if (Level.Session.IsOfficialLevelSet()) 
            {
                xmlDocument = Calc.LoadXML((base.Level.Session.MatchSettings.LevelSystem as QuestLevelSystem).QuestTowerData.DataPath);
            }
            else 
            {
                var path = (base.Level.Session.MatchSettings.LevelSystem as QuestLevelSystem).QuestTowerData.DataPath;
                using var xmlStream = RiseCore.ResourceTree.TreeMap[path].Stream;
                xmlDocument = patch_Calc.LoadXML(xmlStream);
            }
            Gauntlet = xmlDocument["data"].AttrBool("gauntlet", false);
            if (Gauntlet)
            {
                LoadGauntlet(xmlDocument);
                return;
            }
            LoadWaves(xmlDocument);
        }

        [MonoModIgnore]
        private extern void LoadSpawns();

        [MonoModIgnore]
        private extern void LoadGauntlet(XmlDocument doc);

        [MonoModIgnore]
        private extern void LoadWaves(XmlDocument doc);

        [MonoModIgnore]
        [PatchQuestControlStartSequence]
        private extern IEnumerator StartSequence();

        [MonoModIgnore]
        [PatchQuestControlSpawnWave]
        private extern IEnumerator SpawnWave(int waveNum, List<IEnumerator> groups, int[] floors, bool dark, bool slow, bool scroll);
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestControlStartSequence))]
    public class PatchQuestControlStartSequence : Attribute {}

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestControlSpawnWave))]
    public class PatchQuestControlSpawnWave : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchQuestControlSpawnWave(MethodDefinition method, CustomAttribute attrib) 
        {
            MethodDefinition complete = method.GetEnumeratorMoveNext();

            new ILContext(complete).Invoke(ctx => {
                var OnQuestSpawnWave = ctx.Module.GetType("FortRise.RiseCore/Events").FindMethod(
                    "System.Void Invoke_OnQuestSpawnWave(TowerFall.QuestControl,System.Int32,System.Collections.Generic.List`1<System.Collections.IEnumerator>,System.Int32[],System.Boolean,System.Boolean,System.Boolean)");
                var f__4this = ctx.Method.DeclaringType.FindField("<>4__this");
                var waveNum = ctx.Method.DeclaringType.FindField("waveNum");
                var scroll = ctx.Method.DeclaringType.FindField("scroll");
                var dark = ctx.Method.DeclaringType.FindField("dark");
                var slow = ctx.Method.DeclaringType.FindField("slow");
                var floors = ctx.Method.DeclaringType.FindField("floors");
                var groups = ctx.Method.DeclaringType.FindField("groups");

                var cursor = new ILCursor(ctx);
                cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.QuestRoundLogic", "CurrentWave"));

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f__4this);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, waveNum);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, groups);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, floors);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, dark);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, slow);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, scroll);
                cursor.Emit(OpCodes.Call, OnQuestSpawnWave);
            });
        }

        public static void PatchQuestControlStartSequence(MethodDefinition method, CustomAttribute attrib) 
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
                var InitQuestStartLevelGraphics = MapButton.FindMethod("Monocle.Image[] InitQuestStartLevelGraphics(System.Int32,System.String)");

                var cursor = new ILCursor(ctx);
                ILLabel label = null;

                // It's BrTrue in Windows while Br in Linux or MacOS
                Func<Instruction, bool>[] brFalseOrTrue;
                if (IsWindows)
                    brFalseOrTrue = new Func<Instruction, bool>[]{ 
                        instr => instr.MatchLdfld("TowerFall.MatchSettings", "QuestHardcoreMode"),
                        instr => instr.MatchBrtrue(out label),
                    };
                else
                    brFalseOrTrue = new Func<Instruction, bool>[]{ 
                        instr => instr.MatchLdfld("TowerFall.MatchSettings", "QuestHardcoreMode"),
                        instr => instr.MatchBr(out _),
                        instr => instr.MatchLdcI4(1),
                        instr => instr.MatchNop(),
                        instr => instr.MatchStloc(2),
                        instr => instr.MatchLdloc(2),
                        instr => instr.MatchBrtrue(out label),
                    };

                cursor.GotoNext(MoveType.After, brFalseOrTrue);

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f__4this);
                cursor.Emit(OpCodes.Callvirt, get_Level);
                cursor.Emit(OpCodes.Callvirt, get_Session);
                cursor.Emit(OpCodes.Call, GetLevelSet);
                cursor.Emit(OpCodes.Ldstr, "TowerFall");
                cursor.Emit(OpCodes.Call, op_Equality);
                cursor.Emit(OpCodes.Brfalse_S, label);

                cursor.GotoNext(MoveType.Before, instr => instr.MatchCallOrCallvirt("TowerFall.MapButton", "InitQuestStartLevelGraphics"));
                cursor.Next.Operand = InitQuestStartLevelGraphics;

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f__4this);
                cursor.Emit(OpCodes.Callvirt, get_Level);
                cursor.Emit(OpCodes.Callvirt, get_Session);
                cursor.Emit(OpCodes.Call, GetLevelSet);
            });
        }
    }
}
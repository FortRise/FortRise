using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
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
    public class patch_QuestControl : QuestControl 
    {
        private Dictionary<string, QuestSpawnPortal> spawns;
        private List<Vector2> chestSpawns;
        private List<Vector2> bigChestSpawns;
        private List<IEnumerator> waves;
        private HashSet<string> activeEvents;

        public extern void orig_ctor();

        [MonoModConstructor]
        public void ctor() 
        {
            activeEvents = new HashSet<string>();
            orig_ctor();
        }


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

        [MonoModReplace]
        private void LoadWaves(XmlDocument doc) 
        {
            QuestRoundLogic questRoundLogic = Level.Session.RoundLogic as QuestRoundLogic;
            waves = new List<IEnumerator>();
            int waveNum = 0;
            string difficultyName = Level.Session.MatchSettings.QuestHardcoreMode ? "hardcore" : "normal";

            XmlNodeList waveList = doc["data"][difficultyName].GetElementsByTagName("wave");
            int finalWave = (questRoundLogic.TotalWaves = waveList.Count);

            foreach (XmlElement wave in waveList)
            {
                if (waveNum >= Level.Session.QuestTestWave)
                {
                    List<IEnumerator> sequences = new List<IEnumerator>();
                    foreach (XmlElement sequence in wave)
                    {
                        if (TFGame.PlayerAmount != 2 && sequence.AttrBool("coop", false) || TFGame.PlayerAmount != 1 && sequence.AttrBool("solo", false))
                        {
                            continue;
                        }
                        if (sequence.Name == "group")
                        {

                            sequences.Add(SpawnGroup(
                                Calc.ReadCSV(sequence.ChildText("spawns", "")),
                                sequence.ChildText("enemies", ""),
                                Calc.ReadCSV(sequence.ChildText("treasure", "")),
                                Calc.ReadCSV(sequence.ChildText("bigTreasure", "")),
                                sequence.ChildInt("reaper", -1),
                                sequence.ChildBool("jester", false),
                                sequence.AttrInt("delay", 0),
                                waveNum >= finalWave - 1)
                            );
                        }
                        else if (sequence.Name == "event")
                        {
                            sequences.Add(SpawnEvent(Calc.ReadCSV(sequence.InnerText)));
                        }
                        else if (sequence.Name == "event_appear")
                        {
                            sequences.Add(SpawnEvent(Calc.ReadCSV(sequence.InnerText)));
                        }
                        else if (sequence.Name == "event_disappear")
                        {
                            sequences.Add(DespawnEvents(Calc.ReadCSV(sequence.InnerText)));
                        }
                    }
                    int[] floors = null;
                    if (wave.HasChild("floors"))
                    {
                        floors = Calc.ReadCSVInt(wave.ChildText("floors"));
                    }
                    waves.Add(SpawnWave(
                        waveNum - base.Level.Session.QuestTestWave, 
                        sequences, floors, 
                        wave.AttrBool("dark", false) || Level.Session.MatchSettings.Variants.AlwaysDark, 
                        wave.AttrBool("slow", false) || Level.Session.MatchSettings.Variants.SlowTime, 
                        wave.AttrBool("scroll", false) || Level.Session.MatchSettings.Variants.AlwaysScrolling)
                    );
                }
                waveNum++;
            }
        }

        [MonoModReplace]
        private void LoadGauntlet(XmlDocument doc)
        {
            Random random = new Random();
            List<string> portals = new List<string>();

            foreach (var spawn in spawns)
            {
                portals.Add(spawn.Key);
            }

            QuestRoundLogic questRoundLogic = Level.Session.RoundLogic as QuestRoundLogic;
            string difficultyName = Level.Session.MatchSettings.QuestHardcoreMode ? "hardcore" : "normal";

            XmlElement sequences = doc["data"][difficultyName];
            int enemyCount = 0;
            waves = new List<IEnumerator>();
            chestSpawns.Shuffle(random);
            bigChestSpawns.Shuffle(random);

            foreach (XmlElement sequence in sequences)
            {
                switch (sequence.Name)
                {
                case "group":
                    portals.Shuffle(random);
                    List<string> enemies = ReadEnemies(sequence.InnerText);
                    enemies.Shuffle(random);
                    enemyCount += enemies.Count;
                    int max = sequence.AttrInt("max", 2);
                    bool dark = sequence.AttrBool("dark", false);
                    waves.Add(GauntletEnemiesSequence(portals.ToArray(), enemies.ToArray(), max, dark));
                    break;
                case "bigTreasure":
                    Pickups[] pickups = Calc.StringsToEnums<Pickups>(Calc.ReadCSV(sequence.InnerText));
                    waves.Add(GauntletBigTreasureSequence(bigChestSpawns[0], pickups));
                    bigChestSpawns.RemoveAt(0);
                    break;
                case "treasure":
                    Pickups pickup = Calc.StringToEnum<Pickups>(sequence.InnerText);
                    waves.Add(GauntletTreasureSequence(chestSpawns[0], pickup));
                    chestSpawns.RemoveAt(0);
                    break;
                case "event":
                case "event_appear":
                    waves.Add(SpawnEvent(Calc.ReadCSV(sequence.InnerText)));
                    break;
                case "event_disappear":
                    waves.Add(DespawnEvents(Calc.ReadCSV(sequence.InnerText)));
                    break;
                }
            }
            Level.Add(questRoundLogic.GauntletCounter = new QuestGauntletCounter(enemyCount));
        }

        private IEnumerator SpawnEvent(string[] events) 
        {
            for (int i = 0; i < events.Length; i++)
            {
                string eventName = events[i];
                if (QuestEventRegistry.Events.TryGetValue(eventName, out var evt))
                {
                    evt.Appear?.Invoke(Level);
                    activeEvents.Add(eventName);
                }
            }

            yield break;
        }

        private IEnumerator DespawnEvents(string[] events)
        {
            for (int i = 0; i < events.Length; i++)
            {
                string eventName = events[i];
                if (QuestEventRegistry.Events.TryGetValue(eventName, out var evt))
                {
                    evt.DieOut?.Invoke(Level);
                    activeEvents.Remove(eventName);
                }
            }

            yield break;
        }

        private void DespawnEvents() 
        {
            foreach (var evt in activeEvents)
            {
                QuestEventRegistry.Events[evt].DieOut?.Invoke(Level);
            }
            activeEvents.Clear();
        }

        [MonoModIgnore]
        private extern List<string> ReadEnemies(string enemies);

        [MonoModIgnore]
        private extern IEnumerator GauntletEnemiesSequence(string[] spawnNames, string[] enemies, int max, bool dark);

        [MonoModIgnore]
        private extern IEnumerator GauntletBigTreasureSequence(Vector2 spawnAt, Pickups[] pickups);
        
        [MonoModIgnore]
        private extern IEnumerator GauntletTreasureSequence(Vector2 spawnAt, Pickups pickup);

        [MonoModIgnore]
        private extern void LoadSpawns();

        [MonoModIgnore]
        private extern IEnumerator SpawnGroup(string[] spawns, string enemies, string[] treasure, string[] bigTreasure, int reaper, bool jester, int delay, bool finalWave);

        [MonoModIgnore]
        [PatchQuestControlStartSequence]
        private extern IEnumerator StartSequence();

        [MonoModIgnore]
        [PatchQuestControlSpawnWave]
        private extern IEnumerator SpawnWave(int waveNum, List<IEnumerator> groups, int[] floors, bool dark, bool slow, bool scroll);

        [MonoModIgnore]
        [PatchQuestControlLevelSequence]
        private extern IEnumerator LevelSequence();
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestControlStartSequence))]
    public class PatchQuestControlStartSequence : Attribute {}

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestControlSpawnWave))]
    public class PatchQuestControlSpawnWave : Attribute {}

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestControlLevelSequence))]
    public class PatchQuestControlLevelSequence : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchQuestControlSpawnWave(MethodDefinition method, CustomAttribute attrib) 
        {
            MethodDefinition complete = method.GetEnumeratorMoveNext();

            new ILContext(complete).Invoke(ctx => {
                var OnQuestSpawnWave = ctx.Module.GetType("FortRise.RiseCore/Events").FindMethod(
                    "System.Void Invoke_OnQuestSpawnWave(TowerFall.QuestControl,System.Int32,System.Collections.Generic.List`1<System.Collections.IEnumerator>,System.Int32[],System.Boolean,System.Boolean,System.Boolean)");

                var DespawnEvents = ctx.Module.GetType("TowerFall.QuestControl").FindMethod(
                    "System.Void DespawnEvents()");
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

                cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.QuestRoundLogic", "BetweenWaves"));
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f__4this);
                cursor.Emit(OpCodes.Call, DespawnEvents);
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
            });
        }

        public static void PatchQuestControlLevelSequence(MethodDefinition method, CustomAttribute attrib) 
        {
            MethodDefinition complete = method.GetEnumeratorMoveNext();

            new ILContext(complete).Invoke(ctx => {
                var DespawnEvents = ctx.Module.GetType("TowerFall.QuestControl").FindMethod(
                    "System.Void DespawnEvents()");

                var f__4this = ctx.Method.DeclaringType.FindField("<>4__this");

                var cursor = new ILCursor(ctx);

                cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.Level", "Ending"));

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f__4this);
                cursor.Emit(OpCodes.Call, DespawnEvents);
            });
        }
    }
}
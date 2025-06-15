using System;
using System.Collections.Generic;
using FortRise;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public class patch_Session : Session
    {
        private patch_DarkWorldSessionState DarkWorldState;
        public string LevelSet;
        [MonoModIgnore]
        public TreasureSpawner TreasureSpawner { get; private set; }
        private List<Variant> temporarilyActivatedVariants;

        public patch_Session(MatchSettings settings) : base(settings)
        {
        }


        [Prefix("System.Void .ctor(TowerFall.MatchSettings)")]
        private static void ctor_Prefix(patch_Session __instance)
        {
            __instance.temporarilyActivatedVariants = new();
        }

        public patch_MatchSettings MatchSettings;

        private void ActivateTempVariants(Level level, patch_DarkWorldTowerData.patch_LevelData levelData)
        {
            if (levelData.Variants == null)
            {
                return;
            }
            patch_MatchVariants matchVariant = (level.Session.MatchSettings.Variants as patch_MatchVariants);
            foreach (var variant in levelData.Variants)
            {
                var v = matchVariant.GetCustomVariant(variant);
                if (v.CoOpValue != 0)
                {
                    if (v.Value)
                    {
                        continue;
                    }
                    v.Value = true;
                    temporarilyActivatedVariants.Add(v);
                }
            }
        }

        public void DisableTempVariants(Level level)
        {
            foreach (var temp in temporarilyActivatedVariants)
            {
                temp.Value = false;
            }
            temporarilyActivatedVariants.Clear();
        }

        [Postfix(nameof(LevelLoadStart))]
        private void LevelLoadStart_Postfix(Level level)
        {
            DisableTempVariants(CurrentLevel);
            var matchSettings = MatchSettings;

            patch_DarkWorldLevelSystem darkWorld = matchSettings.LevelSystem as patch_DarkWorldLevelSystem;
            var levelData = darkWorld.GetLevelData(matchSettings.DarkWorldDifficulty, RoundIndex);
            ActivateTempVariants(CurrentLevel, levelData);

            var levelType = this.IsOfficialLevelSet() ? "vanilla" : "modded";
            level.AssignTag(levelType);
            var set = this.GetLevelSet();
            level.AssignTag("set=" + set);
            level.AssignTag("theme=" + ((patch_TowerTheme)(matchSettings.LevelSystem.Theme)).ID);
            var levelSystem = matchSettings.LevelSystem;
            switch (levelSystem) 
            {
            case DarkWorldLevelSystem dwSystem:
                level.AssignTag("level=" + dwSystem.DarkWorldTowerData.GetLevelID());
                level.AssignTag("darkworld");
                break;
            case TrialsLevelSystem lSystem:
                level.AssignTag("level=" + lSystem.TrialsLevelData.GetLevelID());
                level.AssignTag("trials");
                break;
            case QuestLevelSystem qSystem:
                level.AssignTag("level=" + qSystem.QuestTowerData.GetLevelID());
                level.AssignTag("quest");
                break;
            case VersusLevelSystem vSystem:
                level.AssignTag("level=" + vSystem.VersusTowerData.GetLevelID());
                level.AssignTag("versus");
                break;
            }
        }


        [MonoModPatch("StartGame")]
        [MonoModIfFlag("Steamworks")]
        public void StartGame_Steam() 
        {
			if (!MatchSettings.SoloMode)
			{
				GameStats stats = SaveData.Instance.Stats;
				int num = stats.MatchesPlayed;
				stats.MatchesPlayed = num + 1;
                if (this.IsOfficialLevelSet())
                    stats.VersusTowerPlays[MatchSettings.LevelSystem.ID.X] += 1UL;
				if (MatchSettings.RandomVersusTower)
				{
					num = stats.VersusRandomPlays;
					stats.VersusRandomPlays = num + 1;
				}
				else if (this.IsOfficialLevelSet())
				{
					stats.RegisterVersusTowerSelection(MatchSettings.LevelSystem.ID.X);
				}
				SessionStats.MatchesPlayed++;
                if (MatchSettings.IsCustom) 
                {
                    var gamemode = MatchSettings.CustomVersusGameMode;
                    if (gamemode != null)
                    {
                        gamemode.OnStartGame(this);
                    }
                }
			}
			if (MatchSettings.Mode == patch_Modes.DarkWorld)
			{
				DarkWorldState = new patch_DarkWorldSessionState(this);
                if (this.IsOfficialLevelSet())
                {
                    SaveData.Instance.DarkWorld.Towers[this.MatchSettings.LevelSystem.ID.X].Attempts += 1UL;
                }
                else 
                {
                    patch_DarkWorldTowerData adventureTower = (patch_DarkWorldTowerData)TowerRegistry.DarkWorldGet(this.GetLevelSet(), MatchSettings.LevelSystem.ID.X);
                    FortRiseModule.SaveData.AdventureWorld.AddOrGet(adventureTower.GetLevelID()).Attempts += 1;
                    if (adventureTower.StartingLives >= 0)
                    {
                        DarkWorldState.ExtraLives = adventureTower.StartingLives;
                    }
                }
			}
			TreasureSpawner = this.MatchSettings.LevelSystem.GetTreasureSpawner(this);
			Engine.Instance.Scene = new LevelLoaderXML(this);
        }

        [MonoModPatch("StartGame")]
        [MonoModIfFlag("NoLauncher")]
        public void StartGame_NoLauncher() 
        {
			if (!MatchSettings.SoloMode)
			{
                if (this.IsOfficialLevelSet()) 
                {
                    GameStats stats = SaveData.Instance.Stats;
                    stats.VersusTowerPlays[MatchSettings.LevelSystem.ID.X] += 1UL;
                }
				SessionStats.MatchesPlayed++;
                if (MatchSettings.IsCustom) 
                {
                    var gamemode = MatchSettings.CustomVersusGameMode;
                    if (gamemode != null)
                    {
                        gamemode.OnStartGame(this);
                    }
                }
			}
			if (MatchSettings.Mode == patch_Modes.DarkWorld)
			{
				DarkWorldState = new patch_DarkWorldSessionState(this);
                if (this.IsOfficialLevelSet())
                {
                    SaveData.Instance.DarkWorld.Towers[this.MatchSettings.LevelSystem.ID.X].Attempts += 1UL;
                }
                else 
                {
                    patch_DarkWorldTowerData adventureTower = (patch_DarkWorldTowerData)TowerRegistry.DarkWorldGet(this.GetLevelSet(), MatchSettings.LevelSystem.ID.X);
                    FortRiseModule.SaveData.AdventureWorld.AddOrGet(adventureTower.GetLevelID()).Attempts += 1;
                    if (adventureTower.StartingLives >= 0)
                    {
                        DarkWorldState.ExtraLives = adventureTower.StartingLives;
                    }
                }
			}
			TreasureSpawner = this.MatchSettings.LevelSystem.GetTreasureSpawner(this);
			Engine.Instance.Scene = new LevelLoaderXML(this);
        }

            /* Having some problems with this, might use this soon */
            // if (DarkWorldState != null) 
            // {
            //     var originalCount = DarkWorldState.defaultInventory.Arrows.Arrows.Count;
            //     var matchArrow = MatchSettings.Variants switch 
            //     {
            //         { StartWithBoltArrows: { Value: true } } => ArrowTypes.Bolt,
            //         { StartWithBombArrows: { Value: true } } => ArrowTypes.Bomb,
            //         { StartWithSuperBombArrows: { Value: true } } => ArrowTypes.SuperBomb,
            //         { StartWithBrambleArrows: { Value: true } } => ArrowTypes.Bramble,
            //         { StartWithDrillArrows: { Value: true } } => ArrowTypes.Drill,
            //         { StartWithFeatherArrows: { Value: true } } => ArrowTypes.Feather,
            //         { StartWithLaserArrows: { Value: true } } => ArrowTypes.Laser,
            //         { StartWithPrismArrows: { Value: true } } => ArrowTypes.Prism,
            //         { StartWithToyArrows: { Value: true } } => ArrowTypes.Toy,
            //         { StartWithTriggerArrows: { Value: true } } => ArrowTypes.Trigger,
            //         { StartWithRandomArrows: { Value: true } } => (ArrowTypes)Calc.Random.Next(0, 10),
            //         _ => ArrowTypes.Normal
            //     };
            //     DarkWorldState.defaultInventory.Arrows = new ArrowList(originalCount, matchArrow);
            // }
    }

    public static class SessionExt 
    {
        public static void SetLevelSet(this Session session, string levelSet) 
        {
            ((patch_Session)session).LevelSet = levelSet;
        }

        public static string GetLevelSet(this Session session) 
        {
            return ((patch_Session)session).LevelSet ?? "TowerFall";
        }

        public static bool IsOfficialLevelSet(this Session session) 
        {
            return ((patch_Session)session).GetLevelSet() == "TowerFall";
        }
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchSessionStartGame))]
    internal class PatchSessionStartGame : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchSessionStartGame(ILContext ctx, CustomAttribute attrib) 
        {
            var SaveData = ctx.Module.Assembly.MainModule.GetType("TowerFall", "SaveData");
            var AdventureActive = SaveData.FindField("AdventureActive");
            var cursor = new ILCursor(ctx);

            cursor.GotoNext(
                MoveType.After,
                instr => instr.MatchAdd(),
                instr => instr.MatchStfld("TowerFall.DarkWorldTowerStats", "Attempts")
            );
            var label = ctx.DefineLabel(cursor.Next);

            cursor.GotoPrev(MoveType.After, instr => instr.MatchStfld("TowerFall.Session", "DarkWorldState"));
            cursor.Emit(OpCodes.Ldsfld, AdventureActive);
            cursor.Emit(OpCodes.Brtrue_S, label);
        }
    }
}
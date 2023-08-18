using System;
using FortRise.Adventure;
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

        public patch_Session(MatchSettings settings) : base(settings)
        {
        }

        public patch_MatchSettings MatchSettings;


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
			}
			if (MatchSettings.Mode == patch_Modes.DarkWorld)
			{
				DarkWorldState = new patch_DarkWorldSessionState(this);
                if (this.IsOfficialLevelSet())
                    SaveData.Instance.DarkWorld.Towers[this.MatchSettings.LevelSystem.ID.X].Attempts += 1UL;
                else 
                {
                    TowerRegistry.DarkWorldGet(this.GetLevelSet(), MatchSettings.LevelSystem.ID.X).Stats.Attempts += 1;
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
			}
			if (MatchSettings.Mode == patch_Modes.DarkWorld)
			{
				DarkWorldState = new patch_DarkWorldSessionState(this);
                if (this.IsOfficialLevelSet())
                    SaveData.Instance.DarkWorld.Towers[this.MatchSettings.LevelSystem.ID.X].Attempts += 1UL;
                else 
                {
                    TowerRegistry.DarkWorldGet(this.GetLevelSet(), MatchSettings.LevelSystem.ID.X).Stats.Attempts += 1;
                }
			}
			TreasureSpawner = this.MatchSettings.LevelSystem.GetTreasureSpawner(this);
			Engine.Instance.Scene = new LevelLoaderXML(this);
        }

        // TODO Use these
        public void AddDeaths(int id) 
        {
            switch (MatchSettings.Mode) 
            {
            case patch_Modes.Custom:
                // patch_GameData.AdventureWorldTowers[id].Stats.Deaths += 1;
                break;
            case patch_Modes.DarkWorld:
                SaveData.Instance.DarkWorld.Towers[id].Deaths += 1;
                break;
            case patch_Modes.Quest:
                SaveData.Instance.Quest.Towers[id].TotalDeaths += 1;
                break;
            }
        }

        public void AddAttempts(int id) 
        {
            switch (MatchSettings.Mode) 
            {
            case patch_Modes.Custom:
                // patch_GameData.AdventureWorldTowers[id].Stats.Attempts += 1;
                break;
            case patch_Modes.DarkWorld:
                SaveData.Instance.DarkWorld.Towers[id].Attempts += 1;
                break;
            case patch_Modes.Quest:
                SaveData.Instance.Quest.Towers[id].TotalAttempts += 1;
                break;
            }
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
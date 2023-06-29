using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public class patch_Session : Session
    {
        private patch_DarkWorldSessionState DarkWorldState;
        public patch_Session(MatchSettings settings) : base(settings)
        {
        }

        public patch_MatchSettings MatchSettings;

        [PatchSessionStartGame]
        public extern void orig_StartGame();


        public void StartGame() 
        {
            orig_StartGame();
            if (patch_SaveData.AdventureActive) 
            {
                var worldTower = patch_GameData.AdventureWorldTowers[MatchSettings.LevelSystem.ID.X];
                worldTower.Stats.Attempts += 1;
                if (worldTower.StartingLives >= 0)
                    DarkWorldState.ExtraLives = worldTower.StartingLives;
            }
        }

        // TODO Use these
        public void AddDeaths(int id) 
        {
            switch (MatchSettings.Mode) 
            {
            case patch_Modes.Custom:
                patch_GameData.AdventureWorldTowers[id].Stats.Deaths += 1;
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
                patch_GameData.AdventureWorldTowers[id].Stats.Attempts += 1;
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
#pragma warning disable CS0626
#pragma warning disable CS0108
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_Session : Session
{
    private patch_DarkWorldSessionState DarkWorldState;
    public patch_Session(MatchSettings settings) : base(settings)
    {
    }

    [PatchSessionStartGame]
    public extern void orig_StartGame();

    public void StartGame() 
    {
        orig_StartGame();
        var worldTower = patch_GameData.AdventureWorldTowers[MatchSettings.LevelSystem.ID.X];
        worldTower.Stats.Attempts += 1;
    }

    [MonoModReplace]
    public void StartRound() 
    {
        patch_DarkWorldControl.DisableTempVariants(CurrentLevel);
        if (this.RoundLogic != null)
        {
            this.RoundLogic.OnRoundStart();
        }
        foreach (Entity entity in this.CurrentLevel.Players)
        {
            Player player = (Player)entity;
            player.StopFlashing();
            player.Unfreeze();
        }

        patch_DarkWorldLevelSystem darkWorld = MatchSettings.LevelSystem as patch_DarkWorldLevelSystem;
        var levelData = darkWorld.GetLevelData(MatchSettings.DarkWorldDifficulty, RoundIndex);
        patch_DarkWorldControl.ActivateTempVariants(CurrentLevel, levelData);
        if (MatchSettings.Variants.AlwaysDark)
        {
            CurrentLevel.OrbLogic.DoDarkOrb();
        }
        if (MatchSettings.Variants.SlowTime)
        {
            CurrentLevel.OrbLogic.DoTimeOrb(delay: true);
        }
        if (MatchSettings.Variants.AlwaysLava)
        {
            CurrentLevel.OrbLogic.DoLavaVariant();
        }
        if (MatchSettings.Variants.AlwaysScrolling)
        {
            CurrentLevel.OrbLogic.StartScroll();
        }
        /* Having some problems with this */
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
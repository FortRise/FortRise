using MonoMod;

namespace TowerFall;

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
        }
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
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.ArchivesStatsPage")]
public class ArchivesStatsPage : TowerFall.ArchivesStatsPage
{
    [MonoModIgnore]
    [MonoModConstructor]
    [MonoModIfFlag("NoLauncher")]
    [FixGameStatsArrowsShot]
    [FixGameStatsArrowsCollected]
    [FixGameStatsArrowsCaught]
    [FixGameStatsTimesLaunched]
    [FixGameStatsMatchesPlayed]
    [FixGameStatsRoundsPlayed]
    [FixGameStatsTotalVersusKills]
    [FixGameStatsJumps]
    [FixGameStatsDodges]
    public extern void ctor();
}


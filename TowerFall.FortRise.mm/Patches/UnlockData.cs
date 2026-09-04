using MonoMod;

namespace TowerFall;

public class patch_UnlockData : UnlockData
{
    public bool ShouldSelectTowerForgeInVersus
    {
        [MonoModIgnore]
        [MonoModIfFlag("NoLauncher")]
        [FixGameStatsRoundsPlayed]
        get => false;
    }

    public bool ShouldOpenPurpleArcherPortal
    {
        [MonoModIgnore]
        [MonoModIfFlag("NoLauncher")]
        [FixGameStatsRoundsPlayed]
        get => false;
    }

    public bool ShouldSelectSunkenCity
    {
        [MonoModIgnore]
        [MonoModIfFlag("NoLauncher")]
        [FixGameStatsVersusRandomPlays]
        get => false;
    }
}


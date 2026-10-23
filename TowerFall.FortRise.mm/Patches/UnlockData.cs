using FortRise;
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

    [MonoModReplace]
    public bool GetArcherUnlocked(int archerID)
    {
        switch (archerID)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                return true;
            case 4:
                return WhiteArcherIntro;
            case 5:
                return YellowArcherIntro;
            case 6:
                return CyanArcherIntro;
            case 7:
                return PurpleArcherIntro;
            case 8:
                return true;
            default:
                var archer = ArcherRegistry.GetArcherEntry(archerID);
                if (archer is null)
                {
                    return true;
                }

                if (archer.Configuration.Locked is { } locked)
                {
                    return !locked();
                }

                return true;
        }
    }
}


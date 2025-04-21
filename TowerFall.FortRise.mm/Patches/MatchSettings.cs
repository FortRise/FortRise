using System;
using FortRise;
using MonoMod;

namespace TowerFall;

public class patch_MatchSettings : MatchSettings
{
    private static int lastPlayers;
    public bool IsCustom;

    public patch_Modes Mode;
    public patch_MatchSettings.patch_MatchLengths MatchLength;
    private static readonly float[] GoalMultiplier;
    public static int CustomGoal { get; set; } = 1;


#nullable enable
    public string? CustomVersusModeName { get; internal set; }
    public IVersusGameMode? CustomVersusGameMode 
    {
        get 
        {
            if (CustomVersusModeName is null)
            {
                return null;
            }
            if (GameModeRegistry.RegistryVersusGameModes.TryGetValue(CustomVersusModeName, out IVersusGameModeEntry? versusGameMode))
            {
                return versusGameMode.VersusGameMode;
            }

            return null;
        }
    }
#nullable disable

    public extern int orig_get_GoalScore();

    public int get_GoalScore() 
    {
        if (IsCustom || MatchLength == patch_MatchLengths.Custom) 
        {
            var matchLength = (int)MatchLength;
            if (matchLength >= GoalMultiplier.Length) 
            {
                return patch_MatchSettings.CustomGoal;
            }
            int goals = (int)PlayerGoals(5, 8, 10);
            return (int)Math.Ceiling(((float)goals * GoalMultiplier[(int)MatchLength]));
        }
        return orig_get_GoalScore();
    }

    [MonoModReplace]
    public bool get_TeamMode() 
    {
        if (IsCustom) 
        {
            var gameMode = CustomVersusGameMode;
            if (gameMode != null) 
            {
                return gameMode.IsTeamMode;
            }
        }
        return Mode == patch_Modes.TeamDeathmatch;
    }

    public extern bool orig_get_CanPlayThisMode();

    public bool get_CanPlayThisMode()
    {
        if (IsCustom) 
        {
            if (CustomVersusGameMode.IsTeamMode) 
            {
                if (TFGame.PlayerAmount >= CustomVersusGameMode.GetMinimumTeamPlayers(this))
                {
                    return true;
                }
            }
            else 
            {
                if (TFGame.PlayerAmount >= CustomVersusGameMode.GetMinimumPlayers(this))
                {
                    return true;
                }
            }

            return false;
        }
        return orig_get_CanPlayThisMode();
    }

    [MonoModIgnore]
    private extern int PlayerGoals(int p2goal, int p3goal, int p4goal);

    public patch_MatchSettings(LevelSystem levelSystem, Modes mode, MatchLengths matchLength) : base(levelSystem, mode, matchLength)
    {
    }

    public enum patch_MatchLengths { Instant, Quick, Standard, Epic, Custom }

    public void CleanSettingsVersus()
    {
        if (!IsCustom) 
        {
            orig_CleanSettingsVersus();
            if (Mode == patch_Modes.LastManStanding)
                patch_VersusModeButton.currentIndex = 0;
            else if (Mode == patch_Modes.HeadHunters)
                patch_VersusModeButton.currentIndex = 1;
            else
                patch_VersusModeButton.currentIndex = 2;
            return;
        }
        if (lastPlayers != TFGame.PlayerAmount) 
        {
            Variants.Clean(lastPlayers);
        }
        lastPlayers = TFGame.PlayerAmount;
    }

    [MonoModIgnore]
    private extern void orig_CleanSettingsVersus();

    [Obsolete("Use 'CustomVersusModeName' instead")]
    public string CurrentModeName;
    [Obsolete("Use 'CustomVersusGameMode' instead")]
    public FortRise.CustomGameMode CurrentCustomGameMode 
    {
        get 
        {
            if (GameModeRegistry.TryGetGameMode(CurrentModeName, out var mode)) 
            {
                return mode as CustomGameMode;
            }
            return null;
        }
    } 
    
}
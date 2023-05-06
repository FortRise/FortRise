using System;
using MonoMod;

namespace TowerFall;

public class patch_MatchSettings : MatchSettings
{
    public bool IsCustom;
    public string CurrentModeName;
    private static readonly float[] GoalMultiplier;


    public extern int orig_get_GoalScore();

    public int get_GoalScore() 
    {
        if (IsCustom) 
        {
            int goals = (int)PlayerGoals(5, 8, 10);
            return (int)Math.Ceiling(((float)goals * GoalMultiplier[(int)MatchLength]));
        }
        return orig_get_GoalScore();
    }

    [MonoModIgnore]
    private extern int PlayerGoals(int p2goal, int p3goal, int p4goal);

    public patch_MatchSettings(LevelSystem levelSystem, Modes mode, MatchLengths matchLength) : base(levelSystem, mode, matchLength)
    {
    }
}
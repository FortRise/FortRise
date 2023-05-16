using System.Collections.Generic;
using Monocle;

namespace TowerFall;

public abstract class CustomVersusRoundLogic : RoundLogic
{
    internal static List<string> VersusModes = new () { "LastManStanding", "HeadHunters", "TeamDeathmatch" };
    internal static Dictionary<string, patch_Modes> LookUpModes = new Dictionary<string, patch_Modes>() 
    {
        {"LastManStanding", patch_Modes.LastManStanding},
        {"HeadHunters", patch_Modes.HeadHunters},
        {"TeamDeathmatch", patch_Modes.TeamDeathmatch}
    };

    protected CustomVersusRoundLogic(Session session, bool canHaveMiasma) : base(session, canHaveMiasma)
    {
    }
}

public struct RoundLogicInfo 
{
    public string Name;
    public Subtexture Icon;
    public RoundLogicType RoundType;
}

public enum RoundLogicType 
{
    FFA,
    HeadHunters,
    TeamDeatchmatch
}
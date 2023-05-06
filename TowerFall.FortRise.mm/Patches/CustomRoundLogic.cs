using System.Collections.Generic;
using Monocle;

namespace TowerFall;

public abstract class CustomVersusRoundLogic : RoundLogic
{
    internal static List<string> VersusModes = new () { "LastManStanding", "HeadHunters", "TeamDeathmatch" };
    internal static Dictionary<string, Modes> LookUpModes = new Dictionary<string, Modes>() 
    {
        {"LastManStanding", Modes.LastManStanding},
        {"HeadHunters", Modes.HeadHunters},
        {"TeamDeathmatch", Modes.TeamDeathmatch}
    };
    internal static List<Modes> BuiltInVersusModes = new () { 
        Modes.LastManStanding, Modes.HeadHunters, Modes.TeamDeathmatch };
    protected CustomVersusRoundLogic(Session session, bool canHaveMiasma) : base(session, canHaveMiasma)
    {
    }

}

public struct RoundLogicIdentifier 
{
    public string Name;
    public Subtexture Icon;
    public RoundLogicType RoundType;
}

public enum RoundLogicType 
{
    FFA,
    HeadHunters,
    // Team Deatchmatch?
}
using System.Collections.Generic;
using MonoMod;

namespace TowerFall;

public class patch_VersusRoundResults : VersusRoundResults
{
    private static Modes oldModes;
    public patch_VersusRoundResults(Session session, List<EventLog> events) : base(session, events)
    {
    }

    public extern void orig_ctor(patch_Session session, List<EventLog> events);

    [MonoModConstructor]
    public void ctor(patch_Session session, List<EventLog> events) 
    {
        if (session.MatchSettings.IsCustom) 
        {
            var roundType = CustomVersusRoundLogic.LookUpModes[session.MatchSettings.CurrentModeName];
            session.MatchSettings.Mode = roundType;
        }
        orig_ctor(session, events);
    }
}

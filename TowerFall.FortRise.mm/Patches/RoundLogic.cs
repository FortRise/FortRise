namespace TowerFall;

public class patch_RoundLogic : RoundLogic
{
    public patch_RoundLogic(Session session, bool canHaveMiasma) : base(session, canHaveMiasma)
    {
    }

    public extern static RoundLogic orig_GetRoundLogic(patch_Session session);

    public static RoundLogic GetRoundLogic(patch_Session session) 
    {
        if (session.MatchSettings.IsCustom && 
            FortRise.RiseCore.RoundLogicLoader.TryGetValue(session.MatchSettings.CurrentModeName, out var logic)) 
        {
            return logic(session, false);
        }
        return orig_GetRoundLogic(session);
    }
}

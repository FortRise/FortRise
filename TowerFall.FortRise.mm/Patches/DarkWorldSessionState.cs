using FortRise.Adventure;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldSessionState : DarkWorldSessionState
{
    private Session session;
    [MonoModPublic]
    public PlayerInventory defaultInventory;
    public patch_DarkWorldSessionState(Session session) : base(session)
    {
    }

    public extern int orig_get_ContinuesRemaining();

    public int get_ContinuesRemaining() 
    {
        if (session.GetLevelSet() != "TowerFall")
        {
            int id = session.MatchSettings.LevelSystem.ID.X;
            var tower = TowerRegistry.DarkWorldGet(session.GetLevelSet(), id);
            var amountContinue = tower.MaxContinues[(int)session.MatchSettings.DarkWorldDifficulty];
            if (amountContinue >= 0)
            {
                return amountContinue - Continues;
            }
        }
        return orig_get_ContinuesRemaining();
    }
}
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldSessionState : DarkWorldSessionState
{
    [MonoModPublic]
    public PlayerInventory defaultInventory;
    public patch_DarkWorldSessionState(Session session) : base(session)
    {
    }
}
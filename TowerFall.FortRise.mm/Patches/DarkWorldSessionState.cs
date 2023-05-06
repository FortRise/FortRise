#pragma warning disable CS0626
#pragma warning disable CS0108
using System.Collections.Generic;
using Monocle;
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
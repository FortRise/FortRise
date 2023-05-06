#pragma warning disable CS0626
#pragma warning disable CS0108
using System.Collections;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldComplete : DarkWorldComplete
{
    public patch_DarkWorldComplete(Session session) : base(session)
    {
    }


    [MonoModIgnore]
    [PatchDarkWorldCompleteSequence]
    private extern IEnumerator Sequence();
}
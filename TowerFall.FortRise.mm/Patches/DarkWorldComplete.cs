using System.Collections;
using MonoMod;

namespace TowerFall;

public partial class patch_DarkWorldComplete : DarkWorldComplete
{
    public patch_DarkWorldComplete(Session session) : base(session)
    {
    }

    // The most error prone patch
    [MonoModIgnore]
    [PatchDarkWorldCompleteSequence]
    private extern IEnumerator Sequence();
}
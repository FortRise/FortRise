#pragma warning disable CS0626
#pragma warning disable CS0108
using System.Collections;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldComplete : DarkWorldComplete
{
    private Session session;
    public patch_DarkWorldComplete(Session session) : base(session)
    {
    }


    [MonoModIgnore]
    [PatchDarkWorldCompleteSequence]
    private extern IEnumerator orig_Sequence();

    private IEnumerator Sequence() 
    {
        int deaths = 0;
        foreach (int num in this.session.DarkWorldState.Deaths)
        {
            deaths += num;
        }
        if (patch_SaveData.AdventureActive)
            patch_GameData.AdventureWorldTowers[session.MatchSettings.LevelSystem.ID.X].Stats.Complete(
                session.MatchSettings.DarkWorldDifficulty, TFGame.PlayerAmount, session.DarkWorldState.Time,
                session.DarkWorldState.Continues, deaths, session.MatchSettings.Variants.GetCoOpCurses()
            );
        yield return orig_Sequence();
    }
}
#pragma warning disable CS0626
#pragma warning disable CS0108


namespace TowerFall;

public class patch_DarkWorldTowerStats : DarkWorldTowerStats
{
    public extern void orig_Complete(DarkWorldDifficulties difficulty, int players, long time, int continues, int deaths, int curses);

    public void Complete(DarkWorldDifficulties difficulty, int players, long time, int continues, int deaths, int curses) 
    {
        if (!patch_SaveData.AdventureActive)
            orig_Complete(difficulty, players, time, continues, deaths, curses);
    }
}
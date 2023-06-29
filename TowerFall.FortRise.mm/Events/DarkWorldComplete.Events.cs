namespace TowerFall;

public partial class patch_DarkWorldComplete 
{
    public static class Events 
    {
        public delegate void DarkWorldComplete_ResultHandler(
            int levelID, DarkWorldDifficulties difficulties,
            int playerAmount, long time, int continues, int deaths, int curses);

        public static event DarkWorldComplete_ResultHandler OnDarkWorldComplete_Result;

        internal static void InvokeDarkWorldComplete_Result(
            int levelID, DarkWorldDifficulties difficulties,
            int playerAmount, long time, int continues, int deaths, int curses) 
        {
            if (patch_SaveData.AdventureActive)
            {
                patch_GameData.AdventureWorldTowers[levelID].Stats.Complete(
                    difficulties, playerAmount, time,
                    continues, deaths, curses);
                return;
            }
            SaveData.Instance.DarkWorld.Towers[levelID].Complete(
                difficulties, playerAmount, time, continues, deaths, curses
            );
            OnDarkWorldComplete_Result?.Invoke(levelID, difficulties, playerAmount, time, continues, deaths, curses);
        }
    }
}
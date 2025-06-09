using TowerFall;

namespace FortRise;

public static partial class RiseCore
{
    public static partial class Events 
    {
        public delegate void DarkWorldComplete_ResultHandler(
            int levelID, DarkWorldDifficulties difficulties,
            int playerAmount, long time, int continues, int deaths, int curses);

        public static event DarkWorldComplete_ResultHandler OnDarkWorldComplete_Result;

        internal static void InvokeDarkWorldComplete_Result(
            int levelID, DarkWorldDifficulties difficulties,
            int playerAmount, long time, int continues, int deaths, int curses, string levelSet) 
        {
            if (levelSet != "TowerFall") 
            {
                var data = TowerRegistry.DarkWorldGet(levelSet, levelID);
                FortRiseModule.SaveData.AdventureWorld.AddOrGet(data.GetLevelID()).Complete(
                    difficulties, playerAmount, time,
                    continues, deaths, curses);

                OnDarkWorldComplete_Result?.Invoke(levelID, difficulties, playerAmount, time, continues, deaths, curses);
                return;
            }
            SaveData.Instance.DarkWorld.Towers[levelID].Complete(
                difficulties, playerAmount, time, continues, deaths, curses
            );
            OnDarkWorldComplete_Result?.Invoke(levelID, difficulties, playerAmount, time, continues, deaths, curses);
        }
    }
}
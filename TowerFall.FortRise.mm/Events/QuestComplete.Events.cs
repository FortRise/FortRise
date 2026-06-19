using TowerFall;

namespace FortRise;

public static partial class RiseCore
{
    public static partial class Events 
    {
        public delegate void QuestComplete_ResultHandler(QuestRoundLogic quest, int playerAmount, long time, bool noDeaths);
        public static event QuestComplete_ResultHandler OnQuestComplete_Result;

        internal static void InvokeQuestComplete_Result(QuestRoundLogic quest, int playerAmount, long time, bool noDeaths) 
        {
            var towerSet = quest.Session.TowerSet;
            var levelID = quest.Session.MatchSettings.LevelSystem.ID.X;
            var hardcoreMode = quest.Session.MatchSettings.QuestHardcoreMode;
            
            var tower = SaveData.Instance.Quest.Towers[levelID];
            if (hardcoreMode)
                tower.BeatHardcore(TFGame.PlayerAmount, time, noDeaths);
            else
                tower.BeatNormal();
            OnQuestComplete_Result?.Invoke(quest, playerAmount, time, noDeaths);
        }
    }
}
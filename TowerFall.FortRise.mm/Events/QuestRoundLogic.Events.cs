using TowerFall;

namespace FortRise;

public static partial class RiseCore
{
    public static partial class Events 
    {
        public delegate void QuestRoundLogic_LevelLoadFinishHandler(QuestRoundLogic quest);
        public static event QuestRoundLogic_LevelLoadFinishHandler OnQuestRoundLogic_LevelLoadFinish;

        internal static void InvokeQuestRoundLogic_LevelLoadFinish(QuestRoundLogic quest) 
        {
            OnQuestRoundLogic_LevelLoadFinish?.Invoke(quest);
            var session = quest.Session;
            var levelID = session.MatchSettings.LevelSystem.ID.X;
            SaveData.Instance.Quest.Towers[levelID].TotalAttempts += 1;
        }

        public delegate void QuestRoundLogic_PlayerDeathsHandler(QuestRoundLogic quest);
        public static event QuestRoundLogic_PlayerDeathsHandler OnQuestRoundLogic_PlayerDeath;

        internal static void InvokeQuestRoundLogic_PlayerDeath(QuestRoundLogic quest) 
        {
            OnQuestRoundLogic_PlayerDeath?.Invoke(quest);
            var session = quest.Session;
            var levelID = session.MatchSettings.LevelSystem.ID.X;
            SaveData.Instance.Quest.Towers[levelID].TotalDeaths += 1;
        }
    }
}
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public static class TowerRegistry 
{
    public static Dictionary<string, IDarkWorldTowerEntry> DarkWorldTowers = [];
    public static Dictionary<string, IQuestTowerEntry> QuestTowers = [];
    public static Dictionary<string, ITrialsTowerEntry> TrialTowers = [];
    public static Dictionary<string, IVersusTowerEntry> VersusTowers = [];


    public static Dictionary<string, List<DarkWorldTowerData>> DarkWorldTowerSets = new();
    public static List<string> DarkWorldLevelSets = new();

    public static Dictionary<string, List<QuestLevelData>> QuestTowerSets = new();
    public static List<string> QuestLevelSets = new();

    public static Dictionary<string, List<VersusTowerData>> VersusTowerSets = new();
    public static List<string> VersusLevelSets = new();

    public static Dictionary<string, List<TrialsLevelData[]>> TrialsTowerSets = new();
    public static List<string> TrialsLevelSet = new();

    public static void PlayDarkWorld(string levelSet, string levelID, DarkWorldDifficulties difficulty, int startLevel = 0) 
    {
        // Checks if any players are present
        foreach (var player in TFGame.Players) 
        {
            if (player)
                // if there is, go to the level loading immediately
                goto Proceed;
        }
        //if not, activate atleast one player
        TFGame.Players[0] = true;
        Proceed:
        var level = DarkWorldGet(levelSet, levelID);
        patch_DarkWorldLevelSystem system = level.GetLevelSystem() as patch_DarkWorldLevelSystem;
        MainMenu.DarkWorldMatchSettings.DarkWorldDifficulty = difficulty;
        system.StartLevel = startLevel;
        MainMenu.DarkWorldMatchSettings.LevelSystem = system;
        Session session = new Session(MainMenu.DarkWorldMatchSettings);
        session.SetLevelSet(levelSet);
        session.StartGame();
    }

    public static void DarkWorldAdd(string levelSet, DarkWorldTowerData data) 
    {
        if (levelSet == string.Empty)
        {
            levelSet = "UNCATEGORIZED";
        }
        if (DarkWorldTowerSets.TryGetValue(levelSet, out var val)) 
        {
            data.ID.X = val.Count;
            val.Add(data);
            return;
        }
        DarkWorldLevelSets.Add(levelSet);
        var list = new List<DarkWorldTowerData>();
        data.ID.X = 0;
        list.Add(data);
        DarkWorldTowerSets[levelSet] = list; 
    }

    public static DarkWorldTowerData DarkWorldGet(string levelSet, int levelID) 
    {
        return DarkWorldTowerSets[levelSet][levelID];
    }

    public static bool TryDarkWorldGet(string levelSet, int levelID, out DarkWorldTowerData data) 
    {
        if (DarkWorldTowerSets.TryGetValue(levelSet, out var arr)) 
        {
            if (levelID < arr.Count && levelID > -1)
            {
                data = arr[levelID];
                return true;
            }
        }
        data = null;
        return false;
    }

    public static bool TryDarkWorldGet(string levelSet, string levelID, out DarkWorldTowerData data) 
    {
        if (DarkWorldTowerSets.TryGetValue(levelSet, out var arr)) 
        {
            foreach (var level in arr) 
            {
                if (level.GetLevelID() == levelID) 
                {
                    data = level;
                    return true;
                }
            }
        }
        data = null;
        return false;
    }

    public static DarkWorldTowerData DarkWorldGet(string levelSet, string levelID) 
    {
        var darkWorldLevel = DarkWorldTowerSets[levelSet];
        foreach (var level in darkWorldLevel) 
        {
            if (level.GetLevelID() == levelID) 
            {
                return level;
            }
        }
        return null;
    }

    public static DarkWorldTowerData DarkWorldGet(string levelSet, string levelID, out int id) 
    {
        var darkWorldLevel = DarkWorldTowerSets[levelSet];
        int i = 0;
        foreach (var level in darkWorldLevel) 
        {
            if (level.GetLevelID() == levelID) 
            {
                id = i;
                return level;
            }
            i++;
        }
        id = -1;
        return null;
    }

    public static bool TryDarkWorldGet(string levelSet, int levelID, out QuestLevelData data) 
    {
        if (QuestTowerSets.TryGetValue(levelSet, out var arr)) 
        {
            if (levelID < arr.Count && levelID > -1)
            {
                data = arr[levelID];
                return true;
            }
        }
        data = null;
        return false;
    }

    public static bool TryDarkWorldGet(string levelSet, string levelID, out QuestLevelData data) 
    {
        if (QuestTowerSets.TryGetValue(levelSet, out var arr)) 
        {
            foreach (var level in arr) 
            {
                if (level.GetLevelID() == levelID) 
                {
                    data = level;
                    return true;
                }
            }
        }
        data = null;
        return false;
    }


    public static void QuestAdd(string levelSet, QuestLevelData data) 
    {
        if (QuestTowerSets.TryGetValue(levelSet, out var val))  
        {
            data.ID.X = val.Count;
            val.Add(data);
            return;
        }
        QuestLevelSets.Add(levelSet);
        var list = new List<QuestLevelData>();
        data.ID.X = 0;
        list.Add(data);
        QuestTowerSets[levelSet] = list; 
    }

    public static QuestLevelData QuestGet(string levelSet, int levelID) 
    {
        return QuestTowerSets[levelSet][levelID];
    }

    public static QuestLevelData QuestGet(string levelSet, string levelID) 
    {
        var questLevel = QuestTowerSets[levelSet];
        foreach (var level in questLevel) 
        {
            if (level.GetLevelID() == levelID) 
            {
                return level;
            }
        }
        return null;
    }

    public static void VersusAdd(string levelSet, VersusTowerData data) 
    {
        if (VersusTowerSets.TryGetValue(levelSet, out var val)) 
        {
            data.ID.X = val.Count;
            val.Add(data);
            return;
        }
        VersusLevelSets.Add(levelSet);
        var list = new List<VersusTowerData>();
        data.ID.X = 0;
        list.Add(data);
        VersusTowerSets[levelSet] = list; 
    }


    public static VersusTowerData VersusGet(string levelSet, int levelID) 
    {
        return VersusTowerSets[levelSet][levelID];
    }

    public static VersusTowerData VersusGet(string levelSet, string levelID) 
    {
        var versusLevel = VersusTowerSets[levelSet];
        foreach (var level in versusLevel) 
        {
            if (level.GetLevelID() == levelID) 
            {
                return level;
            }
        }
        return null;
    }


    public static void TrialsAdd(TrialsLevelData[] datas) 
    {
        if (datas.Length == 0)
            return;    
        var levelSet = datas[0].GetLevelSet();
        if (TrialsTowerSets.TryGetValue(levelSet, out var val)) 
        {
            int i = 0;
            foreach (var data in datas) 
            {
                data.ID.X = val.Count;
                data.ID.Y = i;
                i++;
            }
            val.Add(datas);
            return;
        }
        TrialsLevelSet.Add(levelSet);
        var list = new List<TrialsLevelData[]>();
        datas[0].ID.X = 0;
        datas[1].ID.X = 0;
        datas[2].ID.X = 0;
        datas[0].ID.Y = 0;
        datas[1].ID.Y = 1;
        datas[2].ID.Y = 2;
        list.Add(datas);
        TrialsTowerSets[levelSet] = list; 
    }

    public static TrialsLevelData[] TrialsGet(string levelSet, int x, string levelID) 
    {
        var versusLevel = TrialsTowerSets[levelSet];
        foreach (var level in versusLevel) 
        {
            if (level[x].GetLevelID() == levelID) 
            {
                return level;
            }
        }
        return null;
    }

    public static TrialsLevelData[] TrialsGet(string levelSet, int levelID) 
    {
        return TrialsTowerSets[levelSet][levelID];
    }

    public static TrialsLevelData TrialsGet(string levelSet, int x, int y) 
    {
        return TrialsTowerSets[levelSet][x][y];
    }
}
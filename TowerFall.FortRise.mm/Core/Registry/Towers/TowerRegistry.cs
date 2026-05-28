using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public static class TowerRegistry 
{
    public static Dictionary<string, IDarkWorldTowerEntry> DarkWorldTowers = [];
    public static Dictionary<string, IQuestTowerEntry> QuestTowers = [];
    public static Dictionary<string, ITrialsTowerEntry> TrialTowers = [];
    public static Dictionary<string, IVersusTowerEntry> VersusTowers = [];


    public static List<string> DarkWorldLevelSets = new();
    public static List<string> QuestLevelSets = new();
    public static List<string> VersusLevelSets = new();
    public static List<string> TrialsLevelSet = new();


    [Obsolete("Use GameData.DarkWorldTowers")]
    public static DarkWorldTowerData DarkWorldGet(string levelSet, int levelID) 
    {
        return GameData.DarkWorldTowers[levelID];
    }

    [Obsolete("Use GameData.DarkWorldTowers")]
    public static bool TryDarkWorldGet(string levelSet, int levelID, out DarkWorldTowerData data)
    {
        data = null;
        try
        {
            data = GameData.DarkWorldTowers[levelID];
            return true;
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }
    }

    [Obsolete("Use GameData.DarkWorldTowers")]
    public static bool TryDarkWorldGet(string levelSet, string levelID, out DarkWorldTowerData data) 
    {
        if (DarkWorldTowers.TryGetValue(levelID, out var arr)) 
        {
            data = arr.DarkWorldTowerData;
            return true;
        }

        data = null;
        return false;
    }

    [Obsolete("Use GameData.DarkWorldTowers")]
    public static DarkWorldTowerData DarkWorldGet(string levelSet, string levelID) 
    {
        if (DarkWorldTowers.TryGetValue(levelID, out var arr)) 
        {
            return arr.DarkWorldTowerData;
        }

        return null;
    }

    [Obsolete("Use GameData.QuestLevels")]
    public static QuestLevelData QuestGet(string levelSet, int levelID) 
    {
        return GameData.QuestLevels[levelID];
    }

    [Obsolete("Use GameData.QuestLevels")]
    public static QuestLevelData QuestGet(string levelSet, string levelID)
    {
        return QuestTowers[levelID].QuestLevelData;
    }

    [Obsolete("Use GameData.VersusTowers")]
    public static VersusTowerData VersusGet(string levelSet, int levelID) => GameData.VersusTowers[levelID];

    [Obsolete("Use GameData.VersusTowers")]
    public static VersusTowerData VersusGet(string levelSet, string levelID) => VersusTowers[levelID].VersusTowerData;

    [Obsolete("Use GameData.TrialsLevels")]
    public static TrialsLevelData TrialsGet(string levelSet, int x, string levelID)
    {
        return TrialTowers[levelID].TrialsLevelData;
    }

    [Obsolete("Use GameData.TrialsLevels")]
    public static TrialsLevelData[] TrialsGet(string levelSet, int levelID)
    {
        throw new NotSupportedException("Getting the TrialsLevelData with only a levelID is not supported anymore.");
    }

    [Obsolete("Use GameData.TrialsLevels")]
    public static TrialsLevelData TrialsGet(string levelSet, int x, int y) 
    {
        return GameData.TrialsLevels[x, y];
    }
}
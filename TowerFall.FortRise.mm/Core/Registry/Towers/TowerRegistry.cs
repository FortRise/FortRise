using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TowerFall;

namespace FortRise;

public static class TowerRegistry 
{
    public static Dictionary<string, IDarkWorldTowerEntry> DarkWorldTowers = [];
    public static Dictionary<string, IQuestTowerEntry> QuestTowers = [];
    public static Dictionary<string, ITrialsTowerEntry> TrialTowers = [];
    public static Dictionary<string, IVersusTowerEntry> VersusTowers = [];

    public static Dictionary<string, ITowerTypeEntry> TowerTypes = [];
    public static Dictionary<MapButton.TowerType, ITowerTypeEntry> IDToTowerTypes = [];
    private static Dictionary<string, ITowerTypeEntry> vanillaCache = new();

    public static List<string> DarkWorldLevelSets = new();
    public static List<string> QuestLevelSets = new();
    public static List<string> VersusLevelSets = new();
    public static List<string> TrialsLevelSet = new();

    public static void AddTowerType(string id, ITowerTypeEntry entry)
    {
        TowerTypes[id] = entry;
        IDToTowerTypes[entry.TowerType] = entry;
    }

    public static ITowerTypeEntry GetTowerType(string name)
    {
        if (TowerTypes.TryGetValue(name, out ITowerTypeEntry entry))
        {
            return entry;
        }

        return CreateVanillaEntry(name);
    }

    private static ITowerTypeEntry CreateVanillaEntry(string id)
    {
        ref var cache = ref CollectionsMarshal.GetValueRefOrAddDefault(vanillaCache, id, out bool exists);
        if (exists)
        {
            return cache!;
        }

        if (!Enum.IsDefined(typeof(MapButton.TowerType), id))
        {
            return null;
        }


        var parsed = Enum.Parse<MapButton.TowerType>(id);


        cache = new TowerTypeEntry(id, parsed, new() 
        {
            BlockTexture = null,
            SmallBlockTexture = null,
            TowerSound = null
        });

        return cache;
    }
}

#nullable enable
using System;
using System.Collections.Generic;

namespace FortRise;

internal static class MapRendererRegistry 
{
    private static readonly Dictionary<string, IMapRendererEntry> mapEntries = [];
    private static readonly Dictionary<string, IMapRendererEntry> towerSetToMapEntries = [];

    public static void AddEntry(IMapRendererEntry entry)
    {
        mapEntries[entry.Name] = entry;
        towerSetToMapEntries[entry.Configuration.TowerSet] = entry;
    }

    public static IMapRendererEntry? GetEntry(string name)
    {
        mapEntries.TryGetValue(name, out var map);
        return map;
    }

    public static IMapRendererEntry? GetEntryFromTowerSet(string levelSet)
    {
        towerSetToMapEntries.TryGetValue(levelSet, out var map);
        return map;
    }
}

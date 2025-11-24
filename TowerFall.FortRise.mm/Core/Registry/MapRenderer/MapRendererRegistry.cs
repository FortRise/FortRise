#nullable enable
using System.Collections.Generic;

namespace FortRise;

internal static class MapRendererRegistry 
{
    private static readonly Dictionary<string, IMapRendererEntry> mapEntries = [];
    private static readonly Dictionary<string, IMapRendererEntry> levelSetToMapEntries = [];

    public static void AddEntry(IMapRendererEntry entry)
    {
        mapEntries[entry.Name] = entry;
        levelSetToMapEntries[entry.Configuration.LevelSet] = entry;
    }

    public static IMapRendererEntry? GetEntry(string name)
    {
        mapEntries.TryGetValue(name, out var map);
        return map;
    }

    public static IMapRendererEntry? GetEntryFromLevelSet(string levelSet)
    {
        levelSetToMapEntries.TryGetValue(levelSet, out var map);
        return map;
    }
}

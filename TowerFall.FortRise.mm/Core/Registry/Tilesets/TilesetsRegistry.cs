#nullable enable
using System.Collections.Generic;

namespace FortRise;

internal static class TilesetsRegistry
{
    private static Dictionary<string, ITilesetEntry> tilesetEntries = [];

    public static void AddTileset(ITilesetEntry entry)
    {
        tilesetEntries[entry.Name] = entry;
    }

    public static ITilesetEntry? GetTileset(string id)
    {
        tilesetEntries.TryGetValue(id, out var entry);
        return entry;
    }
}
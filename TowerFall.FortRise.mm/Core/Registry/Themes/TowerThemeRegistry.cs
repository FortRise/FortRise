#nullable enable
using System.Collections.Generic;

namespace FortRise;

internal static class TowerThemeRegistry
{
    private static Dictionary<string, IThemeEntry> themeEntries = [];

    public static void AddTheme(IThemeEntry entry)
    {
        themeEntries[entry.Name] = entry;
    }

    public static IThemeEntry? GetTheme(string id)
    {
        themeEntries.TryGetValue(id, out var entry);
        return entry;
    }
}
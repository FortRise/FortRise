#nullable enable
using System.Collections.Generic;

namespace FortRise;

internal static class BackgroundRegistry
{
    private static Dictionary<string, IBackgroundEntry> backgroundEntries = [];

    public static void AddBackground(IBackgroundEntry entry)
    {
        backgroundEntries[entry.Name] = entry;
    }

    public static IBackgroundEntry? GetBackground(string id)
    {
        backgroundEntries.TryGetValue(id, out var entry);
        return entry;
    }
}
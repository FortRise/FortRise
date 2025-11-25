#nullable enable
using System.Collections.Generic;

namespace FortRise;

internal static class MusicRegistry
{
    private static Dictionary<string, IMusicEntry> musicEntries = [];

    public static IReadOnlyCollection<IMusicEntry> MusicEntries => musicEntries.Values;

    public static void AddMusic(IMusicEntry entry)
    {
        musicEntries[entry.Name] = entry;
    }

    public static IMusicEntry? GetMusic(string id)
    {
        musicEntries.TryGetValue(id, out var entry);
        return entry;
    }
}
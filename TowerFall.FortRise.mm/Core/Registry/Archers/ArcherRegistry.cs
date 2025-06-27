#nullable enable
using System.Collections.Generic;

namespace FortRise;

internal static class ArcherRegistry
{
    private static Dictionary<string, IArcherEntry> archerEntries = [];

    public static void AddArcher(IArcherEntry entry)
    {
        archerEntries[entry.Name] = entry;
    }

    public static IArcherEntry? GetEntry(string id)
    {
        archerEntries.TryGetValue(id, out var entry);
        return entry;
    }
}
#nullable enable
using System.Collections.Generic;

namespace FortRise;

internal static class ArcherRegistry
{
    internal static readonly Dictionary<string, IArcherEntry> ArcherEntries = [];
    private static readonly Dictionary<int, IArcherEntry> indexToEntry = [];
    private static readonly List<IArcherEntry> moddedArchers = [];

    public static void AddArcher(IArcherEntry entry)
    {
        ArcherEntries[entry.Name] = entry;
        indexToEntry[entry.Index] = entry;
        moddedArchers.Add(entry);
    }

    public static IReadOnlyList<IArcherEntry> GetAllArcherEntries() 
    {
        return moddedArchers;
    }

    public static IArcherEntry? GetArcherEntry(int charIndex)
    {
        indexToEntry.TryGetValue(charIndex, out var entry);
        return entry;
    }

    public static IArcherEntry? GetArcherEntry(string id)
    {
        ArcherEntries.TryGetValue(id, out var entry);
        return entry;
    }

    public static IReadOnlyDictionary<string, IArcherEntry> GetArcherEntries()
    {
        return ArcherEntries;
    }
}

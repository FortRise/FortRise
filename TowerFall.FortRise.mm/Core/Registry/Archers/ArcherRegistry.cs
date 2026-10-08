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
        if (entry.Type == ArcherEntryType.Normal)
        {
            indexToEntry[entry.Index] = entry;
        }
        moddedArchers.Add(entry);
    }

    public static void RemoveArcher(IArcherEntry entry)
    {
        ArcherEntries.Remove(entry.Name);
        if (indexToEntry.TryGetValue(entry.Index, out var owner) && owner == entry)
        {
            indexToEntry.Remove(entry.Index);
        }
        moddedArchers.Remove(entry);
    }

    internal static void ShiftIndicesAfter(int removed)
    {
        foreach (var entry in moddedArchers)
        {
            if (entry.Index > removed && entry is ArcherEntry archer)
            {
                archer.SetIndex(entry.Index - 1);
            }
        }

        indexToEntry.Clear();
        foreach (var entry in moddedArchers)
        {
            if (entry.Type == ArcherEntryType.Normal)
            {
                indexToEntry[entry.Index] = entry;
            }
        }
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

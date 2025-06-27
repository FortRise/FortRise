#nullable enable
using System.Collections.Generic;

namespace FortRise;

public static class DarkWorldBossRegistry
{
    public static Dictionary<string, int> DarkWorldBosses = new Dictionary<string, int>();
    public static Dictionary<int, DarkWorldBossLoader?> DarkWorldBossLoader = new Dictionary<int, DarkWorldBossLoader?>();

    private static Dictionary<string, IDarkWorldBossEntry> darkWorldBossEntries = [];

    public static void AddDarkWorldBoss(IDarkWorldBossEntry entry)
    {
        darkWorldBossEntries[entry.Name] = entry;
    }

    public static IDarkWorldBossEntry? GetDarkWorldBoss(string id)
    {
        darkWorldBossEntries.TryGetValue(id, out var entry);
        return entry;
    }
}

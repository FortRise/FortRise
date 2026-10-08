#nullable enable
using System;
using TowerFall;

namespace FortRise;


internal static class ArcherStatsSync
{
    private static bool IsRuntime => ModuleManager.Instance.State == LoadState.Ready && SaveData.Instance is not null;

    public static void Backup()
    {
        var stats = SaveData.Instance?.Stats;
        if (stats is null)
        {
            return;
        }

        var store = FortRiseModule.SaveData;
        foreach (var entry in ArcherRegistry.GetAllArcherEntries())
        {
            if (entry.Type != ArcherEntryType.Normal)
            {
                continue;
            }

            int index = entry.Index;
            if (index < stats.Wins?.Length)
            {
                store.ArcherWins[entry.Name] = stats.Wins[index];
            }

            if (stats.Awards is { } awards)
            {
                var column = new ulong[awards.Length];
                for (int award = 0; award < awards.Length; award += 1)
                {
                    if (index < awards[award]?.Length)
                    {
                        column[award] = awards[award][index];
                    }
                }

                store.ArcherAwards[entry.Name] = column;
            }
        }
    }

    public static void Restore()
    {
        var stats = SaveData.Instance.Stats;
        var store = FortRiseModule.SaveData;

        foreach (var entry in ArcherRegistry.GetAllArcherEntries())
        {
            if (entry.Type != ArcherEntryType.Normal)
            {
                continue;
            }

            int index = entry.Index;
            if (index < stats.Wins?.Length && store.ArcherWins.TryGetValue(entry.Name, out ulong wins))
            {
                stats.Wins[index] = wins;
            }

            if (stats.Awards is { } awards && store.ArcherAwards.TryGetValue(entry.Name, out var column))
            {
                for (int award = 0; award < awards.Length && award < column.Length; award += 1)
                {
                    if (index < awards[award]?.Length)
                    {
                        awards[award][index] = column[award];
                    }
                }
            }
        }
    }

    public static void BeforeSlotsChange()
    {
        if (IsRuntime)
        {
            Backup();
        }
    }

    public static void AfterSlotsChange()
    {
        if (!IsRuntime)
        {
            return;
        }

        var stats = SaveData.Instance.Stats;
        var store = FortRiseModule.SaveData;
        int count = ArcherData.Archers.Length;

        var oldWins = stats.Wins ?? [];
        var oldAwards = stats.Awards ?? [];

        int awardCount = Math.Max(oldAwards.Length, VersusAwards.Awards.Length);

        var wins = new ulong[count];
        var awards = new ulong[awardCount][];
        for (int award = 0; award < awardCount; award += 1)
        {
            awards[award] = new ulong[count];
        }

        for (int i = 0; i < count; i += 1)
        {
            if (ArcherRegistry.GetArcherEntry(i) is { } entry)
            {
                store.ArcherWins.TryGetValue(entry.Name, out wins[i]);

                if (store.ArcherAwards.TryGetValue(entry.Name, out var column))
                {
                    for (int award = 0; award < awardCount && award < column.Length; award += 1)
                    {
                        awards[award][i] = column[award];
                    }
                }
            }
            else
            {
                // vanilla archers come first and never move
                wins[i] = i < oldWins.Length ? oldWins[i] : 0;

                for (int award = 0; award < awardCount; award += 1)
                {
                    if (award < oldAwards.Length && i < oldAwards[award]?.Length)
                    {
                        awards[award][i] = oldAwards[award][i];
                    }
                }
            }
        }

        stats.Wins = wins;
        stats.Awards = awards;
    }
}

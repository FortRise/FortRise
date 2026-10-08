#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

internal static class TowerDataSync
{
    public static bool IsRuntime => ModuleManager.Instance.State == LoadState.Ready && SaveData.Instance is not null;

    public static void Backup()
    {
        if (IsRuntime)
        {
            FortRiseModule.BackupTowerStats();
        }
    }

    // Call after GameData's tower lists changed.
    public static void Rebuild()
    {
        if (!IsRuntime)
        {
            return;
        }

        RebuildVersus();
        RebuildQuest();
        RebuildDarkWorld();
        RebuildTrials();
    }

    public static void RemoveVersusTower(IVersusTowerEntry entry)
    {
        Backup();

        TowerRegistry.VersusTowers.Remove(entry.ID);
        GameData.VersusTowers.RemoveAll(x => x.LevelID == entry.ID);

        for (int i = 0; i < GameData.VersusTowers.Count; i += 1)
        {
            var data = GameData.VersusTowers[i];
            data.ID = new Point(i, 0);
            if (TowerRegistry.VersusTowers.TryGetValue(data.LevelID, out var other))
            {
                other.LevelIndex = data.ID;
            }
        }

        RemoveSetIfUnused(TowerRegistry.VersusLevelSets, entry.TowerSet, TowerRegistry.VersusTowers.Values.Select(x => x.TowerSet));
        Rebuild();
    }

    public static void RemoveQuestTower(IQuestTowerEntry entry)
    {
        Backup();

        TowerRegistry.QuestTowers.Remove(entry.ID);
        GameData.QuestLevels = [.. GameData.QuestLevels.Where(x => x.LevelID != entry.ID)];

        for (int i = 0; i < GameData.QuestLevels.Length; i += 1)
        {
            var data = GameData.QuestLevels[i];
            data.ID = new Point(i, 0);
            if (TowerRegistry.QuestTowers.TryGetValue(data.LevelID, out var other))
            {
                other.LevelIndex = data.ID;
            }
        }

        RemoveSetIfUnused(TowerRegistry.QuestLevelSets, entry.TowerSet, TowerRegistry.QuestTowers.Values.Select(x => x.TowerSet));
        Rebuild();
    }

    public static void RemoveDarkWorldTower(IDarkWorldTowerEntry entry)
    {
        Backup();

        TowerRegistry.DarkWorldTowers.Remove(entry.ID);
        GameData.DarkWorldTowers.RemoveAll(x => x.LevelID == entry.ID);

        for (int i = 0; i < GameData.DarkWorldTowers.Count; i += 1)
        {
            var data = GameData.DarkWorldTowers[i];
            data.ID = new Point(i, 0);
            if (TowerRegistry.DarkWorldTowers.TryGetValue(data.LevelID, out var other))
            {
                other.LevelIndex = data.ID;
            }
        }

        RemoveSetIfUnused(TowerRegistry.DarkWorldLevelSets, entry.TowerSet, TowerRegistry.DarkWorldTowers.Values.Select(x => x.TowerSet));
        Rebuild();
    }

    public static void RemoveTrialTower(ITrialsTowerEntry entry)
    {
        Backup();

        TowerRegistry.TrialTowers.Remove(entry.ID);

        var old = GameData.TrialsLevels;
        int tiers = old.GetLength(1);
        string firstTier = entry.ID + "-1";

        List<int> keptRows = [];
        for (int x = 0; x < old.GetLength(0); x += 1)
        {
            if (old[x, 0]?.LevelID != firstTier)
            {
                keptRows.Add(x);
            }
        }

        var resized = new TrialsLevelData[keptRows.Count, tiers];
        for (int x = 0; x < keptRows.Count; x += 1)
        {
            for (int y = 0; y < tiers; y += 1)
            {
                var data = old[keptRows[x], y];
                resized[x, y] = data;
                if (data is not null)
                {
                    data.ID = new Point(x, y);
                }
            }
        }

        GameData.TrialsLevels = resized;

        foreach (var other in TowerRegistry.TrialTowers.Values)
        {
            for (int x = 0; x < resized.GetLength(0); x += 1)
            {
                if (resized[x, 0]?.LevelID == other.ID + "-1")
                {
                    other.LevelIndex = new Point(x, 0);
                    break;
                }
            }
        }

        RemoveSetIfUnused(TowerRegistry.TrialsLevelSet, entry.TowerSet, TowerRegistry.TrialTowers.Values.Select(x => x.TowerSet));
        Rebuild();
    }

    private static void RemoveSetIfUnused(List<string> sets, string set, IEnumerable<string> usedSets)
    {
        if (!usedSets.Contains(set))
        {
            sets.Remove(set);
        }
    }

    private static bool IsModded(string? levelID) => levelID is not null && levelID.Contains('/');

    private static void RebuildVersus()
    {
        var stats = SaveData.Instance.Stats;
        var old = stats.VersusTowerPlays ?? [];
        var store = FortRiseModule.SaveData.VersusTowerPlays;
        var plays = new ulong[GameData.VersusTowers.Count];

        for (int i = 0; i < plays.Length; i += 1)
        {
            var levelID = GameData.VersusTowers[i].LevelID;
            if (IsModded(levelID))
            {
                store.TryGetValue(levelID, out plays[i]);
            }
            else if (i < old.Length)
            {
                // vanilla towers come first and never move
                plays[i] = old[i];
            }
        }

        stats.VersusTowerPlays = plays;
    }

    private static void RebuildQuest()
    {
        var quest = SaveData.Instance.Quest;
        var store = FortRiseModule.SaveData.AdventureQuest.Towers;

        Dictionary<string, TowerFall.QuestTowerStats> current = [];
        foreach (var stats in quest.Towers ?? [])
        {
            if (stats is TowerFall.Patching.QuestTowerStats patched && patched.LevelID is not null)
            {
                current.TryAdd(patched.LevelID, stats);
            }
        }

        var towers = new TowerFall.QuestTowerStats[GameData.QuestLevels.Length];
        for (int i = 0; i < towers.Length; i += 1)
        {
            var levelID = GameData.QuestLevels[i].LevelID;
            if (!current.TryGetValue(levelID, out var stats))
            {
                stats = store.TryGetValue(levelID, out var stored) ? stored : new TowerFall.Patching.QuestTowerStats();
            }

            ((TowerFall.Patching.QuestTowerStats)stats).LevelID = levelID;
            towers[i] = stats;
        }

        quest.Towers = towers;
    }

    private static void RebuildDarkWorld()
    {
        var darkWorld = SaveData.Instance.DarkWorld;
        var store = FortRiseModule.SaveData.AdventureWorld.Towers;

        Dictionary<string, TowerFall.DarkWorldTowerStats> current = [];
        foreach (var stats in darkWorld.Towers ?? [])
        {
            if (stats is TowerFall.Patching.DarkWorldTowerStats patched && patched.LevelID is not null)
            {
                current.TryAdd(patched.LevelID, stats);
            }
        }

        var towers = new TowerFall.DarkWorldTowerStats[GameData.DarkWorldTowers.Count];
        for (int i = 0; i < towers.Length; i += 1)
        {
            var levelID = GameData.DarkWorldTowers[i].LevelID;
            if (!current.TryGetValue(levelID, out var stats))
            {
                stats = store.TryGetValue(levelID, out var stored) ? stored : new TowerFall.Patching.DarkWorldTowerStats();
            }

            ((TowerFall.Patching.DarkWorldTowerStats)stats).LevelID = levelID;
            towers[i] = stats;
        }

        darkWorld.Towers = towers;
    }

    private static void RebuildTrials()
    {
        var trials = SaveData.Instance.Trials;
        var store = FortRiseModule.SaveData.AdventureTrials.Towers;

        // trials stats are structs, so the latest values only exist in the vanilla array
        Dictionary<string, TowerFall.Patching.TrialsLevelStats> current = [];
        foreach (var row in trials.Levels ?? [])
        {
            foreach (var level in row)
            {
                var stats = Unsafe.BitCast<TowerFall.TrialsLevelStats, TowerFall.Patching.TrialsLevelStats>(level);
                if (stats.LevelID is not null)
                {
                    current.TryAdd(stats.LevelID, stats);
                }
            }
        }

        int rows = GameData.TrialsLevels.GetLength(0);
        int tiers = GameData.TrialsLevels.GetLength(1);
        var levels = new TowerFall.TrialsLevelStats[rows][];

        for (int x = 0; x < rows; x += 1)
        {
            levels[x] = new TowerFall.TrialsLevelStats[tiers];
            for (int y = 0; y < tiers; y += 1)
            {
                var levelID = GameData.TrialsLevels[x, y]?.LevelID;
                TowerFall.Patching.TrialsLevelStats stats = default;

                if (levelID is not null && !current.TryGetValue(levelID, out stats))
                {
                    store.TryGetValue(levelID, out stats);
                }

                stats.LevelID = levelID!;
                levels[x][y] = Unsafe.BitCast<TowerFall.Patching.TrialsLevelStats, TowerFall.TrialsLevelStats>(stats);
            }
        }

        trials.Levels = levels;
    }
}

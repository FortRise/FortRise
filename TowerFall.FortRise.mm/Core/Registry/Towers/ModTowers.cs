#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

public interface IModTowers
{
    IReadOnlyDictionary<string, IDarkWorldTowerEntry> RegisteredDarkWorldTowers { get; }
    IReadOnlyDictionary<string, IQuestTowerEntry> RegisteredQuestTowers { get; }
    IReadOnlyDictionary<string, ITrialsTowerEntry> RegisteredTrialTowers  { get; }
    IReadOnlyDictionary<string, IVersusTowerEntry> RegisteredVersusTowers { get; }
    IReadOnlyDictionary<string, ITowerTypeEntry> RegisteredTowerTypes { get; }

    IReadOnlyCollection<string> RegisteredDarkWorldTowerSets { get; }
    IReadOnlyCollection<string> RegisteredQuestTowerSets { get; }
    IReadOnlyCollection<string> RegisteredVersusTowerSets { get; }
    IReadOnlyCollection<string> RegisteredTrialsTowerSets { get; }

    ITowerTypeEntry RegisterTowerType(string id, in TowerTypeConfiguration configuration);

    IVersusTowerEntry RegisterVersusTower(string id, in VersusTowerConfiguration configuration);

    IVersusTowerEntry RegisterVersusTower(string id, string levelSet, in VersusTowerConfiguration configuration);

    IQuestTowerEntry RegisterQuestTower(string id, in QuestTowerConfiguration configuration);

    IQuestTowerEntry RegisterQuestTower(string id, string levelSet, in QuestTowerConfiguration configuration);

    IDarkWorldTowerEntry RegisterDarkWorldTower(string id, in DarkWorldTowerConfiguration configuration);

    IDarkWorldTowerEntry RegisterDarkWorldTower(string id, string levelSet, in DarkWorldTowerConfiguration configuration);

    ITrialsTowerEntry RegisterTrialTower(string id, in TrialsTowerConfiguration configuration);

    ITrialsTowerEntry RegisterTrialTower(string id, string levelSet, in TrialsTowerConfiguration configuration);

    ITowerTypeEntry? GetTowerType(string name);
    IVersusTowerEntry? GetVersusTower(string id);
    IQuestTowerEntry? GetQuestTower(string id);
    IDarkWorldTowerEntry? GetDarkWorldTower(string id);
    ITrialsTowerEntry? GetTrialTower(string id);
}

internal sealed class ModTowers : IModTowers
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<IVersusTowerEntry> versusTowerQueue;
    private readonly PreparedRegistryQueue<IQuestTowerEntry> questTowerQueue;
    private readonly RegistryQueue<IDarkWorldTowerEntry> darkWorldTowerQueue;
    private readonly PreparedRegistryQueue<ITrialsTowerEntry> trialTowerQueue;
    private readonly RegistryQueue<ITowerTypeEntry> towerTypeQueue;
    // prepared queues drop their entries after invoking, so these only keep them for detaching
    private readonly RegistryQueue<IQuestTowerEntry> questTowerDetachQueue;
    private readonly RegistryQueue<ITrialsTowerEntry> trialTowerDetachQueue;

    public IReadOnlyDictionary<string, IDarkWorldTowerEntry> RegisteredDarkWorldTowers => TowerRegistry.DarkWorldTowers;
    public IReadOnlyDictionary<string, IQuestTowerEntry> RegisteredQuestTowers => TowerRegistry.QuestTowers;
    public IReadOnlyDictionary<string, ITrialsTowerEntry> RegisteredTrialTowers => TowerRegistry.TrialTowers;
    public IReadOnlyDictionary<string, IVersusTowerEntry> RegisteredVersusTowers => TowerRegistry.VersusTowers;

    public IReadOnlyCollection<string> RegisteredDarkWorldTowerSets => TowerRegistry.DarkWorldLevelSets;
    public IReadOnlyCollection<string> RegisteredQuestTowerSets => TowerRegistry.QuestLevelSets;
    public IReadOnlyCollection<string> RegisteredVersusTowerSets => TowerRegistry.VersusLevelSets;
    public IReadOnlyCollection<string> RegisteredTrialsTowerSets => TowerRegistry.TrialsLevelSet;

    public IReadOnlyDictionary<string, ITowerTypeEntry> RegisteredTowerTypes => TowerRegistry.TowerTypes;

    internal ModTowers(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        versusTowerQueue = manager.CreateQueue<IVersusTowerEntry>(metadata, VersusTowerInvoke, VersusTowerDetach);
        questTowerQueue = manager.CreatePrepareQueue<IQuestTowerEntry>(QuestTowerPrepare, QuestTowerInvoke);
        questTowerDetachQueue = manager.CreateQueue<IQuestTowerEntry>(metadata, static _ => {}, QuestTowerDetach);
        darkWorldTowerQueue = manager.CreateQueue<IDarkWorldTowerEntry>(metadata, DarkWorldTowerInvoke, DarkWorldTowerDetach);
        trialTowerQueue = manager.CreatePrepareQueue<ITrialsTowerEntry>(TrialTowerPrepare, TrialTowerInvoke);
        trialTowerDetachQueue = manager.CreateQueue<ITrialsTowerEntry>(metadata, static _ => {}, TrialTowerDetach);
        towerTypeQueue = manager.CreateQueue<ITowerTypeEntry>(metadata, static _ => {}, TowerTypeDetach);
    }


    public ITowerTypeEntry RegisterTowerType(string id, in TowerTypeConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";

        ITowerTypeEntry entry = new TowerTypeEntry(name, IndexReservationPool.ClaimOrObtain<MapButton.TowerType>(metadata, name), configuration);
        TowerRegistry.AddTowerType(name, entry);
        towerTypeQueue.AddOrInvoke(entry);
        return entry;
    }

    public IVersusTowerEntry RegisterVersusTower(string id, in VersusTowerConfiguration configuration)
    {
        return RegisterVersusTower(id, metadata.Name, configuration);
    }

    public IVersusTowerEntry RegisterVersusTower(string id, string towerSet, in VersusTowerConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        string set = $"{metadata.Name}/{towerSet}";
        IVersusTowerEntry entry = new VersusTowerEntry(name, set, configuration);
        versusTowerQueue.AddOrInvoke(entry);
        TowerRegistry.VersusTowers.Add(name, entry);
        return entry;
    }

    public IQuestTowerEntry RegisterQuestTower(string id, in QuestTowerConfiguration configuration)
    {
        return RegisterQuestTower(id, metadata.Name, configuration);
    }

    public IQuestTowerEntry RegisterQuestTower(string id, string towerSet, in QuestTowerConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        string set = $"{metadata.Name}/{towerSet}";
        IQuestTowerEntry entry = new QuestTowerEntry(name, set, configuration);
        questTowerQueue.AddOrInvoke(entry);
        questTowerDetachQueue.AddOrInvoke(entry);
        TowerRegistry.QuestTowers.Add(name, entry);
        return entry;
    }

    public IDarkWorldTowerEntry RegisterDarkWorldTower(string id, in DarkWorldTowerConfiguration configuration)
    {
        return RegisterDarkWorldTower(id, metadata.Name, configuration);
    }

    public IDarkWorldTowerEntry RegisterDarkWorldTower(string id, string towerSet, in DarkWorldTowerConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        string set = $"{metadata.Name}/{towerSet}";
        IDarkWorldTowerEntry entry = new DarkWorldTowerEntry(name, set, configuration);
        darkWorldTowerQueue.AddOrInvoke(entry);
        TowerRegistry.DarkWorldTowers.Add(name, entry);
        return entry;
    }

    public ITrialsTowerEntry RegisterTrialTower(string id, in TrialsTowerConfiguration configuration)
    {
        return RegisterTrialTower(id, metadata.Name, configuration);
    }

    public ITrialsTowerEntry RegisterTrialTower(string id, string towerSet, in TrialsTowerConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        string set = $"{metadata.Name}/{towerSet}";
        ITrialsTowerEntry entry = new TrialsTowerEntry(name, set, configuration);
        trialTowerQueue.AddOrInvoke(entry);
        trialTowerDetachQueue.AddOrInvoke(entry);
        TowerRegistry.TrialTowers.Add(name, entry);
        return entry;
    }

    private void TowerTypeDetach(ITowerTypeEntry entry)
    {
        TowerRegistry.RemoveTowerType(entry);
        IndexReservationPool.FreeOrReserve(metadata, entry.Name, entry.TowerType);
    }

    // Hot reload keeps a tower's GameData slot, so its index and stats stay put.
    private void VersusTowerDetach(IVersusTowerEntry entry)
    {
        if (IndexReservationPool.For(metadata) is { } session && GameData.VersusTowers.Find(x => x.LevelID == entry.ID) is { } data)
        {
            TowerRegistry.VersusTowers.Remove(entry.ID);
            session.Reserve("versusTower", entry.ID, data, () => TowerDataSync.RemoveVersusTower(entry));
            return;
        }

        TowerDataSync.RemoveVersusTower(entry);
    }

    private void QuestTowerDetach(IQuestTowerEntry entry)
    {
        if (IndexReservationPool.For(metadata) is { } session && Array.Find(GameData.QuestLevels, x => x?.LevelID == entry.ID) is { } data)
        {
            TowerRegistry.QuestTowers.Remove(entry.ID);
            session.Reserve("questTower", entry.ID, data, () => TowerDataSync.RemoveQuestTower(entry));
            return;
        }

        TowerDataSync.RemoveQuestTower(entry);
    }

    private void DarkWorldTowerDetach(IDarkWorldTowerEntry entry)
    {
        if (IndexReservationPool.For(metadata) is { } session && GameData.DarkWorldTowers.Find(x => x.LevelID == entry.ID) is { } data)
        {
            TowerRegistry.DarkWorldTowers.Remove(entry.ID);
            session.Reserve("darkWorldTower", entry.ID, data, () => TowerDataSync.RemoveDarkWorldTower(entry));
            return;
        }

        TowerDataSync.RemoveDarkWorldTower(entry);
    }

    private void TrialTowerDetach(ITrialsTowerEntry entry)
    {
        if (IndexReservationPool.For(metadata) is { } session && FindTrialRow(entry.ID) is int row)
        {
            TowerRegistry.TrialTowers.Remove(entry.ID);
            session.Reserve("trialTower", entry.ID, GameData.TrialsLevels[row, 0], () => TowerDataSync.RemoveTrialTower(entry));
            return;
        }

        TowerDataSync.RemoveTrialTower(entry);
    }

    private static int? FindTrialRow(string id)
    {
        for (int x = 0; x < GameData.TrialsLevels.GetLength(0); x += 1)
        {
            if (GameData.TrialsLevels[x, 0]?.LevelID == id + "-1")
            {
                return x;
            }
        }

        return null;
    }

    private bool IsReserved(string kind, string id)
    {
        return IndexReservationPool.For(metadata)?.IsReserved(kind, id) == true;
    }

    private int ClaimIndex<TData>(string kind, string id, Func<TData, int> indexOf)
    where TData : class
    {
        if (IndexReservationPool.For(metadata) is { } session && session.TryClaim(kind, id, out TData data))
        {
            return indexOf(data);
        }

        return -1;
    }

    private void VersusTowerInvoke(IVersusTowerEntry entry)
    {
        TowerDataSync.Backup();
        int reclaimed = ClaimIndex<VersusTowerData>("versusTower", entry.ID, GameData.VersusTowers.IndexOf);
        var levelData = new patch_VersusTowerData
        {
            LevelID = entry.ID,
            TowerSet = entry.TowerSet,
            Author = entry.Configuration.Author?.ToUpperInvariant().Trim(),
            Levels = [],
            Procedural = entry.Configuration.Procedural,
            ID = new Point(reclaimed >= 0 ? reclaimed : GameData.VersusTowers.Count, 0)
        };

        foreach (var level in entry.Configuration.Levels)
        {
            var xml = level.Xml?["level"];
            if (xml is null)
            {
                continue;
            }

            int playerSpawnCount = 0;
            int teamSpawnsCount = 0;

            var entitiesXml = xml?["Entities"];
            if (entitiesXml is not null)
            {
                playerSpawnCount = entitiesXml.GetElementsByTagName("PlayerSpawn").Count;

                teamSpawnsCount = Math.Min(
                    entitiesXml.GetElementsByTagName("TeamSpawnA").Count,
                    entitiesXml.GetElementsByTagName("TeamSpawnB").Count
                );
            }

            var versusLevel = new patch_VersusLevelData()
            {
                Path = level.RootPath,
                PlayerSpawns = playerSpawnCount,
                TeamSpawns = teamSpawnsCount,
            };

            levelData.Levels.Add(versusLevel);
        }

        levelData.Theme = GameData.Themes[entry.Configuration.Theme];

        if (entry.Configuration.Treasure != null)
        {
            levelData.ArrowShuffle = entry.Configuration.ArrowShuffle;
            levelData.SpecialArrowRate = entry.Configuration.SpecialArrowRate;

            var count = PickupsRegistry.GetAllPickups().Count;

            levelData.TreasureMask = new int[TreasureSpawner.FullTreasureMask.Length + count];
            float[] treasureChances = [.. TreasureSpawner.DefaultTreasureChances];
            Array.Resize(ref treasureChances, TreasureSpawner.DefaultTreasureChances.Length + count);

            for (int i = 0; i < entry.Configuration.Treasure.Length; i += 1)
            {
                var treasure = entry.Configuration.Treasure[i];
                if (!treasure.Rates.TryGetValue(out int rate))
                {
                    levelData.TreasureMask[(int)treasure.Pickup] += 1;
                }
                else
                {
                    levelData.TreasureMask[(int)treasure.Pickup] += rate;
                }

                if (treasure.Chance.TryGetValue(out float chance))
                {
                    treasureChances[(int)treasure.Pickup] = chance;
                }
            }

            levelData.TreasureChances = treasureChances;
        }
        else
        {
            levelData.TreasureMask = TreasureSpawner.FullTreasureMask;
            levelData.ArrowShuffle = false;
            levelData.SpecialArrowRate = 0.6f;
        }

        // Inject
        entry.LevelIndex = levelData.ID; 
        if (!TowerRegistry.VersusLevelSets.Contains(entry.TowerSet))
        {
            TowerRegistry.VersusLevelSets.Add(entry.TowerSet);
        }
        if (reclaimed >= 0)
        {
            GameData.VersusTowers[reclaimed] = levelData;
        }
        else
        {
            GameData.VersusTowers.Add(levelData);
        }
        TowerDataSync.Rebuild();
    }

    private int totalQuestCount;

    private void QuestTowerPrepare(List<IQuestTowerEntry> entries)
    {
        TowerDataSync.Backup();
        totalQuestCount = GameData.QuestLevels.Length;
        int added = entries.Count(x => !IsReserved("questTower", x.ID));
        Array.Resize(ref GameData.QuestLevels, GameData.QuestLevels.Length + added);
    }

    private void QuestTowerInvoke(IQuestTowerEntry entry)
    {
        int index = ClaimIndex<QuestLevelData>("questTower", entry.ID, x => Array.IndexOf(GameData.QuestLevels, x));
        if (index < 0)
        {
            index = totalQuestCount;
            totalQuestCount += 1;
        }

        var levelData = new patch_QuestLevelData
        {
            LevelID = entry.ID,
            TowerSet = entry.TowerSet,
            Path = entry.Configuration.Level.RootPath,
            DataPath = entry.Configuration.Data.RootPath,

            Author = entry.Configuration.Author?.ToUpperInvariant().Trim(),
            Theme = GameData.Themes[entry.Configuration.Theme],
            ID = new Point(index, 0)
        };

        if (!TowerRegistry.QuestLevelSets.Contains(entry.TowerSet))
        {
            TowerRegistry.QuestLevelSets.Add(entry.TowerSet);
        }

        entry.LevelIndex = levelData.ID;
        GameData.QuestLevels[index] = levelData;
        TowerDataSync.Rebuild();
    }

    private void DarkWorldTowerInvoke(IDarkWorldTowerEntry entry)
    {
        TowerDataSync.Backup();
        int reclaimed = ClaimIndex<DarkWorldTowerData>("darkWorldTower", entry.ID, GameData.DarkWorldTowers.IndexOf);
        var towerData = new patch_DarkWorldTowerData
        {
            LevelID = entry.ID,
            TowerSet = entry.TowerSet,
            Author = entry.Configuration.Author?.ToUpperInvariant().Trim(),
            TimeAdd = entry.Configuration.TimeAdd,
            TimeBase = entry.Configuration.TimeBase,
            Theme = GameData.Themes[entry.Configuration.Theme],
            MaxContinues = entry.Configuration.MaxContinues,
            StartingLives = entry.Configuration.StartingLives,
            EnemySets = entry.Configuration.EnemySets,
            Levels = [],
            ID = new Point(reclaimed >= 0 ? reclaimed : GameData.DarkWorldTowers.Count, 0)
        };

        foreach (var level in entry.Configuration.Levels)
        {
            towerData.Levels.Add(level.RootPath);
        }

        towerData.Normal = CreateLevelData(entry.Configuration.Normal, towerData.EnemySets);
        towerData.Hardcore = CreateLevelData(entry.Configuration.Hardcore, towerData.EnemySets);
        towerData.Legendary = CreateLevelData(entry.Configuration.Legendary, towerData.EnemySets);

        entry.LevelIndex = towerData.ID;
        if (!TowerRegistry.DarkWorldLevelSets.Contains(entry.TowerSet))
        {
            TowerRegistry.DarkWorldLevelSets.Add(entry.TowerSet);
        }
        if (reclaimed >= 0)
        {
            GameData.DarkWorldTowers[reclaimed] = towerData;
        }
        else
        {
            GameData.DarkWorldTowers.Add(towerData);
        }
        TowerDataSync.Rebuild();


        static List<DarkWorldTowerData.LevelData> CreateLevelData(
            DarkWorldLevelData[] data,
            Dictionary<string, List<DarkWorldTowerData.EnemyData>> enemySets)
        {
            var newData = new List<patch_DarkWorldTowerData.patch_LevelData>();
            foreach (var level in data)
            {
                newData.Add(new(level, enemySets));
            }

            newData[^1].FinalLevel = true;

            return (List<DarkWorldTowerData.LevelData>)(object)newData;
        }
    }

    private int totalTrialsCount;

    private void TrialTowerPrepare(List<ITrialsTowerEntry> entries)
    {
        TowerDataSync.Backup();
        int origXLength = totalTrialsCount = GameData.TrialsLevels.GetLength(0);
        int origYLength = GameData.TrialsLevels.GetLength(1);
        int newRowXLength = origXLength + entries.Count(x => !IsReserved("trialTower", x.ID));

        TrialsLevelData[,] resized = new TrialsLevelData[newRowXLength, origYLength];

        for (int x = 0; x < origXLength; x += 1)
        {
            for (int y = 0; y < origYLength; y += 1)
            {
                resized[x, y] = GameData.TrialsLevels[x, y];
            }
        }

        GameData.TrialsLevels = resized;
    }

    private void TrialTowerInvoke(ITrialsTowerEntry entry)
    {
        int row = ClaimIndex<TrialsLevelData>("trialTower", entry.ID, _ => FindTrialRow(entry.ID) ?? -1);
        if (row < 0)
        {
            row = totalTrialsCount;
            totalTrialsCount += 1;
        }

        var tier1 = new patch_TrialsLevelData();
        tier1.LevelID = entry.ID + "-" + "1";
        tier1.TowerSet = entry.TowerSet;
        tier1.Author = entry.Configuration.Author?.ToUpperInvariant().Trim();
        tier1.Path = entry.Configuration.Tier1.Level.RootPath;
        tier1.Arrows = entry.Configuration.Tier1.Arrows;
        tier1.SwitchBlockTimer = entry.Configuration.Tier1.SwitchBlockTimer;
        tier1.Theme = GameData.Themes[entry.Configuration.Tier1.Theme];
        tier1.ID = new Point(row, 0);
        tier1.Goals = new TimeSpan[3];
        tier1.Goals[0] = TimeSpan.FromSeconds(entry.Configuration.Tier1.GoldTime);
        tier1.Goals[1] = TimeSpan.FromSeconds(entry.Configuration.Tier1.DiamondTime);
        tier1.Goals[2] = TimeSpan.FromSeconds(entry.Configuration.Tier1.DevTime);

        var tier2 = new patch_TrialsLevelData();
        tier2.LevelID = entry.ID + "-" + "2";
        tier2.TowerSet = entry.TowerSet;
        tier2.Author = entry.Configuration.Author;
        tier2.Path = entry.Configuration.Tier2.Level.RootPath;
        tier2.Arrows = entry.Configuration.Tier2.Arrows;
        tier2.SwitchBlockTimer = entry.Configuration.Tier2.SwitchBlockTimer;
        tier2.Theme = GameData.Themes[entry.Configuration.Tier2.Theme];
        tier2.ID = new Point(row, 1);
        tier2.Goals = new TimeSpan[3];
        tier2.Goals[0] = TimeSpan.FromSeconds(entry.Configuration.Tier2.GoldTime);
        tier2.Goals[1] = TimeSpan.FromSeconds(entry.Configuration.Tier2.DiamondTime);
        tier2.Goals[2] = TimeSpan.FromSeconds(entry.Configuration.Tier2.DevTime);

        var tier3 = new patch_TrialsLevelData();
        tier3.LevelID = entry.ID + "-" + "3";
        tier3.TowerSet = entry.TowerSet;
        tier3.Author = entry.Configuration.Author;
        tier3.Path = entry.Configuration.Tier3.Level.RootPath;
        tier3.Arrows = entry.Configuration.Tier3.Arrows;
        tier3.SwitchBlockTimer = entry.Configuration.Tier3.SwitchBlockTimer;
        tier3.Theme = GameData.Themes[entry.Configuration.Tier3.Theme];
        tier3.ID = new Point(row, 2);
        tier3.Goals = new TimeSpan[3];
        tier3.Goals[0] = TimeSpan.FromSeconds(entry.Configuration.Tier3.GoldTime);
        tier3.Goals[1] = TimeSpan.FromSeconds(entry.Configuration.Tier3.DiamondTime);
        tier3.Goals[2] = TimeSpan.FromSeconds(entry.Configuration.Tier3.DevTime);

        entry.LevelIndex = new Point(row, 0);

        GameData.TrialsLevels[row, 0] = tier1;
        GameData.TrialsLevels[row, 1] = tier2;
        GameData.TrialsLevels[row, 2] = tier3;

        if (!TowerRegistry.TrialsLevelSet.Contains(entry.TowerSet))
        {
            TowerRegistry.TrialsLevelSet.Add(entry.TowerSet);
        }

        TowerDataSync.Rebuild();
    }

    public IVersusTowerEntry? GetVersusTower(string id)
    {
        TowerRegistry.VersusTowers.TryGetValue(id, out IVersusTowerEntry? entry);
        return entry;
    }

    public IQuestTowerEntry? GetQuestTower(string id)
    {
        TowerRegistry.QuestTowers.TryGetValue(id, out IQuestTowerEntry? entry);
        return entry;
    }

    public IDarkWorldTowerEntry? GetDarkWorldTower(string id)
    {
        TowerRegistry.DarkWorldTowers.TryGetValue(id, out IDarkWorldTowerEntry? entry);
        return entry;
    }

    public ITrialsTowerEntry? GetTrialTower(string id)
    {
        TowerRegistry.TrialTowers.TryGetValue(id, out ITrialsTowerEntry? entry);
        return entry;
    }

    public ITowerTypeEntry? GetTowerType(string name)
    {
        return TowerRegistry.GetTowerType(name);
    }
}

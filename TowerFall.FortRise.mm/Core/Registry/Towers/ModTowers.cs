#nullable enable
using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public interface IModTowers
{
    IVersusTowerEntry RegisterVersusTower(string id, in VersusTowerConfiguration configuration);

    IVersusTowerEntry RegisterVersusTower(string id, string levelSet, in VersusTowerConfiguration configuration);

    IQuestTowerEntry RegisterQuestTower(string id, in QuestTowerConfiguration configuration);

    IQuestTowerEntry RegisterQuestTower(string id, string levelSet, in QuestTowerConfiguration configuration);

    IDarkWorldTowerEntry RegisterDarkWorldTower(string id, in DarkWorldTowerConfiguration configuration);

    IDarkWorldTowerEntry RegisterDarkWorldTower(string id, string levelSet, in DarkWorldTowerConfiguration configuration);

    ITrialsTowerEntry RegisterTrialTower(string id, in TrialsTowerConfiguration configuration);

    ITrialsTowerEntry RegisterTrialTower(string id, string levelSet, in TrialsTowerConfiguration configuration);

    IVersusTowerEntry? GetVersusTower(string id);
    IQuestTowerEntry? GetQuestTower(string id);
    IDarkWorldTowerEntry? GetDarkWorldTower(string id);
    ITrialsTowerEntry? GetTrialTower(string id);
}

internal sealed class ModTowers : IModTowers
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<IVersusTowerEntry> versusTowerQueue;
    private readonly RegistryQueue<IQuestTowerEntry> questTowerQueue;
    private readonly RegistryQueue<IDarkWorldTowerEntry> darkWorldTowerQueue;
    private readonly RegistryQueue<ITrialsTowerEntry> trialTowerQueue;

    internal ModTowers(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        versusTowerQueue = manager.CreateQueue<IVersusTowerEntry>(VersusTowerInvoke);
        questTowerQueue = manager.CreateQueue<IQuestTowerEntry>(QuestTowerInvoke);
        darkWorldTowerQueue = manager.CreateQueue<IDarkWorldTowerEntry>(DarkWorldTowerInvoke);
        trialTowerQueue = manager.CreateQueue<ITrialsTowerEntry>(TrialTowerInvoke);
    }

    public IVersusTowerEntry RegisterVersusTower(string id, in VersusTowerConfiguration configuration)
    {
        return RegisterVersusTower(id, metadata.Name, configuration);
    }

    public IVersusTowerEntry RegisterVersusTower(string id, string levelSet, in VersusTowerConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        string set = $"{metadata.Name}/{levelSet}";
        IVersusTowerEntry entry = new VersusTowerEntry(name, set, configuration);
        versusTowerQueue.AddOrInvoke(entry);
        TowerRegistry.VersusTowers.Add(name, entry);
        return entry;
    }

    public IQuestTowerEntry RegisterQuestTower(string id, in QuestTowerConfiguration configuration)
    {
        return RegisterQuestTower(id, metadata.Name, configuration);
    }

    public IQuestTowerEntry RegisterQuestTower(string id, string levelSet, in QuestTowerConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        string set = $"{metadata.Name}/{levelSet}";
        IQuestTowerEntry entry = new QuestTowerEntry(name, set, configuration);
        questTowerQueue.AddOrInvoke(entry);
        TowerRegistry.QuestTowers.Add(name, entry);
        return entry;
    }

    public IDarkWorldTowerEntry RegisterDarkWorldTower(string id, in DarkWorldTowerConfiguration configuration)
    {
        return RegisterDarkWorldTower(id, metadata.Name, configuration);
    }

    public IDarkWorldTowerEntry RegisterDarkWorldTower(string id, string levelSet, in DarkWorldTowerConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        string set = $"{metadata.Name}/{levelSet}";
        IDarkWorldTowerEntry entry = new DarkWorldTowerEntry(name, set, configuration);
        darkWorldTowerQueue.AddOrInvoke(entry);
        TowerRegistry.DarkWorldTowers.Add(name, entry);
        return entry;
    }

    public ITrialsTowerEntry RegisterTrialTower(string id, in TrialsTowerConfiguration configuration)
    {
        return RegisterTrialTower(id, metadata.Name, configuration);
    }

    public ITrialsTowerEntry RegisterTrialTower(string id, string levelSet, in TrialsTowerConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        string set = $"{metadata.Name}/{levelSet}";
        ITrialsTowerEntry entry = new TrialsTowerEntry(name, set, configuration);
        trialTowerQueue.AddOrInvoke(entry);
        TowerRegistry.TrialTowers.Add(name, entry);
        return entry;
    }

    private void VersusTowerInvoke(IVersusTowerEntry entry)
    {
        var levelData = new patch_VersusTowerData();
        levelData.SetLevelID(entry.ID);
        levelData.SetLevelSet(entry.LevelSet);
        levelData.Author = entry.Configuration.Author?.ToUpperInvariant().Trim();
        levelData.Levels = new();
        levelData.Procedural = entry.Configuration.Procedural;

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
            float[] treasureChances = (float[])TreasureSpawner.DefaultTreasureChances.Clone();
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

            levelData.SetTreasureChances(treasureChances);
        }
        else
        {
            levelData.TreasureMask = TreasureSpawner.FullTreasureMask;
            levelData.ArrowShuffle = false;
            levelData.SpecialArrowRate = 0.6f;
        }

        TowerRegistry.VersusAdd(levelData.GetLevelSet(), levelData);
    }

    private void QuestTowerInvoke(IQuestTowerEntry entry)
    {
        var levelData = new patch_QuestLevelData();
        levelData.SetLevelID(entry.ID);
        levelData.SetLevelSet(entry.LevelSet);
        levelData.Path = entry.Configuration.Level.RootPath;
        levelData.DataPath = entry.Configuration.Data.RootPath;

        levelData.Author = entry.Configuration.Author?.ToUpperInvariant().Trim();
        levelData.Theme = GameData.Themes[entry.Configuration.Theme];

        TowerRegistry.QuestAdd(levelData.GetLevelSet(), levelData);
    }

    private void DarkWorldTowerInvoke(IDarkWorldTowerEntry entry)
    {
        var towerData = new patch_DarkWorldTowerData();
        towerData.SetLevelID(entry.ID);
        towerData.SetLevelSet(entry.LevelSet);
        towerData.Author = entry.Configuration.Author?.ToUpperInvariant().Trim();
        towerData.TimeAdd = entry.Configuration.TimeAdd;
        towerData.TimeBase = entry.Configuration.TimeBase;
        towerData.Theme = GameData.Themes[entry.Configuration.Theme];
        towerData.MaxContinues = entry.Configuration.MaxContinues;
        towerData.StartingLives = entry.Configuration.StartingLives;
        towerData.EnemySets = entry.Configuration.EnemySets;
        towerData.Levels = [];

        foreach (var level in entry.Configuration.Levels)
        {
            towerData.Levels.Add(level.RootPath);
        }

        towerData.Normal = CreateLevelData(entry.Configuration.Normal, towerData.EnemySets);
        towerData.Hardcore = CreateLevelData(entry.Configuration.Hardcore, towerData.EnemySets);
        towerData.Legendary = CreateLevelData(entry.Configuration.Legendary, towerData.EnemySets);

        TowerRegistry.DarkWorldAdd(towerData.GetLevelSet(), towerData);


        static List<DarkWorldTowerData.LevelData> CreateLevelData(
            DarkWorldLevelData[] data,
            Dictionary<string, List<patch_DarkWorldTowerData.EnemyData>> enemySets)
        {
            var newData = new List<patch_DarkWorldTowerData.patch_LevelData>();
            foreach (var level in data)
            {
                var pickupList = new List<Pickups>[4];
                for (int i = 0; i < 4; i++)
                {
                    pickupList[i] = new List<Pickups>();
                }

                if (level.Treasures != null)
                {
                    foreach (var treasure in level.Treasures)
                    {
                        var minPlayer = 1;
                        var maxPlayer = 4;

                        if (treasure.MinPlayer.TryGetValue(out var min))
                        {
                            minPlayer = min;
                        }

                        if (treasure.MaxPlayer.TryGetValue(out var max))
                        {
                            maxPlayer = max;
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            if (i >= minPlayer - 1 && i <= maxPlayer - 1)
                            {
                                pickupList[i].Add(treasure.Pickups);
                            }
                        }
                    }
                }

                var levelMode = DarkWorldTowerData.LevelData.BossModes.Normal;
                if (level.BossID.TryGetValue(out int val))
                {
                    levelMode = DarkWorldTowerData.LevelData.BossModes.Boss;
                }
                newData.Add(new()
                {
                    File = level.LevelIndex,
                    BossID = val,
                    EnemySet = level.EnemySet is not null ? enemySets[level.EnemySet] : [],
                    DelayMultiplier = level.DelayMultiplier,
                    Difficulty = level.Difficulty,
                    Waves = level.Waves,
                    TreasureData = pickupList,
                    LevelMode = levelMode,
                    Variants = level.Variants
                });
            }

            newData[^1].FinalLevel = true;

            return (List<DarkWorldTowerData.LevelData>)(object)newData;
        }
    }

    private void TrialTowerInvoke(ITrialsTowerEntry entry)
    {
        var tier1 = new patch_TrialsLevelData();
        tier1.SetLevelID(entry.ID + "-" + "1");
        tier1.SetLevelSet(entry.LevelSet);
        tier1.Author = entry.Configuration.Author?.ToUpperInvariant().Trim();
        tier1.Path = entry.Configuration.Tier1.Level.RootPath;
        tier1.Arrows = entry.Configuration.Tier1.Arrows;
        tier1.SwitchBlockTimer = entry.Configuration.Tier1.SwitchBlockTimer;
        tier1.Theme = GameData.Themes[entry.Configuration.Tier1.Theme];
        tier1.Goals = new TimeSpan[3];
        tier1.Goals[0] = TimeSpan.FromSeconds(entry.Configuration.Tier1.GoldTime);
        tier1.Goals[1] = TimeSpan.FromSeconds(entry.Configuration.Tier1.DiamondTime);
        tier1.Goals[2] = TimeSpan.FromSeconds(entry.Configuration.Tier1.DevTime);

        var tier2 = new patch_TrialsLevelData();
        tier2.SetLevelID(entry.ID + "-" + "2");
        tier2.SetLevelSet(entry.LevelSet);
        tier2.Author = entry.Configuration.Author;
        tier2.Path = entry.Configuration.Tier2.Level.RootPath;
        tier2.Arrows = entry.Configuration.Tier2.Arrows;
        tier2.SwitchBlockTimer = entry.Configuration.Tier2.SwitchBlockTimer;
        tier2.Theme = GameData.Themes[entry.Configuration.Tier2.Theme];
        tier2.Goals = new TimeSpan[3];
        tier2.Goals[0] = TimeSpan.FromSeconds(entry.Configuration.Tier2.GoldTime);
        tier2.Goals[1] = TimeSpan.FromSeconds(entry.Configuration.Tier2.DiamondTime);
        tier2.Goals[2] = TimeSpan.FromSeconds(entry.Configuration.Tier2.DevTime);

        var tier3 = new patch_TrialsLevelData();
        tier3.SetLevelID(entry.ID + "-" + "3");
        tier3.SetLevelSet(entry.LevelSet);
        tier3.Author = entry.Configuration.Author;
        tier3.Path = entry.Configuration.Tier3.Level.RootPath;
        tier3.Arrows = entry.Configuration.Tier3.Arrows;
        tier3.SwitchBlockTimer = entry.Configuration.Tier3.SwitchBlockTimer;
        tier3.Theme = GameData.Themes[entry.Configuration.Tier3.Theme];
        tier3.Goals = new TimeSpan[3];
        tier3.Goals[0] = TimeSpan.FromSeconds(entry.Configuration.Tier3.GoldTime);
        tier3.Goals[1] = TimeSpan.FromSeconds(entry.Configuration.Tier3.DiamondTime);
        tier3.Goals[2] = TimeSpan.FromSeconds(entry.Configuration.Tier3.DevTime);

        TowerRegistry.TrialsAdd([
            tier1,
            tier2,
            tier3
        ]);
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
}
#nullable enable
using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public interface ITowerEntry
{
    public string ID { get; init; }
    public string LevelSet { get; init; }
}

public interface IVersusTowerEntry : ITowerEntry
{
    public VersusTowerConfiguration Configuration { get; init; }
}

public readonly struct VersusTowerConfiguration
{
    public required string Theme { get; init; }
    public required IResourceInfo[] Levels { get; init; }
    public Treasure[]? Treasure { get; init; }
    public string? Author { get; init; }
    public bool ArrowShuffle { get; init; }
    public float SpecialArrowRate { get; init; }
}

public readonly struct Treasure
{
    /// <summary>
    /// An enum pickup to use.
    /// </summary>
    public required Pickups Pickup { get; init; }
    /// <summary>
    /// Override the chance of a pickup.
    /// </summary>
    public Option<float> Chance { get; init; }
    /// <summary>
    /// Override the rate of a pickup.
    /// </summary>
    public Option<int> Rates { get; init; }
}

internal sealed class VersusTowerEntry : IVersusTowerEntry
{
    public VersusTowerConfiguration Configuration { get; init; }
    public string ID { get; init; }
    public string LevelSet { get; init; }

    public VersusTowerEntry(string id, string levelSet, VersusTowerConfiguration configuration)
    {
        ID = id;
        LevelSet = levelSet;
        Configuration = configuration;
    }
}

public class ModTowers
{
    private ModuleMetadata metadata;
    private readonly Dictionary<string, IVersusTowerEntry> versusTowerEntries = new Dictionary<string, IVersusTowerEntry>();
    private readonly RegistryQueue<IVersusTowerEntry> versusTowerQueue;

    internal ModTowers(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        versusTowerQueue = manager.CreateQueue<IVersusTowerEntry>(VersusTowerInvoke);
    }

    public IVersusTowerEntry RegisterVersusTower(string id, in VersusTowerConfiguration configuration)
    {
        return RegisterVersusTower(id, metadata.Name, configuration);
    }

    public IVersusTowerEntry RegisterVersusTower(string id, string levelSet, in VersusTowerConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        string set = $"{metadata.Name}/{levelSet}";
        IVersusTowerEntry entry = new VersusTowerEntry(name, levelSet, configuration);
        versusTowerEntries.Add(name, entry);
        versusTowerQueue.AddOrInvoke(entry);
        return entry;
    }

    private void VersusTowerInvoke(IVersusTowerEntry entry)
    {
        var levelData = new patch_VersusTowerData();
        levelData.SetLevelID(entry.ID);
        levelData.SetLevelSet(entry.LevelSet);
        levelData.Levels = new();

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
                Path = level.FullPath,
                PlayerSpawns = playerSpawnCount,
                TeamSpawns = teamSpawnsCount
            };

            levelData.Levels.Add(versusLevel);
        }

        levelData.Theme = GameData.Themes[entry.Configuration.Theme];
        levelData.ArrowShuffle = entry.Configuration.ArrowShuffle;
        levelData.SpecialArrowRate = entry.Configuration.SpecialArrowRate;

        if (entry.Configuration.Treasure != null)
        {
            for (int i = 0; i < entry.Configuration.Treasure.Length; i++)
            {
                var treasure = entry.Configuration.Treasure[i];
                if (!treasure.Rates.TryGetValue(out int rate))
                {
                    levelData.TreasureMask[(int)treasure.Pickup]++;
                }
                else
                {
                    levelData.TreasureMask[(int)treasure.Pickup] += rate;
                }

                if (treasure.Chance.TryGetValue(out float chance))
                {
                    levelData.GetTreasureChances()[(int)treasure.Pickup] = chance;
                }
            }
        }

        TowerRegistry.VersusAdd(levelData.GetLevelSet(), levelData);
    }
}
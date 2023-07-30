using System;
using System.Collections.Generic;
using System.IO;
using TowerFall;

namespace FortRise.Adventure;

public interface IAdventureTowerLoader 
{
    string FileExtension { get; }
    AdventureTowerInfo Load(int id, Stream stream, string levelDirectory, string directoryPrefix, bool customIcons);
}

public interface IAdventureTowerLoader<T> : IAdventureTowerLoader
{
    List<DarkWorldTowerData.LevelData> LoadLevelSet(T data, Dictionary<string, List<DarkWorldTowerData.EnemyData>> enemySets);
    ExtraAdventureTowerInfo LoadExtraData(T data);
}

public struct ExtraAdventureTowerInfo 
{
    public int StartingLives = -1;
    public int NormalContinues = -1;
    public int HardcoreContinues = -1;
    public int LegendaryContinues = -1;
    public bool Procedural = false;

    public ExtraAdventureTowerInfo() 
    {

    }
}

public struct AdventureTowerInfo 
{
    public string StoredDirectory;
    public int ID;
    public string Author;
    public AdventureWorldTowerStats Stats;
    public ExtraAdventureTowerInfo Extras;
    public int TimeBase;
    public int TimeAdd;
    public Dictionary<string, List<DarkWorldTowerData.EnemyData>> EnemySets;
    public TowerTheme Theme;
    public List<DarkWorldTowerData.LevelData> Normal;
    public List<DarkWorldTowerData.LevelData> Hardcore;
    public List<DarkWorldTowerData.LevelData> Legendary;
    public string RequiredMods;
}
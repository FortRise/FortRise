using System;
using System.IO;
using System.Xml;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldLevelSystem : DarkWorldLevelSystem
{
    private int startLevel;
    public int StartLevel 
    {
        get => startLevel;
        set => startLevel = value;
    }
    public patch_DarkWorldLevelSystem(DarkWorldTowerData tower) : base(tower)
    {
    }

    public override bool Procedural 
    {
        get 
        {
            return ((patch_DarkWorldTowerData)DarkWorldTowerData).Procedural;
        }
    }

    [MonoModIgnore]
    public DarkWorldTowerData DarkWorldTowerData { get; private set; }


    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(DarkWorldTowerData tower) 
    { 
        DarkWorldTowerData = tower;
        ID = tower.ID;
        Theme = tower.Theme;
        ShowControls = false;
        ShowTriggerControls = tower.TowerSet == "TowerFall" && ID.X == 2;
    }

    [MonoModReplace]
    public override XmlElement GetNextRoundLevel(MatchSettings matchSettings, int roundIndex, out int randomSeed)
    {
        if (Procedural)
        {
            matchSettings.RandomLevelSeed = new Random().Next(1000000000);
        }
        int file = DarkWorldTowerData[matchSettings.DarkWorldDifficulty][roundIndex + startLevel].File;
        randomSeed = file;
        var levelFile = DarkWorldTowerData.Levels[file];

        // Load ogmo levels
        if (levelFile.EndsWith("json"))
        {
            using var level = ModIO.OpenRead(levelFile);
            return Ogmo3ToOel.OgmoToOel(Ogmo3ToOel.LoadOgmo(level))["level"];
        }

        return patch_Calc.LoadXML(levelFile)["level"];
    }


    [MonoModIgnore]
    public extern patch_DarkWorldTowerData.patch_LevelData GetLevelData(DarkWorldDifficulties difficulty, int roundIndex);
}
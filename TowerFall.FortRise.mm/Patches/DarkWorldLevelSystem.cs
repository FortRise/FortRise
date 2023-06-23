using System;
using System.Xml;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldLevelSystem : DarkWorldLevelSystem
{
    private int startLevel;
    public patch_DarkWorldLevelSystem(DarkWorldTowerData tower) : base(tower)
    {
    }

    public override bool Procedural 
    {
        get 
        {
            if (DarkWorldTowerData is AdventureWorldTowerData towerData) 
            {
                return towerData.Procedural;
            }
            return false;
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
        ShowTriggerControls = (!patch_SaveData.AdventureActive && ID.X == 2);
    }

    [MonoModReplace]
    public override XmlElement GetNextRoundLevel(MatchSettings matchSettings, int roundIndex, out int randomSeed)
    {
        try 
        {
            if (Procedural) 
            {
                matchSettings.RandomLevelSeed = new Random().Next(1000000000);
            }
            int file = DarkWorldTowerData[matchSettings.DarkWorldDifficulty][roundIndex + startLevel].File;
            randomSeed = file;
            var levelFile = DarkWorldTowerData.Levels[file];
            if (DarkWorldTowerData is AdventureWorldTowerData && levelFile.EndsWith("json")) 
            {
                return Ogmo3ToOel.OgmoToOel(Ogmo3ToOel.LoadOgmo(levelFile))["level"];
            }
            return Calc.LoadXML(levelFile)["level"];
        }
        catch (ArgumentOutOfRangeException e)
        {
            ErrorHelper.StoreException("Missing Level", e);
            randomSeed = 0;
            return null;
        }
    }

    [MonoModIgnore]
    public extern patch_DarkWorldTowerData.patch_LevelData GetLevelData(DarkWorldDifficulties difficulty, int roundIndex);

    public override TilesetData GetBGTileset()
    {
        if (patch_SaveData.AdventureActive) 
        {
            if (patch_GameData.CustomTilesets.TryGetValue(Theme.BGTileset, out var val))
                return val;
        }
        return base.GetBGTileset();
    }

    public override TilesetData GetTileset()
    {
        if (patch_SaveData.AdventureActive) 
        {
            if (patch_GameData.CustomTilesets.TryGetValue(Theme.Tileset, out var val))
                return val;
        }
        return base.GetTileset();
    }
}
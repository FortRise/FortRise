using System;
using Monocle;
using TowerFall;

namespace FortRise.Adventure;

public class AdventureVersusLevelData : VersusLevelData
{
    public AdventureVersusLevelData(string path) : base(path)
    {
    }

    public static VersusLevelData CreateFromAdventure(string path) 
    {
        var levelData = new patch_VersusLevelData();
        levelData.Path = path;
        if (RiseCore.ResourceTree.TreeMap.TryGetValue(path, out var res)) 
        {
            using var xml = res.Stream;
            var xmlElement = patch_Calc.LoadXML(xml)["level"]["Entities"];
            levelData.PlayerSpawns = xmlElement.GetElementsByTagName("PlayerSpawn").Count;
            levelData.TeamSpawns = Math.Min(xmlElement.GetElementsByTagName("TeamSpawnA").Count, 
                xmlElement.GetElementsByTagName("TeamSpawnB").Count);
        }
        return levelData;
    }
}
using System.Xml;
using FortRise;
using Monocle;

namespace TowerFall;

public class patch_DarkWorldLevelSystem : DarkWorldLevelSystem
{
    private int startLevel;
    public patch_DarkWorldLevelSystem(DarkWorldTowerData tower) : base(tower)
    {
    }

    public override XmlElement GetNextRoundLevel(MatchSettings matchSettings, int roundIndex, out int randomSeed)
    {
        int file = DarkWorldTowerData[matchSettings.DarkWorldDifficulty][roundIndex + startLevel].File;
        randomSeed = file;
        var levelFile = DarkWorldTowerData.Levels[file];
        if (DarkWorldTowerData is AdventureWorldData worldData && levelFile.EndsWith("json")) 
        {
            return Ogmo3ToOel.OgmoToOel(Ogmo3ToOel.LoadOgmo(worldData.Levels[file]))["level"];
        }
        return Calc.LoadXML(DarkWorldTowerData.Levels[file])["level"];
    }
}
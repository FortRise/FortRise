using System.Xml;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_QuestLevelSystem : QuestLevelSystem
{
    public patch_QuestLevelSystem(QuestLevelData tower) : base(tower)
    {
    }

    [MonoModReplace]
    public override XmlElement GetNextRoundLevel(MatchSettings matchSettings, int roundIndex, out int randomSeed)
    {
        randomSeed = QuestTowerData.ID.X;
        if (QuestTowerData.GetLevelSet() != "TowerFall") 
        {
            using var xmlStream = RiseCore.ResourceTree.TreeMap[this.QuestTowerData.Path].Stream;
            return patch_Calc.LoadXML(xmlStream)["level"];
        }
        return Calc.LoadXML(this.QuestTowerData.Path)["level"];
    }
}
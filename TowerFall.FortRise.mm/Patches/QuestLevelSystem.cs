using System.Xml;
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
        return Calc.LoadXML(this.QuestTowerData.Path)["level"];
    }
}
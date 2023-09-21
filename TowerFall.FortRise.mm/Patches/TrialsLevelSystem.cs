using System.Xml;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_TrialsLevelSystem : TrialsLevelSystem
{
    public TrialsLevelData TrialsLevelData { [MonoModIgnore] get => null; [MonoModIgnore] private set => throw null; }
    
    public patch_TrialsLevelSystem(TrialsLevelData trial) : base(trial)
    {
    }

    [MonoModReplace]
    [MonoModConstructor]
    public void ctor(TrialsLevelData trial)
    {
        this.TrialsLevelData = trial;
        base.ID = trial.ID;
        base.Theme = trial.Theme;
        base.ShowControls = (base.ID.X == 0 && base.ID.Y == 0) && trial.GetLevelSet() == "TowerFall";
        base.ShowTriggerControls = base.ID.X == 14 && base.ID.Y != 0 && trial.GetLevelSet() == "TowerFall";
    }


    [MonoModReplace]
    public override XmlElement GetNextRoundLevel(MatchSettings matchSettings, int roundIndex, out int randomSeed)
    {
        randomSeed = this.TrialsLevelData.ID.X * 10 + this.TrialsLevelData.ID.Y;
        if (TrialsLevelData.GetLevelSet() != "TowerFall") 
        {
            using var xmlStream = RiseCore.ResourceTree.TreeMap[TrialsLevelData.Path].Stream;
            return patch_Calc.LoadXML(xmlStream)["level"];
        }
        return Calc.LoadXML(this.TrialsLevelData.Path)["level"];
    }
}
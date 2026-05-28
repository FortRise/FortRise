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
        base.ShowControls = base.ID.X == 0 && base.ID.Y == 0 && trial.TowerSet == "TowerFall";
        base.ShowTriggerControls = base.ID.X == 14 && base.ID.Y != 0 && trial.TowerSet == "TowerFall";
    }


    [MonoModReplace]
    public override XmlElement GetNextRoundLevel(MatchSettings matchSettings, int roundIndex, out int randomSeed)
    {
        randomSeed = this.TrialsLevelData.ID.X * 10 + this.TrialsLevelData.ID.Y;
        return Calc.LoadXML(this.TrialsLevelData.Path)["level"];
    }
}
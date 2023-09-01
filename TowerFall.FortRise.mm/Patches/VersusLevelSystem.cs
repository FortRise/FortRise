using System.Collections.Generic;
using System.Xml;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_VersusLevelSystem : VersusLevelSystem
{
    private List<string> levels;
    private string lastLevel;

    [MonoModIgnore]
    public VersusTowerData VersusTowerData { get; private set; }
    


    public patch_VersusLevelSystem(VersusTowerData tower) : base(tower)
    {
    }

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(VersusTowerData tower)
    {
        this.VersusTowerData = tower;
        base.ID = tower.ID;
        base.Theme = tower.Theme;
        base.ShowControls = base.ID.X == 0 && tower.IsOfficialLevelSet();
        base.ShowTriggerControls = base.ID.X == 14 && tower.IsOfficialLevelSet();
        this.levels = new List<string>();
    }

    public extern XmlElement orig_GetNextRoundLevel(MatchSettings matchSettings, int roundIndex, out int randomSeed);

    public override XmlElement GetNextRoundLevel(MatchSettings matchSettings, int roundIndex, out int randomSeed)
    {
        if (VersusTowerData.GetLevelSet() != "TowerFall") 
        {
            if (this.levels.Count == 0)
            {
                this.GenLevels(matchSettings);
            }
            this.lastLevel = this.levels[0];
            this.levels.RemoveAt(0);
            randomSeed = 0;
            foreach (char c in this.lastLevel)
            {
                randomSeed += (int)c;
            }
            if (RiseCore.ResourceTree.TreeMap.TryGetValue(lastLevel, out var res)) 
            {
                using var lastLevelStream = res.Stream;
                return patch_Calc.LoadXML(lastLevelStream)["level"];
            }
            Logger.Error($"[VERSUSLEVELSYSTEM][{lastLevel}] Path not found!");
        }
        return orig_GetNextRoundLevel(matchSettings, roundIndex, out randomSeed);
    }

    [MonoModIgnore]
    private extern void GenLevels(MatchSettings matchSettings);
}
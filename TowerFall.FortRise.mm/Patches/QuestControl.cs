using System.Xml;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_QuestControl : QuestControl 
{
    [MonoModLinkTo("TowerFall.HUD", "System.Void Added()")]
    [MonoModIgnore]
    public void base_Added() 
    {
        base.Added();
    }
    public override void Added()
    {
        base_Added();
        LoadSpawns();
        XmlDocument xmlDocument;
        if (Level.Session.IsOfficialLevelSet()) 
        {
            xmlDocument = Calc.LoadXML((base.Level.Session.MatchSettings.LevelSystem as QuestLevelSystem).QuestTowerData.DataPath);
        }
        else 
        {
            var path = (base.Level.Session.MatchSettings.LevelSystem as QuestLevelSystem).QuestTowerData.DataPath;
            using var xmlStream = RiseCore.Resources.GlobalResources[path].Stream;
            xmlDocument = patch_Calc.LoadXML(xmlStream);
        }
        Gauntlet = xmlDocument["data"].AttrBool("gauntlet", false);
        if (Gauntlet)
        {
            LoadGauntlet(xmlDocument);
            return;
        }
        LoadWaves(xmlDocument);
    }

    [MonoModIgnore]
    private extern void LoadSpawns();

    [MonoModIgnore]
    private extern void LoadGauntlet(XmlDocument doc);

    [MonoModIgnore]
    private extern void LoadWaves(XmlDocument doc);
}
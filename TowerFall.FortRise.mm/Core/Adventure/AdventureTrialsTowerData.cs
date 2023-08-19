using System.Xml;
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise.Adventure;

public class AdventureTrialsTowerData : patch_TrialsLevelData
{
    public AdventureTrialsTowerStats Stats;

    public AdventureTrialsTowerData(Point id, XmlElement xml) : base(id, xml)
    {
    }

    public AdventureTrialsTowerData() : base() {}
}
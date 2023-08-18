using System.Xml;
using Microsoft.Xna.Framework;
using MonoMod;
using FortRise.Adventure;

namespace TowerFall;

public class patch_TrialsLevelData : TrialsLevelData
{
    public AdventureTrialsTowerStats Stats;
    public patch_TrialsLevelData(Point id, XmlElement xml) : base(id, xml)
    {
    }

    public patch_TrialsLevelData() : base(default, null)
    {
    }

    [MonoModConstructor]
    public void ctor() {}
}
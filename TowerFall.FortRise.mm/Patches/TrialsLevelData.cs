using System.Xml;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_TrialsLevelData : TrialsLevelData
{
    public patch_TrialsLevelData(Point id, XmlElement xml) : base(id, xml)
    {
    }

    public patch_TrialsLevelData() : base(default, null)
    {
    }

    [MonoModConstructor]
    public void ctor() {}
}
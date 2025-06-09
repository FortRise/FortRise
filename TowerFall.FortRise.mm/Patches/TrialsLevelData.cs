using System.Xml;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_TrialsLevelData : TrialsLevelData
{
    [MonoModIgnore]
    [MonoModLinkTo("TowerFall.LevelData", "Author")]
    public string Author;

    public patch_TrialsLevelData(Point id, XmlElement xml) : base(id, xml)
    {
    }

    public patch_TrialsLevelData() : base(default, null)
    {
    }

    [MonoModConstructor]
    public void ctor() {}
}
using System.Xml;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_AutotileData : AutotileData
{
    public patch_AutotileData() : base(null) {}

    public patch_AutotileData(XmlElement xml) : base(xml)
    {
    }

    [MonoModConstructor]
    public void ctor() {}
}
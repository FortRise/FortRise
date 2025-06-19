using System.Xml;
using FortRise;
using MonoMod;

namespace TowerFall;

public class patch_ArcherData : ArcherData
{
    public static ArcherData[] Archers
    {
        [MonoModIgnore]
        get;
        [MonoModIgnore]
        [MonoModPublic]
        set;
    }
    public static ArcherData[] AltArchers
    {
        [MonoModIgnore]
        get;
        [MonoModIgnore]
        [MonoModPublic]
        set;
    }
    public static ArcherData[] SecretArchers
    {
        [MonoModIgnore]
        get;
        [MonoModIgnore]
        [MonoModPublic]
        set;
    }
    public patch_ArcherData(XmlElement xml) : base(xml)
    {
    }

    public patch_ArcherData() : base(null) {}

    public Option<HairInfo> ExtraHairData;

    [MonoModConstructor]
    public void ctor() {}
}
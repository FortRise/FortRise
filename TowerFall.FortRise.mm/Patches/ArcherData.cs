using System.Xml;
using FortRise;
using Monocle;
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

    public patch_ArcherData() : base(null) { }

    public Option<HairInfo> ExtraHairData;

    [MonoModConstructor]
    public void ctor() { }

    [MonoModReplace]
    public void PlayVictoryMusic()
    {
        if (patch_Audio.TrackMap.TryGetValue(VictoryMusic, out var info))
        {
            patch_Music.PlayImmediate(info.Name, false);
            patch_Music.PlayNext("TheArchives", true);
            return;
        }

        patch_Music.PlayImmediate("Victory" + VictoryMusic);
    }
}
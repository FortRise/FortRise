using System;
using System.IO;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework.Audio;
using Monocle;

namespace TowerFall;

public class patch_ArcherData : ArcherData
{
    private SoundEffectInstance instance;
    public patch_ArcherData(XmlElement xml) : base(xml)
    {
    }

    public void PlayCustomVictoryMusic(string archerPath) 
    {
        Music.Stop();
        var path = PathUtils.CombinePrefixPath(VictoryMusic, archerPath, "custom:");

        Music.Play(path);
    }
}
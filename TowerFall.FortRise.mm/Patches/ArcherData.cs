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
        if (instance == null) 
        {
            var victorySpan = VictoryMusic.AsSpan().Slice(7);
            var localPath = victorySpan.ToString();
            var path = Path.Combine(archerPath, localPath);

            if (!File.Exists(path)) 
            {
                Music.PlayImmediate("VictoryBlue");
                Logger.Error($"Path: {path} for Archer Victory music does not exists. Falling back to Blue Victory Music");
                return;
            }
            if (!SoundHelper.StoredInstance.ContainsKey(localPath)) 
            {
                SoundHelper.PathToSound(path, out instance);
                SoundHelper.StoredInstance.Add(path, instance);
            } 
            else 
            {
                instance = SoundHelper.StoredInstance[localPath];
            }
        }
        instance.Play();
    }
}
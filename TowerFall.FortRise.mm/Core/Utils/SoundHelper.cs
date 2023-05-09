using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Monocle;

namespace FortRise;

public static class SoundHelper 
{
    public static Dictionary<string, SoundEffectInstance> StoredInstance = new();

    public static SoundEffect PathToSound(string path, out SoundEffectInstance instance) 
    {
        var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        var effect = SoundEffect.FromStream(fs);
        fs.Close();
        instance = effect.CreateInstance();
        instance.IsLooped = true;
        instance.Volume = Music.MasterVolume;
        return effect;
    }
}
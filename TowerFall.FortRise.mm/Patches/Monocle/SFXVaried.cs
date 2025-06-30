using System.IO;
using FortRise;
using Microsoft.Xna.Framework.Audio;
using MonoMod;

namespace Monocle;

public class patch_SFXVaried : patch_SFX
{
    public SoundEffect[] Datas
    {
        [MonoModIgnore]
        get => throw null;
        [MonoModIgnore]
        private set => throw null;
    }

    [MonoModIgnore]
    public bool ObeysMasterPitch
    {
        [MonoModIgnore] get => throw null;
        [MonoModIgnore] private set => throw null;
    }

    internal patch_SFXVaried(bool obeysMasterPitch) : base(obeysMasterPitch)
    {
    }

    public patch_SFXVaried(Stream[] stream, int amount, bool obeysMasterPitch) : base(obeysMasterPitch)
    {
    }

    [MonoModLinkTo("Monocle.SFX", "System.Void .ctor(System.Boolean)")]
    [MonoModIgnore]
    public void thisctor(bool obeysMasterPitch) {}

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(string filename, int amount, bool obeysMasterPitch) 
    {
        thisctor(obeysMasterPitch);
        Datas = new SoundEffect[amount];
        for (int i = 0; i < amount; i++)
        {
            using Stream fileStream = ModIO.OpenRead(Audio.LOAD_PREFIX + filename + GetSuffix(i + 1) + ".wav");
            try
            {
                Datas[i] = SoundEffect.FromStream(fileStream);
            }
            catch
            {
                Datas = null;
            }
        }
    }

    [MonoModIgnore]
    private extern string GetSuffix(int num);

    [MonoModConstructor]
    public void ctor(Stream[] stream, int amount, bool obeysMasterPitch) 
    {
        this.thisctor(obeysMasterPitch);
        Datas = new SoundEffect[amount];
        for (int i = 0; i < amount; i++)
        {
            var current = stream[i];
            try
            {
                Datas[i] = SoundEffect.FromStream(current);
            }
            catch
            {
                Datas = null;
            }
            current.Close();
        }
    }
}
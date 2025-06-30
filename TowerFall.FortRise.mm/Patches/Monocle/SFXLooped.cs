using System.IO;
using FortRise;
using Microsoft.Xna.Framework.Audio;
using MonoMod;

namespace Monocle;

public class patch_SFXLooped : patch_SFX
{
    public SoundEffectInstance Instance
    {
        [MonoModIgnore] get => throw null;
        [MonoModIgnore] private set => throw null;
    }
    internal patch_SFXLooped(bool obeysMasterPitch) : base(obeysMasterPitch)
    {
    }

    public patch_SFXLooped(Stream stream, bool obeysMasterPitch) : base(stream, obeysMasterPitch)
    {
    }

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(string filename, bool obeysMasterPitch = true) 
    {
        base.ctor(filename, obeysMasterPitch);
        if (base.Data != null)
        {
            this.Instance = base.Data.CreateInstance();
            this.Instance.IsLooped = true;
            patch_Audio.loopList.Add(this);
        }
    }

    [MonoModConstructor]
    public void ctor(Stream stream, bool obeysMasterPitch = true) 
    {
        base.ctor(stream, obeysMasterPitch);
        if (base.Data != null)
        {
            this.Instance = base.Data.CreateInstance();
            this.Instance.IsLooped = true;
            patch_Audio.loopList.Add(this);
        }
    }
}
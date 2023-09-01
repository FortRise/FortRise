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

public static class SFXLoopedExt 
{
    public static patch_SFXLooped CreateSFXLooped(this FortContent content, string filename, bool obeysMasterPitch = true, ContentAccess contentAccess = ContentAccess.Root) 
    {
        if (Path.GetExtension(filename) == string.Empty)
            filename += ".wav";
        switch (contentAccess) 
        {
        case ContentAccess.Content: 
            filename = Calc.LOADPATH + filename;
            break;
        case ContentAccess.ModContent:
            {
                if (content == null) 
                {
                    Logger.Error("[Atlas] You cannot use SFXLoopedExt.CreateSFXLooped while FortContent is null");
                    return null;
                }
                using var stream = content[filename].Stream;
                return CreateSFXLooped(content, stream, obeysMasterPitch);
            }
        }
        using var fileStream = new FileStream(filename, FileMode.Open);
        return CreateSFXLooped(content, fileStream, obeysMasterPitch);
    }

    public static patch_SFXLooped CreateSFXLooped(this FortContent content, Stream stream, bool obeysMasterPitch = true) 
    {
        return new patch_SFXLooped(stream, obeysMasterPitch);
    }
}
using System.IO;
using FortRise;
using Microsoft.Xna.Framework.Audio;
using MonoMod;

namespace Monocle;

public class patch_SFXInstanced : patch_SFX 
{
    public SoundEffectInstance[] Instances
    {
        [MonoModIgnore] get => throw null;
        [MonoModIgnore] private set => throw null;
    }
    public patch_SFXInstanced(string filename, int instances = 2, bool obeysMasterPitch = true) : base(false)
    {
    }

    public patch_SFXInstanced(Stream stream, int instances = 2, bool obeysMasterPitch = true) : base(stream, obeysMasterPitch)
    {
    }

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(string filename, int instances = 2, bool obeysMasterPitch = true) 
    {
        base.ctor(filename, obeysMasterPitch);
        if (base.Data != null)
        {
            this.Instances = new SoundEffectInstance[instances];
            for (int i = 0; i < instances; i++)
            {
                this.Instances[i] = base.Data.CreateInstance();
            }
        }
    }

    [MonoModConstructor]
    public void ctor(Stream stream, int instances = 2, bool obeysMasterPitch = true) 
    {
        base.ctor(stream, obeysMasterPitch);
        if (base.Data != null)
        {
            this.Instances = new SoundEffectInstance[instances];
            for (int i = 0; i < instances; i++)
            {
                this.Instances[i] = base.Data.CreateInstance();
            }
        }
    }
}

public static class SFXInstancedExt 
{
    public static patch_SFXInstanced CreateSFXInstanced(this FortContent content, string filename, int instances = 2, bool obeysMasterPitch = true, ContentAccess contentAccess = ContentAccess.Root) 
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
                    Logger.Error("[Atlas] You cannot use SFXInstancedExt.CreateSFXInstanced while FortContent is null");
                    return null;
                }
                using var stream = content[filename].Stream;
                return CreateSFXInstanced(content, stream, instances, obeysMasterPitch);
            }
        }
        using var fileStream = new FileStream(filename, FileMode.Open);
        return CreateSFXInstanced(content, fileStream, instances, obeysMasterPitch);
    }

    public static patch_SFXInstanced CreateSFXInstanced(this FortContent content, Stream stream, int instances = 2, bool obeysMasterPitch = true) 
    {
        return new patch_SFXInstanced(stream, instances, obeysMasterPitch);
    }
}
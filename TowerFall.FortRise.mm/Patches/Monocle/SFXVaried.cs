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

public static class SFXVariedExt 
{
    public static patch_SFXVaried CreateSFXVaried(this FortContent content, string filename, int amount, bool obeysMasterPitch = true, ContentAccess contentAccess = ContentAccess.Root) 
    {
        var currentExtension = Path.GetExtension(".wav");
        if (currentExtension == string.Empty)
            currentExtension = ".wav";
        filename = filename.Replace(currentExtension, "");
        switch (contentAccess) 
        {
        case ContentAccess.Content: 
            filename = Calc.LOADPATH + filename;
            break;
        case ContentAccess.ModContent:
            {
                if (content == null) 
                {
                    Logger.Error("[Atlas] You cannot use SFXVariedExt.CreateSFXVaried while FortContent is null");
                    return null;
                }
                var contentStreams = new Stream[amount];
                for (int i = 0; i < amount; i++) 
                {
                    contentStreams[i] = content[filename + GetSuffix(i + 1) + currentExtension].Stream;
                }
                return CreateSFXVaried(content, contentStreams, amount, obeysMasterPitch);
            }
        }
        var streams = new Stream[amount];
        for (int i = 0; i < amount; i++) 
        {
            streams[i] = File.OpenRead(filename + GetSuffix(i + 1) + currentExtension);
        }
        
        using var fileStream = new FileStream(filename, FileMode.Open);
        return CreateSFXVaried(content, streams, amount, obeysMasterPitch);
    }

    public static patch_SFXVaried CreateSFXVaried(this FortContent content, Stream[] stream, int amount, bool obeysMasterPitch = true) 
    {
        return new patch_SFXVaried(stream, amount, obeysMasterPitch);
    }

    public static string GetSuffix(int num)
    {
        return "_" + ((num < 10) ? ("0" + num.ToString()) : num.ToString());
    }
}
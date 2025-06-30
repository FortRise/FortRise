using System.IO;
using FortRise;
using Microsoft.Xna.Framework.Audio;
using MonoMod;

namespace Monocle;

public class patch_SFX : SFX
{
    public SoundEffect Data
    {
        [MonoModIgnore] get => throw null;
        [MonoModIgnore] private set => throw null;
    }
    public bool ObeysMasterPitch
    {
        [MonoModIgnore] get => throw null;
        [MonoModIgnore] private set => throw null;
    }
    internal patch_SFX(bool obeysMasterPitch) :base("", false) {}
    public patch_SFX(string filename, bool obeysMasterPitch = true) : base(filename, obeysMasterPitch)
    {
    }
    public patch_SFX(Stream filename, bool obeysMasterPitch = true) : base("", obeysMasterPitch)
    {
    }

    [MonoModConstructor]
    [MonoModReplace]
    internal void ctor(bool obeysMasterPitch) 
    {
        ObeysMasterPitch = obeysMasterPitch;
        lock (patch_Audio.pitchList)
        {
            patch_Audio.pitchList.Add(this);
        }
    }

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(string filename, bool obeysMasterPitch = true) 
    {
        ctor(obeysMasterPitch);
        string path = Audio.LOAD_PREFIX + filename + ".wav";
        if (!ModIO.IsDirectoryOrFileExists(path)) 
        {
            path = filename; // Mods can use .ogg
            if (!Path.HasExtension(path))
                path += ".wav";
        }
        using Stream stream = ModIO.OpenRead(path);
        try
        {
            Data = SoundEffect.FromStream(stream);
        }
        catch (NoAudioHardwareException)
        {
            Data = null;
        }
    }

    [MonoModConstructor]
    public void ctor(Stream stream, bool obeysMasterPitch = true) 
    {
        ctor(obeysMasterPitch);
        try
        {
            Data = SoundEffect.FromStream(stream);
        }
        catch (NoAudioHardwareException)
        {
            Data = null;
        }
    }

    [MonoModReplace]
    public override void Play(float panX = 160f, float volume = 1f)
    {
        if (Data != null && Audio.MasterVolume > 0f)
        {
            AddToPlayedList(panX, volume);
            volume *= Audio.MasterVolume;
            var instance = Data.CreateInstance();
            instance.Volume = volume;
            instance.Pitch = ObeysMasterPitch ? Audio.MasterPitch : 0f;
            instance.Pan = SFX.CalculatePan(panX);
            instance.Play();
        }
    }
}
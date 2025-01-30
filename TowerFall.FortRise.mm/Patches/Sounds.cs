using System.Collections.Generic;
using System.IO;
using FortRise;
using Monocle;

namespace TowerFall;

public static class patch_Sounds 
{
    public static Dictionary<string, SFX> SoundsLoaded { get; private set; } = new Dictionary<string, SFX>();

    public static void AddSFX(FortContent content, string id, SFX sfx) 
    {
        var name = content.ResourceSystem.Metadata.Name;
        SoundsLoaded[$"{name}/{id}"] = sfx;
    }

    public static void Play(string sfxName, float panX = 160f, float volume = 1f) 
    {
        if (SoundsLoaded.TryGetValue(sfxName, out var sfx))
        {
            sfx.Play(panX, volume);
            return;
        }

        Logger.Error($"SFX '{sfxName}' cannot be played as it cannot be found.");
    }

    public static void Pause(string sfxName) 
    {
        if (SoundsLoaded.TryGetValue(sfxName, out var sfx))
        {
            sfx.Pause();
            return;
        }
        Logger.Error($"SFX '{sfxName}' cannot be paused as it cannot be found.");
    }

    public static void Resume(string sfxName) 
    {
        if (SoundsLoaded.TryGetValue(sfxName, out var sfx))
        {
            sfx.Resume();
            return;
        }
        Logger.Error($"SFX '{sfxName}' cannot be resumed as it cannot be found.");
    }

    public static void Stop(string sfxName, bool addToList = true) 
    {
        if (SoundsLoaded.TryGetValue(sfxName, out var sfx))
        {
            sfx.Stop(addToList);
            return;
        }
        Logger.Error($"SFX '{sfxName}' cannot be stopped as it cannot be found.");
    }
}
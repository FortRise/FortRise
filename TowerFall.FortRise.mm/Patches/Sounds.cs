using System;
using System.Collections.Generic;
using FortRise;
using Monocle;

namespace TowerFall;

public static class patch_Sounds 
{
    public static Dictionary<string, SFX> SoundsLoaded { get; private set; } = new Dictionary<string, SFX>();

    public static void AddSFX(FortContent content, ReadOnlySpan<char> id, SFX sfx) 
    {
        ReadOnlySpan<char> name = content.ResourceSystem.Metadata.Name;

        var lookup = SoundsLoaded.GetAlternateLookup<ReadOnlySpan<char>>();
        lookup[$"{name}/{id}"] = sfx;
    }

    public static void Play(ReadOnlySpan<char> sfxName, float panX = 160f, float volume = 1f) 
    {
        var lookup = SoundsLoaded.GetAlternateLookup<ReadOnlySpan<char>>();
        if (lookup.TryGetValue(sfxName, out var sfx))
        {
            sfx.Play(panX, volume);
            return;
        }

        Logger.Error($"SFX '{sfxName}' cannot be played as it cannot be found.");
    }

    public static void Pause(ReadOnlySpan<char> sfxName) 
    {
        var lookup = SoundsLoaded.GetAlternateLookup<ReadOnlySpan<char>>();
        if (lookup.TryGetValue(sfxName, out var sfx))
        {
            sfx.Pause();
            return;
        }
        Logger.Error($"SFX '{sfxName}' cannot be paused as it cannot be found.");
    }

    public static void Resume(ReadOnlySpan<char> sfxName) 
    {
        var lookup = SoundsLoaded.GetAlternateLookup<ReadOnlySpan<char>>();
        if (lookup.TryGetValue(sfxName, out var sfx))
        {
            sfx.Resume();
            return;
        }
        Logger.Error($"SFX '{sfxName}' cannot be resumed as it cannot be found.");
    }

    public static void Stop(ReadOnlySpan<char> sfxName, bool addToList = true) 
    {
        var lookup = SoundsLoaded.GetAlternateLookup<ReadOnlySpan<char>>();
        if (lookup.TryGetValue(sfxName, out var sfx))
        {
            sfx.Stop(addToList);
            return;
        }
        Logger.Error($"SFX '{sfxName}' cannot be stopped as it cannot be found.");
    }

    public static void AddSFX(FortContent content, string id, SFX sfx) 
    {
        AddSFX(content, id, sfx);
    }

    public static void Play(string sfxName, float panX = 160f, float volume = 1f) 
    {
        Play(sfxName, panX, volume);
    }

    public static void Pause(string sfxName) 
    {
        Pause(sfxName);
    }

    public static void Resume(string sfxName) 
    {
        Resume(sfxName);
    }

    public static void Stop(string sfxName, bool addToList = true) 
    {
        Stop(sfxName, addToList);
    }
}
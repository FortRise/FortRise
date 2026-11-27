using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public static class patch_Sounds
{
    private static List<IBaseSFXEntry> queuedModdedSFXs = new List<IBaseSFXEntry>();
    public static Dictionary<string, SFX> SoundsLoaded { get; private set; } = new Dictionary<string, SFX>();

    public static Lock SoundLoadLock = new Lock();

    [Postfix("Load")]
    private static void AddAllSFXToSoundsLoaded()
    {
        FieldInfo[] array = typeof(Sounds).GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (var field in array)
        {
            if (field.GetValue(null) is SFX sfx)
            {
                SoundsLoaded.TryAdd(field.Name, sfx);
            }
        }
    }

    public static void LoadModdedCharacterSounds()
    {
        foreach (var (sfxID, sounds) in CharacterSoundsRegistry.ModdedSounds)
        {
            SetCharacterSounds(sfxID, sounds);
        }

        CharacterSoundsRegistry.Injected = true;
    }

    internal static void SetCharacterSounds(int sfxID, CharacterSounds sounds)
    {
        var characters = Sounds.Characters;
        if (sfxID >= characters.Length)
        {
            int oldLength = characters.Length;
            Array.Resize(ref characters, sfxID + 1);

            for (int i = oldLength; i < characters.Length; i++)
            {
                characters[i] ??= characters[0];
            }

            Sounds.Characters = characters;
        }

        characters[sfxID] = sounds;
    }

    internal static void AddSFX(IBaseSFXEntry entry)
    {
        //queuedModdedSFXs.Add(entry);

        ReadOnlySpan<char> name = entry.Name;

        var lookup = SoundsLoaded.GetAlternateLookup<ReadOnlySpan<char>>();
        lookup[name] = entry.BaseSFX;
    }

    internal static void RemoveSFX(IBaseSFXEntry entry)
    {
        SoundsLoaded.Remove(entry.Name);
    }

    public static void AddSFX(ModuleMetadata metadata, ReadOnlySpan<char> id, SFX sfx)
    {
        ReadOnlySpan<char> name = metadata.Name;

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

        PlayFromVanilla(sfxName);
    }

    // TODO: optimize this method, probably using a generator?
    private static void PlayFromVanilla(ReadOnlySpan<char> sfxName, float panX = 160f, float volume = 1f)
    {
        var sfxNameStr = sfxName.ToString();

        var sfx = typeof(Sounds).GetField(sfxNameStr);
        if (sfx is null)
        {
            Logger.Error($"SFX '{sfxName}' cannot be played as it cannot be found.");
            return;
        }
        sfx.DeclaringType.GetMethod("Play").Invoke(sfx.GetValue(null), [panX, volume]);
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

    public static void Play(string sfxName, float panX = 160f, float volume = 1f)
    {
        Play(sfxName.AsSpan(), panX, volume);
    }

    public static void Pause(string sfxName)
    {
        Pause(sfxName.AsSpan());
    }

    public static void Resume(string sfxName)
    {
        Resume(sfxName.AsSpan());
    }

    public static void Stop(string sfxName, bool addToList = true)
    {
        Stop(sfxName.AsSpan(), addToList);
    }
}

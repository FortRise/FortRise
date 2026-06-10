#nullable enable
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Monocle;
using TowerFall;

namespace FortRise;

internal static class SFXRegistry
{
    private static Dictionary<string, IBaseSFXEntry> cachedVanillaEntry = [];
    private static Dictionary<string, IBaseSFXEntry> sfxEntries = [];

    public static T? CreateVanillaEntry<T>(string id)
    where T : IBaseSFXEntry
    {
        ref var cached = ref CollectionsMarshal.GetValueRefOrAddDefault(cachedVanillaEntry, id, out bool exists);

        if (exists)
        {
            return (T?)cached;
        }

        var sfx = typeof(Sounds).GetField(id);

        if (sfx is null)
        {
            return default;
        }

        var type = sfx.FieldType;

        if (type == typeof(SFX))
        {
            ISFXEntry entry = new SFXEntry(id, () => (SFX)sfx.GetValue(null)!, false);
            cachedVanillaEntry[id] = entry;
            return (T)entry;
        }

        if (type == typeof(SFXInstanced))
        {
            ISFXInstancedEntry entry = new SFXInstancedEntry(id, () => (patch_SFXInstanced)sfx.GetValue(null)!, 0, false);
            cachedVanillaEntry[id] = entry;
            return (T)entry;
        }

        if (type == typeof(SFXLooped))
        {
            ISFXLoopedEntry entry = new SFXLoopedEntry(id, () => (patch_SFXLooped)sfx.GetValue(null)!, false);
            cachedVanillaEntry[id] = entry;
            return (T)entry;
        }

        if (type == typeof(SFXLooped))
        {
            ISFXVariedEntry entry = new SFXVariedEntry(id, () => (patch_SFXVaried)sfx.GetValue(null)!, false);
            cachedVanillaEntry[id] = entry;
            return (T)entry;
        }

        return default;
    }

    public static void AddSFX(IBaseSFXEntry entry)
    {
        sfxEntries[entry.Name] = entry;
    }

    public static T? GetSFX<T>(string id)
    where T : IBaseSFXEntry
    {
        sfxEntries.TryGetValue(id, out var entry);
        return (T?)entry;
    }
}
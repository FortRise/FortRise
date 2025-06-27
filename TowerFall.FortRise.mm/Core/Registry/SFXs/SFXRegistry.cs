#nullable enable
using System.Collections.Generic;

namespace FortRise;

internal static class SFXRegistry
{
    private static Dictionary<string, IBaseSFXEntry> sfxEntries = [];

    public static void AddSFX(ISFXEntry entry)
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
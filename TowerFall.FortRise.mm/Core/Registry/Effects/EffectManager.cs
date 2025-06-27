#nullable enable
using System.Collections.Generic;

namespace FortRise;

public static class EffectManager
{
    private static Dictionary<string, IEffectEntry> effectEntries = new();
    public static Dictionary<string, EffectResource> Shaders = new();

    public static void AddEffect(IEffectEntry entry)
    {
        effectEntries[entry.ID] = entry;
    }

    public static IEffectEntry? GetEffect(string id)
    {
        effectEntries.TryGetValue(id, out var entry);
        return entry;
    }
}
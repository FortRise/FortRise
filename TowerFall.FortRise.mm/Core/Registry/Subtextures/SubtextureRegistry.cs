#nullable enable
using System.Collections.Generic;

namespace FortRise;

internal static class SubtextureRegistry
{
    private static Dictionary<string, ISubtextureEntry> subtextureEntries = [];
    private static Dictionary<string, ISubtextureEntry> menuSubtextureEntries = [];
    private static Dictionary<string, ISubtextureEntry> bgSubtextureEntries = [];
    private static Dictionary<string, ISubtextureEntry> bossSubtextureEntries = [];

    public static void AddSubtexture(ISubtextureEntry entry, SubtextureAtlasDestination destination)
    {
        switch (destination)
        {
            case SubtextureAtlasDestination.Atlas:
                subtextureEntries[entry.ID] = entry;
                break;
            case SubtextureAtlasDestination.BGAtlas:
                bgSubtextureEntries[entry.ID] = entry;
                break;
            case SubtextureAtlasDestination.MenuAtlas:
                menuSubtextureEntries[entry.ID] = entry;
                break;
            case SubtextureAtlasDestination.BossAtlas:
                bossSubtextureEntries[entry.ID] = entry;
                break;
        }
    }

    public static ISubtextureEntry? GetSubtexture(string id, SubtextureAtlasDestination destination)
    {
        ISubtextureEntry? entry;
        switch (destination)
        {
            case SubtextureAtlasDestination.Atlas:
                subtextureEntries.TryGetValue(id, out entry);
                break;
            case SubtextureAtlasDestination.BGAtlas:
                bgSubtextureEntries.TryGetValue(id, out entry);
                break;
            case SubtextureAtlasDestination.MenuAtlas:
                menuSubtextureEntries.TryGetValue(id, out entry);
                break;
            default:
                bossSubtextureEntries.TryGetValue(id, out entry);
                break;
        }
        return entry;
    }
}
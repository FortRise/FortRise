#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise;

internal static class SubtextureRegistry
{
    private static readonly Dictionary<string, ISubtextureEntry> subtextureEntries = [];
    private static readonly Dictionary<string, ISubtextureEntry> menuSubtextureEntries = [];
    private static readonly Dictionary<string, ISubtextureEntry> bgSubtextureEntries = [];
    private static readonly Dictionary<string, ISubtextureEntry> bossSubtextureEntries = [];
    private static readonly Dictionary<string, ISubtextureEntry> cachedVanillaSubtextureEntries = [];

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

    public static IReadOnlyList<ISubtextureEntry> GetAllSubtextures(SubtextureAtlasDestination destination) 
    {
        var list = new List<ISubtextureEntry>();
        var entries = destination switch
        {
            SubtextureAtlasDestination.Atlas => subtextureEntries,
            SubtextureAtlasDestination.MenuAtlas => menuSubtextureEntries,
            SubtextureAtlasDestination.BGAtlas => bgSubtextureEntries,
            SubtextureAtlasDestination.BossAtlas => bossSubtextureEntries,
            _ => throw new NotImplementedException(),
        };       

        var vanilla = cachedVanillaSubtextureEntries
            .Select(x => x.Value)
            .Where(x => x.AtlasDestination == destination);

        list.AddRange(entries.Values);
        list.AddRange(vanilla);

        return list;
    }

    public static ISubtextureEntry? GetSubtexture(string id, SubtextureAtlasDestination destination)
    {
        var entries = destination switch
        {
            SubtextureAtlasDestination.Atlas => subtextureEntries,
            SubtextureAtlasDestination.MenuAtlas => menuSubtextureEntries,
            SubtextureAtlasDestination.BGAtlas => bgSubtextureEntries,
            SubtextureAtlasDestination.BossAtlas => bossSubtextureEntries,
            _ => throw new NotImplementedException(),
        };

        ref var entry = ref CollectionsMarshal.GetValueRefOrNullRef(entries, id);

        if (Unsafe.IsNullRef(ref entry))
        {
            return null;
        }

        return entry;
    }

    private static HashSet<string>? atlasIDCached;
    private static readonly Dictionary<string, XmlDocument> xmls = [];
    
    private static XmlDocument? LoadXml(string xmlPath)
    {
        if (!File.Exists(xmlPath))
        {
            return null; 
        }

        ref var col = ref CollectionsMarshal.GetValueRefOrAddDefault(xmls, xmlPath, out bool exists);
        if (exists)
        {
            return col;
        }

        return Calc.LoadXML(xmlPath);
    }

    public static ISubtextureEntry? GetVanillaSubtextureEntry(string id, SubtextureAtlasDestination destination)
    {
        ref var cached = ref CollectionsMarshal.GetValueRefOrAddDefault(cachedVanillaSubtextureEntries, id, out bool exists);

        if (exists)
        {
            return cached;
        }

        var getAll = GetAllAvailableSubtexturesID();

        if (!getAll.Contains(id))
        {
            return null;
        }

        return cached = destination switch 
        {
            SubtextureAtlasDestination.Atlas => new SubtextureEntry(id, () => TFGame.Atlas[id], destination),
            SubtextureAtlasDestination.BGAtlas => new SubtextureEntry(id, () => TFGame.BGAtlas[id], destination),
            SubtextureAtlasDestination.MenuAtlas => new SubtextureEntry(id, () => TFGame.MenuAtlas[id], destination), 
            SubtextureAtlasDestination.BossAtlas => new SubtextureEntry(id, () => TFGame.BossAtlas[id], destination),
            _ => throw new NotImplementedException()
        };
    }

    private static bool CheckDarkWorld()
    {
        return File.Exists("DarkWorldContent/Atlas/atlas.png");
    }

    private static HashSet<string> GetAllAvailableSubtexturesID() 
    {
        if (atlasIDCached is not null)
        {
            return atlasIDCached;
        }
        atlasIDCached = [];
        string atlasPath;
        string bossAtlasPath;

        if (CheckDarkWorld()) 
        {
            atlasPath = "DarkWorldContent/Atlas/atlas.xml";
            bossAtlasPath = "DarkWorldContent/Atlas/bossAtlas.xml";
        }
        else 
        {
            atlasPath = "Content/Atlas/atlas.xml";
            bossAtlasPath = "Content/Atlas/bossAtlas.xml";
        }

        string menuAtlasPath = "Content/Atlas/menuAtlas.xml";
        string bgAtlasPath = "Content/Atlas/bgAtlas.xml";

        var atlasDoc = LoadXml(atlasPath)!["TextureAtlas"];
        foreach (XmlElement elm in atlasDoc!.GetElementsByTagName("SubTexture"))
        {
            atlasIDCached.Add(elm.Attr("name"));
        }

        atlasDoc = LoadXml(bgAtlasPath)!["TextureAtlas"];
        foreach (XmlElement elm in atlasDoc!.GetElementsByTagName("SubTexture"))
        {
            atlasIDCached.Add(elm.Attr("name"));
        }

        atlasDoc = LoadXml(bossAtlasPath)!["TextureAtlas"];
        foreach (XmlElement elm in atlasDoc!.GetElementsByTagName("SubTexture"))
        {
            atlasIDCached.Add(elm.Attr("name"));
        }

        atlasDoc = LoadXml(menuAtlasPath)!["TextureAtlas"];
        foreach (XmlElement elm in atlasDoc!.GetElementsByTagName("SubTexture"))
        {
            atlasIDCached.Add(elm.Attr("name"));
        }

        return atlasIDCached;
    }
}

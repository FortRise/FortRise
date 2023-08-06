using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FortRise;
using FortRise.Adventure;
using Ionic.Zip;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;
using TeuJson;

namespace TowerFall;

public static class patch_GameData 
{
    public static Dictionary<string, TilesetData> CustomTilesets;
    public static Dictionary<Guid, CustomBGStorage> CustomBGAtlas;
    public static List<string> AdventureWorldCategories;
    // TODO clean up
    public static Dictionary<string, int> AdventureWorldModTowersLookup;
    public static List<(bool contains, CustomMapRenderer renderer)> AdventureWorldMapRenderer;
    public static List<List<AdventureWorldTowerData>> AdventureWorldModTowers;

    public static extern void orig_Load();

    public static void Load() 
    {
        RiseCore.Events.Invoke_OnBeforeDataLoad();
        orig_Load();
        RiseCore.GameData.Load();

        // Assign its LevelID
        foreach (var darkWorldTowers in GameData.DarkWorldTowers) 
        {
            darkWorldTowers.SetLevelID("TowerFall");
        }
        TFGame.WriteLineToLoadLog("Loading Adventure World Tower Data...");

        TowerRegistry.LoadQuest();
        TowerRegistry.LoadDarkWorld();

        // patch_DarkWorldTowerData.Load();
        // ReloadCustomTowers();
        TFGame.WriteLineToLoadLog("  " + TowerRegistry.DarkWorldTowerSets.Count + " loaded");
        patch_MapScene.FixedStatic();
        RiseCore.Events.Invoke_OnAfterDataLoad();

        CustomBGAtlas ??= new();
        CustomTilesets ??= new();

        CustomBGAtlas.Clear();
        CustomTilesets.Clear();

        AdventureWorldModTowersLookup ??= new();
        AdventureWorldModTowersLookup.Clear();

        AdventureWorldModTowers ??= new();
        AdventureWorldModTowers.Clear();

        AdventureWorldMapRenderer ??= new();
        AdventureWorldMapRenderer.Clear();

        AdventureWorldCategories ??= new();
        AdventureWorldCategories.Clear();
    }

    /// <summary>
    /// Reload custom adventure towers.
    /// </summary>
    public static void ReloadCustomTowers() 
    {
        if (CustomTilesets != null) 
        {
            foreach (var tileset in CustomTilesets) 
            {
                tileset.Value.Texture.Texture2D.Dispose();
            }
        }

        CustomBGAtlas ??= new();
        CustomTilesets ??= new();

        CustomBGAtlas.Clear();
        CustomTilesets.Clear();
        // AdventureWorldTowers ??= new();
        // AdventureWorldTowers.Clear();

        AdventureWorldModTowersLookup ??= new();
        AdventureWorldModTowersLookup.Clear();

        AdventureWorldModTowers ??= new();
        AdventureWorldModTowers.Clear();

        AdventureWorldMapRenderer ??= new();
        AdventureWorldMapRenderer.Clear();

        AdventureWorldCategories ??= new();
        AdventureWorldCategories.Clear();

        const string AdventureModPath = "Content/Mod/Adventure/DarkWorld";
        if (!Directory.Exists(AdventureModPath))
            Directory.CreateDirectory(AdventureModPath);

        var contentModDirectories = new List<string>(Directory.EnumerateDirectories(AdventureModPath));
        contentModDirectories.InsertRange(0, AdventureModule.SaveData.LevelLocations);

        // if (Directory.Exists("AdventureWorldContent/Levels")) 
        // {
        //     Logger.Warning("AdventureWorldContent path is obsolete! Use DLL-Less Mods using Mods folder or Load it inside of Content/Mod/Adventure/DarkWorld instead");
        //     contentModDirectories.AddRange(Directory.EnumerateDirectories("AdventureWorldContent/Levels"));
        // }
        RiseCore.ResourceTree.AddMod(null, new RiseCore.AdventureGlobalLevelResource());

        // foreach (var adventurePath in contentModDirectories) 
        // {
        //     var file = adventurePath.Replace('\\', '/');
        //     var globalResource = new RiseCore.GlobalLevelResource(file, "::global::/" + file);
        //     RiseCore.Resources.GlobalResources.Add("::global/::" + file, globalResource);
        //     // LoadAdventureModTowers(globalResource);
        // }


        // AdventureWorldMapRenderer.Add((false, null));

        // Load mods that contains Levels/DarkWorld folder
        foreach (var levelMod in RiseCore.ResourceTree.TreeMap.Where(
            level => level.Value.Path is "Content/Levels/DarkWorld" or "Content/Mod/DarkWorld"))
        {
            var resource = levelMod.Value;
            
            foreach (var dir in resource.Childrens)
            {
                LoadAdventureModTowers(dir);
            }
            if (!RiseCore.ResourceTree.TreeMap.TryGetValue(resource.Root + "Content/Levels/map.xml", out var mapXml)) 
            {
                AdventureWorldMapRenderer.Add((false, null));
                continue;
            }

            using var mapXmlStream = mapXml.Stream;

            var xmlMapRenderer = new XmlMapRenderer(mapXmlStream, resource);
            AdventureWorldMapRenderer.Add((true, xmlMapRenderer));
        }

        // if (AdventureWorldModTowers.Count > 0)
        //     AdventureWorldTowers = AdventureWorldModTowers[0];
    }

    /// <summary>
    /// Load Adventure towers by directory, and specify its metadata or null if it's global.
    /// </summary>
    /// <param name="resource">A resource to find and locate level</param>
    /// <returns>A boolean determines whether the load success or fails</returns>
    public static bool LoadAdventureModTowers(RiseCore.Resource resource) 
    {
        string modName = resource.Root;
        var directory = resource.Path;

        if (AdventureWorldModTowersLookup.TryGetValue(modName, out int id))
        {
            var tower = AdventureWorldModTowers[id];
            var adventureTowerDataOnCache = new AdventureWorldTowerData(resource);
            if (adventureTowerDataOnCache.ModAdventureLoad(tower.Count, directory)) 
            {
                AdventureWorldModTowers[id].Add(adventureTowerDataOnCache);
                Logger.Verbose($"[Adventure] Added {directory} tower to {modName}.");
                return true;
            }
            return false;
        }
        var lookup = AdventureWorldModTowers.Count;
        AdventureWorldCategories.Add(modName);
        AdventureWorldModTowersLookup.Add(modName, lookup);

        var adventureTowerData = new AdventureWorldTowerData(resource);
        // if (adventureTowerData.ModAdventureLoad(AdventureWorldTowers.Count, directory)) 
        // {
        //     AdventureWorldModTowers.Add(new List<AdventureWorldTowerData>() { adventureTowerData });
        //     Logger.Verbose($"[Adventure] Added {directory} tower to {modName}.");
        //     return true;
        // }
        return false;
    }

    /// <summary>
    /// Load Adventure towers by directory, and specify its metadata or null if it's global.
    /// </summary>
    /// <param name="directory">A directory path to the levels</param>
    /// <param name="mod">A mod metadata or null to categorize the level</param>
    /// <param name="system">A callback to initialize the ResourceSystem which will be used to manage the files</param>
    /// <returns>A boolean determines whether the load success or fails</returns>
    public static bool LoadAdventureTowers(string directory, ModuleMetadata mod, Func<RiseCore.ModResource> system = null) 
    {
        system = system == null ? () => new RiseCore.FolderModResource(directory) : system;
        string modName = mod == null ? "::global::" : mod.Name;
        if (AdventureWorldModTowersLookup.TryGetValue(modName, out int id))
        {
            var tower = AdventureWorldModTowers[id];
            var adventureTowerDataOnCache = new AdventureWorldTowerData(system(), directory);
            if (adventureTowerDataOnCache.AdventureLoad(tower.Count, directory)) 
            {
                AdventureWorldModTowers[id].Add(adventureTowerDataOnCache);
                Logger.Verbose($"[Adventure] Added {directory} tower to {modName}.");
                return true;
            }
            return false;
        }


        var adventureTowerData = new AdventureWorldTowerData(system(), directory);
        // if (adventureTowerData.AdventureLoad(AdventureWorldTowers.Count, directory)) 
        // {
        //     var lookup = AdventureWorldModTowers.Count;
        //     AdventureWorldCategories.Add(modName);
        //     AdventureWorldModTowersLookup.Add(modName, lookup);
        //     AdventureWorldModTowers.Add(new List<AdventureWorldTowerData>() { adventureTowerData });
        //     Logger.Verbose($"[Adventure] Added {directory} tower to {modName}.");
        //     return true;
        // }
        return false;
    }
}


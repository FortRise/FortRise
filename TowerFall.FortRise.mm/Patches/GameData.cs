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

        TFGame.WriteLineToLoadLog("  " + TowerRegistry.DarkWorldTowerSets.Count + " loaded");
        patch_MapScene.FixedStatic();
        RiseCore.Events.Invoke_OnAfterDataLoad();
    }
}


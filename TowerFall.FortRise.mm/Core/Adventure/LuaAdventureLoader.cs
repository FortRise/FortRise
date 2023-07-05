using System;
using System.Collections.Generic;
using System.IO;
using NLua;
using TowerFall;

namespace FortRise.Adventure;

public sealed class LuaAdventureLoader : IAdventureTowerLoader<NLua.LuaTable>
{
    public string FileExtension => "lua";

    public AdventureTowerInfo Load(int id, Stream stream, string levelDirectory, string directoryPrefix, bool customIcons)
    {
        var info = new AdventureTowerInfo();

        info.StoredDirectory = levelDirectory;
        info.ID = id;
        var luaTable = (LuaTable)RiseCore.Lua.LoadScript(stream, "tower").Call()[0];
        info.Theme = luaTable.Contains("theme") ? new patch_TowerTheme(luaTable.GetTable("theme")) : patch_TowerTheme.GetDefault();
        info.Author = luaTable.Contains("author") ? luaTable.Get("author") : string.Empty;
        info.Stats = AdventureModule.SaveData.AdventureWorld.AddOrGet(info.Theme.Name, levelDirectory);
        info.Extras = LoadExtraData(luaTable);

        var guid = (info.Theme as patch_TowerTheme).GenerateThemeID();

        info.TimeBase = luaTable.GetTable("time")?.GetInt("base", 300) ?? 300;
        info.TimeAdd = luaTable.GetTable("time")?.GetInt("add", 40) ?? 40;
        info.EnemySets = new Dictionary<string, List<DarkWorldTowerData.EnemyData>>();

        var enemiesDict = RiseCore.Lua.Context.GetTableDict(luaTable.GetTable("enemies"));
        foreach (var obj in enemiesDict)
        {
            var unboxed = (LuaTable)obj.Value;
            string key = (string)unboxed["id"];
            LuaTable set = (LuaTable)unboxed["set"];
            List<DarkWorldTowerData.EnemyData> list = new List<DarkWorldTowerData.EnemyData>();
            
            var spawnDict = RiseCore.Lua.Context.GetTableDict(set.GetTable("spawn"));
            foreach (var obj2 in spawnDict)
            {
                list.Add(new patch_DarkWorldTowerData.patch_EnemyData((LuaTable)obj2.Value));
            }
            info.EnemySets.Add(key, list);
        }
        info.Normal = LoadLevelSet(luaTable.GetTable("normal"), info.EnemySets);
        info.Hardcore = LoadLevelSet(luaTable.GetTable("hardcore"), info.EnemySets);
        info.Legendary = LoadLevelSet(luaTable.GetTable("legendary"), info.EnemySets);
        if (luaTable.Contains("required"))
            info.RequiredMods = luaTable.GetStringArray("required");
        else
            info.RequiredMods = Array.Empty<string>();

        return info;
    }

    public ExtraAdventureTowerInfo LoadExtraData(LuaTable data)
    {
        var info = new ExtraAdventureTowerInfo();
        if (data.TryGetInt("lives", out var lives)) 
        {
            info.StartingLives = lives;
        }
        if (data.TryGetBool("procedural", out bool procedural))
            info.Procedural = procedural;
        if (data.TryGetTable("continues", out LuaTable continues)) 
        {
            if (continues.TryGetInt("normal", out var normal))
                info.NormalContinues = normal;
            if (continues.TryGetInt("hardcore", out var hardcore))
                info.HardcoreContinues = hardcore;
            if (continues.TryGetInt("legendary", out var legendary))
                info.LegendaryContinues = legendary;
        }
        return info;
    }

    public List<DarkWorldTowerData.LevelData> LoadLevelSet(LuaTable data, Dictionary<string, List<DarkWorldTowerData.EnemyData>> enemySets)
    {
        List<DarkWorldTowerData.LevelData> list = new List<DarkWorldTowerData.LevelData>();
        var levelSetDict = RiseCore.Lua.Context.GetTableDict(data.GetTable("level"));

        foreach (var obj in levelSetDict)
        {
            list.Add(new patch_DarkWorldTowerData.patch_LevelData(obj.Value as LuaTable, enemySets));
        }
        list[list.Count - 1].FinalLevel = true;
        return list;
    }
}

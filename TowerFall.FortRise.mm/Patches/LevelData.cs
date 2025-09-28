using System;
using MonoMod;

namespace TowerFall;

public class patch_LevelData : LevelData
{
    public string Author;
    public bool Procedural;
    public string LevelID;
    internal string InternalTowerSet;
    [Obsolete("Use TowerSet instead")]
    public string LevelSet => TowerSet;

    public string TowerSet
    {
        get
        {
            return InternalTowerSet;
        }
    }

    [MonoModConstructor]
    public void ctor()
    {
        Author = "";
    }

    [MonoModIgnore]
    public extern override string[] GetLevelFiles();

    [MonoModIgnore]
    public extern override LevelSystem GetLevelSystem();
}

public static class LevelDataExt 
{
    extension(LevelData data)
    {
        public string LevelID
        {
            get => ((patch_LevelData)data).LevelID ?? "::UNKNOWN::";
            set => ((patch_LevelData)data).LevelID = value;
        }

        public string TowerSet
        {
            get => ((patch_LevelData)data).TowerSet;
            set => ((patch_LevelData)data).InternalTowerSet = value;
        }

        public bool IsOfficialTowerSet => ((patch_LevelData)data).TowerSet == "TowerFall";
    }


    // TODO: Deprecate this when .NET 10 comes out
    public static void SetLevelID(this LevelData data, string modID) 
    {
        ((patch_LevelData)data).LevelID = modID;
    }

    // TODO: Deprecate this when .NET 10 comes out
    public static void SetLevelSet(this LevelData data, string levelSet) 
    {
        ((patch_LevelData)data).InternalTowerSet = levelSet;
    }

    // TODO: Deprecate this when .NET 10 comes out
    public static bool IsOfficialLevelSet(this LevelData data) 
    {
        return ((patch_LevelData)data).TowerSet == "TowerFall";
    }

    // TODO: Deprecate this when .NET 10 comes out
    public static string GetLevelID(this LevelData data) 
    {
        return ((patch_LevelData)data).LevelID ?? "::UNKNOWN::";
    }

    // TODO: Deprecate this when .NET 10 comes out
    public static string GetLevelSet(this LevelData data) 
    {
        return ((patch_LevelData)data).TowerSet;
    }
}

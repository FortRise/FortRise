using System;
using MonoMod;

namespace TowerFall;

public class patch_LevelData : LevelData
{
    public string Author;
    public bool Procedural;
    public string LevelID;
    internal string InternalLevelSet;
    public string LevelSet 
    {
        get
        {
            return InternalLevelSet;
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
    public static void SetLevelID(this LevelData data, string modID) 
    {
        ((patch_LevelData)data).LevelID = modID;
    }

    public static void SetLevelSet(this LevelData data, string levelSet) 
    {
        ((patch_LevelData)data).InternalLevelSet = levelSet;
    }

    public static bool IsOfficialLevelSet(this LevelData data) 
    {
        return ((patch_LevelData)data).LevelSet == "TowerFall";
    }

    public static string GetLevelID(this LevelData data) 
    {
        return ((patch_LevelData)data).LevelID ?? "::UNKNOWN::";
    }

    public static string GetLevelSet(this LevelData data) 
    {
        return ((patch_LevelData)data).LevelSet;
    }
}
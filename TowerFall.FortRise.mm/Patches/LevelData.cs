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
            if (string.IsNullOrEmpty(InternalLevelSet))
                return "TowerFall";
            ReadOnlySpan<char> setSpan = LevelID.AsSpan();
            if (setSpan[0] == 'm' && setSpan[1] == 'o' && setSpan[2] == 'd' && setSpan[3] == ':') 
            {
                setSpan = setSpan.Slice(0, 4);
            }
            int indexOfSlash = setSpan.IndexOf('/');
            
            if (indexOfSlash == -1)
                return InternalLevelSet = "UNCATEGORIZED";
            
            var levelSet = setSpan.Slice(0, indexOfSlash);
            
            return InternalLevelSet = levelSet.ToString();
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
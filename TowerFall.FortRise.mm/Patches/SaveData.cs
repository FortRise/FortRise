using System.IO;
using FortRise;
using TeuJson;

namespace TowerFall;

public class patch_SaveData : SaveData
{
    public static bool AdventureActive;

    public extern string orig_Save();

    public string Save() 
    {
        WorldSaveData.Save(WorldSaveData.SavePath);
        foreach (var module in RiseCore.InternalModules)
        {
            module.SaveData();
        }
        return orig_Save();
    }

    public static extern string orig_Load();

    public static string Load() 
    {
        foreach (var module in RiseCore.InternalModules)
        {
            module.LoadData();
        }
        return orig_Load();
    }
}


public class WorldSaveData
{
    public const string SavePath = "AdventureWorldContent/tfa_saveData.json";
    public AdventureWorldStats AdventureWorld = new AdventureWorldStats();

    // SOME CRIME THAT I HAD TO MAKE
    public static WorldSaveData Instance;

    public static void Load(string path) 
    {
        Instance ??= new WorldSaveData();
        if (File.Exists(path))
            Instance.AdventureWorld = JsonConvert.DeserializeFromFile<AdventureWorldStats>(path);
    }

    public static void Save(string path) 
    {
        JsonConvert.SerializeToFile(Instance.AdventureWorld, path);
    }
}
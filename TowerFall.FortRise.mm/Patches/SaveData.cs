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

    public extern void orig_Verify();

    public void Verify() 
    {
        orig_Verify();
        foreach (var module in RiseCore.InternalModules) 
        {
            module.VerifyData();
        }
    }
}

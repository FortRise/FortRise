using FortRise;

namespace TowerFall;

public class patch_SaveData : SaveData
{
    public extern string orig_Save();

    public string Save() 
    {
        foreach (var module in RiseCore.InternalFortModules)
        {
            module.SaveData();
        }
        return orig_Save();
    }

    public static extern string orig_Load();

    public static string Load() 
    {
        var error = orig_Load();
        foreach (var module in RiseCore.InternalFortModules)
        {
            module.LoadData();
        }
        return error;
    }

    public extern void orig_Verify();

    public void Verify() 
    {
        orig_Verify();
        foreach (var module in RiseCore.InternalFortModules) 
        {
            module.VerifyData();
        }
    }
}

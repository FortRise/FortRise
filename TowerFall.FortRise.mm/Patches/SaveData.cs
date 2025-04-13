using FortRise;
using MonoMod;

namespace TowerFall;

public class patch_SaveData : SaveData
{
    [PatchSDL2ToSDL3]
    public extern string orig_Save();

    public string Save() 
    {
        foreach (var module in RiseCore.ModuleManager.InternalFortModules)
        {
            module.SaveData();
        }
        return orig_Save();
    }

    [PatchSDL2ToSDL3]
    public static extern string orig_Load();

    public static string Load() 
    {
        var error = orig_Load();
        foreach (var module in RiseCore.ModuleManager.InternalFortModules)
        {
            module.LoadData();
        }
        return error;
    }

    public extern void orig_Verify();

    public void Verify() 
    {
        orig_Verify();
        foreach (var module in RiseCore.ModuleManager.InternalFortModules) 
        {
            module.VerifyData();
        }
    }
}

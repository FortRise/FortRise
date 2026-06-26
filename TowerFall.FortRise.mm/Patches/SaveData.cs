using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_SaveData : SaveData
{
    public GamepadConfig[] Gamepad;

    [PatchSDL2ToSDL3]
    public extern string orig_Save();

    public string Save() 
    {
        ModEventsManager.Instance.OnBeforeSaveSaveData.Raise(this, new());
        foreach (var module in RiseCore.ModuleManager.InternalFortModules)
        {
            module.SaveSaveData();
            module.SaveSettings();
        }

        string result = orig_Save();
        ModEventsManager.Instance.OnAfterSaveSaveData.Raise(this, new(result));
        return result;
    }

    [PatchSDL2ToSDL3]
    public static extern string orig_Load();

    public static string Load() 
    {
        var error = orig_Load();
        if (((patch_SaveData)Instance).Gamepad == null)
        {
            ((patch_SaveData)Instance).Gamepad = GamepadConfig.GetDefaults();
        }

        foreach (var module in RiseCore.ModuleManager.InternalFortModules)
        {
            var saveData = module.CreateSaveData();
            if (saveData != null)
            {
                module.LoadSaveData(saveData.GetType());
            }

            var settings = module.CreateSettings();
            if (settings != null)
            {
                module.LoadSettings(settings.GetType());
            }
        }
        return error;
    }

    public extern void orig_Verify();

    public void Verify() 
    {
        orig_Verify();
        Gamepad = Gamepad.VerifyLength(4);
        for (int i = 0; i < Gamepad.Length; i += 1)
        {
            if (Gamepad[i] is null)
            {
                Gamepad[i] = GamepadConfig.GetDefault();
            }
        }
        

        Verified = true;
        foreach (var module in RiseCore.ModuleManager.InternalFortModules) 
        {
            module.VerifySaveData();
            module.VerifySettings();
        }
    }
}

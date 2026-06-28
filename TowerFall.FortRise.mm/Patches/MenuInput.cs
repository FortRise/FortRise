using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.MenuInput")]
public static class StaticMenuInput 
{
    public static bool Arrows
    {
        get
        {
            for (int i = 0; i < MenuInput.MenuInputs.Length; i++)
            {
                if (MenuInput.MenuInputs[i] != null && ((patch_PlayerInput)MenuInput.MenuInputs[i]).get_MenuArrows_base())
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static bool ArrowsCheck
    {
        get
        {
            for (int i = 0; i < MenuInput.MenuInputs.Length; i++)
            {
                if (MenuInput.MenuInputs[i] != null && ((patch_PlayerInput)MenuInput.MenuInputs[i]).get_MenuArrowsCheck_base())
                {
                    return true;
                }
            }
            return false;
        }
    }
}

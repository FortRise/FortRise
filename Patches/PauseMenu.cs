#pragma warning disable CS0626
#pragma warning disable CS0108
using Microsoft.Xna.Framework;

namespace TowerFall;

public class patch_PauseMenu : PauseMenu
{
    private PauseMenu.MenuType menuType;


    public patch_PauseMenu(Level level, Vector2 position, MenuType menuType, int controllerDisconnected = -1) : base(level, position, menuType, controllerDisconnected)
    {
    }

    private extern void orig_DarkWorldMap();

    private void DarkWorldMap() 
    {
        patch_SaveData.AdventureActive = false;
        orig_DarkWorldMap();       
    }

    private extern void orig_Quit();

    private void Quit() 
    {
        if (menuType == MenuType.DarkWorldPause || 
        menuType == MenuType.DarkWorldComplete || 
        menuType == MenuType.DarkWorldGameOver)
            patch_SaveData.AdventureActive = false;
        orig_Quit();
    }
}
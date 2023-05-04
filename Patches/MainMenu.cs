#pragma warning disable CS0626
#pragma warning disable CS0108
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_MainMenu : MainMenu
{
    private float scrolling;
    private int totalScroll;
    private int count;
    public patch_MainMenu(MenuState state) : base(state)
    {
    }

    [MonoModIgnore]
    [PatchMainMenuCreateOptions]
    public extern void CreateOptions();

    private extern void orig_InitOptions(List<OptionsButton> buttons);

    private void InitOptions(List<OptionsButton> buttons) 
    {
        count = buttons.Count;
        orig_InitOptions(buttons);
    }

    public extern void orig_Update();

    public override void Update()
    {
        if (State == MenuState.Options) 
        {
            if (MenuInput.Up && totalScroll > 0) 
            {
                scrolling += FortRise.RiseCore.ScrollAmount;
                totalScroll--;
            }
            if (MenuInput.Down && totalScroll < count) 
            {
                scrolling -= FortRise.RiseCore.ScrollAmount; 
                totalScroll++;
            }
            if (totalScroll > 9 && totalScroll < count - 5) 
            {
                foreach (var menuItem in Layers[-1].GetList<MenuItem>()) 
                {
                    menuItem.Position.Y += scrolling;
                }
            }
            scrolling = 0;
        }
        else 
        {
            scrolling = 0;
            totalScroll = 0;
        }
        orig_Update();
    }

}
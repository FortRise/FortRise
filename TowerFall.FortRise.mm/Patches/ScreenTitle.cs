using System.Collections.Generic;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_ScreenTitle : ScreenTitle
{
    private Dictionary<patch_MainMenu.patch_MenuState, Subtexture> textures;
    public patch_ScreenTitle(MainMenu.MenuState state) : base(state)
    {
    }

    public extern void orig_ctor(patch_MainMenu.patch_MenuState state);

    [MonoModConstructor]
    public void ctor(patch_MainMenu.patch_MenuState state) 
    {
        orig_ctor(state);
        textures[patch_MainMenu.patch_MenuState.None] = null;
        textures[patch_MainMenu.patch_MenuState.Mods] = TFGame.MenuAtlas["menuTitles/options"];
        textures[patch_MainMenu.patch_MenuState.ModOptions] = TFGame.MenuAtlas["menuTitles/options"];
    }
}
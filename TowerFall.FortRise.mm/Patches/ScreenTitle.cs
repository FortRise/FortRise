using System.Collections.Generic;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_ScreenTitle : ScreenTitle
{
    private Dictionary<patch_MainMenu.MenuState, Subtexture> textures;
    public patch_ScreenTitle(MainMenu.MenuState state) : base(state)
    {
    }

    public extern void orig_ctor(patch_MainMenu.MenuState state);

    [MonoModConstructor]
    public void ctor(patch_MainMenu.MenuState state) 
    {
        orig_ctor(state);
        textures[patch_MainMenu.MenuState.None] = null;
        textures[ModRegisters.MenuState<UIModMenu>()] = TFGame.MenuAtlas["menuTitles/options"];
        textures[ModRegisters.MenuState<UIModOptions>()] = TFGame.MenuAtlas["menuTitles/options"];
        textures[ModRegisters.MenuState<UIModToggler>()] = TFGame.MenuAtlas["menuTitles/options"];
    }
}
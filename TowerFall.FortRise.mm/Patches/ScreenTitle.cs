using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_ScreenTitle : ScreenTitle
{
    private MainMenu.MenuState targetState;
    private Subtexture targetTexture;
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
        textures[ModRegisters.MenuState<UIModToggler>()] = TFGame.MenuAtlas["menuTitles/options"];
    }

    [MonoModReplace]
    public void ChangeState(MainMenu.MenuState state)
    {
        if (state != MainMenu.MenuState.Fade)
        {
            targetState = state;
            if (state == MainMenu.MenuState.Rollcall && MainMenu.RollcallMode == MainMenu.RollcallModes.Trials)
            {
                targetTexture = TFGame.MenuAtlas["menuTitles/trials"];
            }
            else if (state == MainMenu.MenuState.Rollcall && MainMenu.RollcallMode == MainMenu.RollcallModes.Quest)
            {
                targetTexture = TFGame.MenuAtlas["menuTitles/quest"];
            }
            else if (state == MainMenu.MenuState.Rollcall && MainMenu.RollcallMode == MainMenu.RollcallModes.DarkWorld)
            {
                targetTexture = TFGame.MenuAtlas["menuTitles/darkWorld"];
            }
            else
            {
                ref var texture = ref CollectionsMarshal.GetValueRefOrNullRef(textures, state);
                if (Unsafe.IsNullRef(ref texture))
                {
                    return;
                }
                targetTexture = texture;
            }
        }
    }
}
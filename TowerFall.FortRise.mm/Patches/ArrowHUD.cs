using System;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_ArrowHUD : ArrowHUD
{
    private Subtexture[] images;
    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        Array.Resize(ref images, Arrow.ARROW_TYPES + ArrowsRegistry.ArrowDatas.Count);
        foreach (var arrowObj in ArrowsRegistry.ArrowDatas.Values) 
        {
            var arrow = arrowObj.Types;
            var value = arrowObj.Hud;
            images[(int)arrow] = value ?? TFGame.Atlas["player/arrowHUD/arrow"];
        }
    }
}
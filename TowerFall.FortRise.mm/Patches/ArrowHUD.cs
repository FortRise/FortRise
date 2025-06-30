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
        Array.Resize(ref images, Arrow.ARROW_TYPES + ArrowsRegistry.GetArrowEntries().Count);
        foreach (var arrowObj in ArrowsRegistry.GetArrowEntries().Values) 
        {
            var arrow = arrowObj.ArrowTypes;
            var value = arrowObj.Configuration.HUD.Subtexture;
            images[(int)arrow] = value ?? TFGame.Atlas["player/arrowHUD/arrow"];
        }
    }
}
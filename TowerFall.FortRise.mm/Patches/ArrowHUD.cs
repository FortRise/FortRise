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
        Array.Resize(ref images, Arrow.ARROW_TYPES + RiseCore.ArrowsID.Count);
        foreach (var arrow in RiseCore.ArrowsID.Values) 
        {
            var loader = RiseCore.PickupGraphicArrows[arrow];
            var info = loader?.Invoke();
            if (info == null)
                return;
            var value = info.Value;
            images[(int)arrow] = value.HUD ?? TFGame.Atlas["player/arrowHUD/arrow"];
        }
    }
}
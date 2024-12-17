using System;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_EnemyArrowHUD : EnemyArrowHUD
{
    private Subtexture[] images;
    public patch_EnemyArrowHUD(Enemy enemy) : base(enemy)
    {
    }

    public extern void orig_ctor(Enemy enemy);

    [MonoModConstructor]
    public void ctor(Enemy enemy) 
    {
        orig_ctor(enemy);
        Array.Resize(ref images, Arrow.ARROW_TYPES + ArrowsRegistry.ArrowDatas.Count);
        foreach (var arrowObj in ArrowsRegistry.ArrowDatas.Values) 
        {
            var arrow = arrowObj.Types;
            var value = arrowObj.InfoLoader();
            images[(int)arrow] = value.HUD ?? TFGame.Atlas["player/arrowHUD/arrow"];
        }
    }
}
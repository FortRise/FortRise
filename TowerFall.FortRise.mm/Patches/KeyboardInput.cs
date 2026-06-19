using System;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.KeyboardInput")]
public class KeyboardInput : PlayerInputAbstract
{
    private Subtexture iconStart;
    private Subtexture iconJump;
    private Subtexture iconShoot;
    private Subtexture iconAltShoot;
    private Subtexture iconDodge;
    private Subtexture iconMenuAlt;
    private Subtexture iconLeft;
    private Subtexture iconRight;
    private Subtexture iconUp;
    private Subtexture iconDown;
    private Subtexture iconArrows;
    public KeyboardConfig Config;

    public override Subtexture ArrowsIcon => iconArrows;
    public override bool MenuArrows => MInput.Keyboard.Pressed(Config.Arrows);
    public override bool MenuArrowsCheck => MInput.Keyboard.Check(Config.Arrows);


    [MonoModReplace]
    private void InitIcons()
    {
        iconStart = GetIcon(Config.Start);
        iconJump = GetIcon(Config.Jump);
        iconShoot = GetIcon(Config.Shoot);
        iconAltShoot = GetIcon(Config.AltShoot);
        iconDodge = GetIcon(Config.Dodge);
        iconMenuAlt = GetIcon(Config.MenuAlt);
        iconLeft = GetIcon(Config.Left);
        iconRight = GetIcon(Config.Right);
        iconUp = GetIcon(Config.Up);
        iconDown = GetIcon(Config.Down);
        iconArrows = GetIcon(Config.Arrows);
    }

    [MonoModIgnore]
    private extern Subtexture GetIcon(Keys[] keys);
}

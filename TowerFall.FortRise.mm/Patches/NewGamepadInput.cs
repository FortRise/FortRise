using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

[MonoModIfFlag("OS:Windows")]
[MonoModRemove]
public class patch_NewGamepadInput : NewGamepadInput
{
    public patch_NewGamepadInput(int gamepadID) : base(gamepadID)
    {
    }
}
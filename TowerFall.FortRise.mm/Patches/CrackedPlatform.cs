using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_CrackedPlatform : CrackedPlatform
{
    public patch_CrackedPlatform(Vector2 position, int width) : base(position, width)
    {
    }

    [MonoModPublic]
    private enum patch_States {}
}
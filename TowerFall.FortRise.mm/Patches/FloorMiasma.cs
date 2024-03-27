using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_FloorMiasma : FloorMiasma 
{
    public patch_FloorMiasma(Vector2 position, int width, int group) : base(position, width, group)
    {
    }

    [MonoModPublic]
    private enum patch_States {}
}
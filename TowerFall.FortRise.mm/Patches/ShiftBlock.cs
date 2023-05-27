using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_ShiftBlock : ShiftBlock
{
    public patch_ShiftBlock(Vector2 position, int width, int height, Vector2 node) : base(position, width, height, node)
    {
    }

    [MonoModPublic]
    private enum patch_States {}
}
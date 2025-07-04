using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace TowerFall;

public static class patch_WrapMath
{
    // I don't want this specific method to get inlined
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Vector2 Opposite(Vector2 vec)
    {
        return new Vector2(160f + (160f - vec.X), vec.Y);
    }
}
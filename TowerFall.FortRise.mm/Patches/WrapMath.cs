using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.WrapMath")]
public static class WrapMath
{
    // I don't want this specific method to get inlined
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Vector2 Opposite(Vector2 vec)
    {
        return new Vector2(160f + (160f - vec.X), vec.Y);
    }
}
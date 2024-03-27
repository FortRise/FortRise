using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_InputTestScene : InputTestScene 
{
    // Completely removed in FNA
    [MonoModReplace]
    private void DrawGamepadData(Vector2 at, int id) {}
}
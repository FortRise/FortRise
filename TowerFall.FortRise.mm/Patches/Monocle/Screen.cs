using MonoMod;

namespace Monocle;

public class patch_Screen : Screen
{
    public patch_Screen(Engine engine, int width, int height, float scale) : base(engine, width, height, scale)
    {
    }

    [MonoModIgnore]
    [PatchScreenResize]
    public extern void Resize(int width, int height, float scale);
}
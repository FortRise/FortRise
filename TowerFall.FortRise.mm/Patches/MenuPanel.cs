using MonoMod;

namespace TowerFall;

public class patch_MenuPanel : MenuPanel
{
    public float Width { [MonoModIgnore] get; [MonoModIgnore] [MonoModPublic] set; }

    public float Height { [MonoModIgnore] get; [MonoModIgnore] [MonoModPublic] set; }

    public patch_MenuPanel(int width, int height) : base(width, height)
    {
    }
}
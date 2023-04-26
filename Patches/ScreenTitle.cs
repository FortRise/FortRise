#pragma warning disable CS0626
#pragma warning disable CS0108

using MonoMod;

namespace TowerFall;

public class patch_ScreenTitle : ScreenTitle
{
    public patch_ScreenTitle(MainMenu.MenuState state) : base(state)
    {
    }

    [PatchScreenTitleConstructor]
    [MonoModIgnore]
    public extern void ChangeState(MainMenu.MenuState state);
}
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.MenuButtons")]
public static class MenuButtons
{
    public static Subtexture Confirm;
    public static Subtexture Back;
    public static Subtexture Alt;
    public static Subtexture SkipReplay;
    public static Subtexture StartGame;
    public static Subtexture AltShoot;
    public static Subtexture Arrows;

    [MonoModReplace]
    public static void Update()
    {
        Confirm = Back = Alt = SkipReplay = null;
        for (int i = 0; i < 4; i++)
        {
            if (TFGame.PlayerInputs[i] != null)
            {
                Confirm = TFGame.PlayerInputs[i].ConfirmIcon;
                Back = TFGame.PlayerInputs[i].BackIcon;
                Alt = TFGame.PlayerInputs[i].AltIcon;
                SkipReplay = TFGame.PlayerInputs[i].SkipReplayIcon;
                StartGame = TFGame.PlayerInputs[i].StartIcon;
                AltShoot = TFGame.PlayerInputs[i].AltShootIcon;
                Arrows = ((patch_PlayerInput)TFGame.PlayerInputs[i]).get_ArrowsIcon_base();
                break;
            }
        }
    }
}
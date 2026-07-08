using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.MenuButtonGuide")]
public class MenuButtonGuide : TowerFall.MenuButtonGuide
{
    private string title;
    private Subtexture icon;

    public MenuButtonGuide(int slot, ButtonModes buttonMode, string title) : base(slot, TowerFall.MenuButtonGuide.ButtonModes.Alt, title)
    {
    }

    [MonoModPublic]
    public void SetDetails(MenuButtonGuide.ButtonModes buttonMode, string title)
    {
        this.title = title;
        PlayerInput playerInput = null;
        for (int i = 0; i < 4; i++)
        {
            if (TFGame.PlayerInputs[i] != null)
            {
                playerInput = TFGame.PlayerInputs[i];
                break;
            }
        }
        switch (buttonMode)
        {
        case ButtonModes.None:
            icon = null;
            break;
        case ButtonModes.Confirm:
            icon = playerInput.ConfirmIcon;
            break;
        case ButtonModes.Back:
            icon = playerInput.BackIcon;
            break;
        case ButtonModes.Alt:
            icon = playerInput.AltIcon;
            break;
        case ButtonModes.Alt2:
            icon = playerInput.Alt2Icon;
            break;
        case ButtonModes.Start:
            icon = playerInput.StartIcon;
            break;
        case ButtonModes.SaveReplay:
            icon = playerInput.SaveReplayIcon;
            break;
        case ButtonModes.Arrows:
            icon = (playerInput as patch_PlayerInput).get_ArrowsIcon_base();
            break;
        }
    }

    public enum ButtonModes
    {
        None,
        Confirm,
        Back,
        Alt,
        Alt2,
        Start,
        SaveReplay,
        Arrows
    }
}
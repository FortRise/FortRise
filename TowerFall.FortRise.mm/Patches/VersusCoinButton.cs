using Microsoft.Xna.Framework;

namespace TowerFall;

public class patch_VersusCoinButton : VersusCoinButton
{
    public patch_VersusCoinButton(Vector2 position, Vector2 tweenFrom) : base(position, tweenFrom)
    {
    }

    public extern void orig_Render();

    public override void Render() 
    {
        if (patch_MainMenu.VersusMatchSettings.CurrentModeName == null)
            patch_MainMenu.VersusMatchSettings.CurrentModeName = "LastManStanding";

        var mode = CustomVersusRoundLogic.LookUpModes[patch_MainMenu.VersusMatchSettings.CurrentModeName];
        patch_MainMenu.VersusMatchSettings.Mode = mode;

        orig_Render();
    }
}

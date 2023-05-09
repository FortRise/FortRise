using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_VersusModeButton : VersusModeButton
{
    private Wiggler iconWiggler;
    public patch_VersusModeButton(Vector2 position, Vector2 tweenFrom) : base(position, tweenFrom)
    {
    }

    [MonoModLinkTo("TowerFall.BorderButton", "Update")]
    public void base_Update() 
    {
        base.Update();
    }

    [MonoModLinkTo("TowerFall.BorderButton", "OnConfirm")]
    protected void base_OnConfirm() 
    {
        base.OnConfirm();
    }

    public override void Update()
    {
        const int BuiltInModeCount = 3;
        base_Update();

        Modes mode = MainMenu.VersusMatchSettings.Mode;
        string currentModeName = patch_MainMenu.VersusMatchSettings.CurrentModeName;
        if (!Selected)
            return;
        
        int idx = CustomVersusRoundLogic.VersusModes.IndexOf(currentModeName);
        if (idx < CustomVersusRoundLogic.VersusModes.Count - 1 && MenuInput.Right)
        {
            patch_MainMenu.VersusMatchSettings.IsCustom = !((idx + 1) < BuiltInModeCount);
            var modeName = patch_MainMenu.VersusMatchSettings.CurrentModeName = CustomVersusRoundLogic.VersusModes[idx + 1];
            patch_MainMenu.VersusMatchSettings.Mode = CustomVersusRoundLogic.LookUpModes[modeName];
            Sounds.ui_move2.Play(160f, 1f);
            iconWiggler.Start();
            base_OnConfirm();
            UpdateSides();
        }
        else if (idx > 0 && MenuInput.Left)
        {
            patch_MainMenu.VersusMatchSettings.IsCustom = !((idx - 1) < BuiltInModeCount);
            var modeName = patch_MainMenu.VersusMatchSettings.CurrentModeName = CustomVersusRoundLogic.VersusModes[idx - 1];
            patch_MainMenu.VersusMatchSettings.Mode = CustomVersusRoundLogic.LookUpModes[modeName];
            Sounds.ui_move2.Play(160f, 1f);
            iconWiggler.Start();
            base_OnConfirm();
            UpdateSides();
        }
    }

    private extern void orig_UpdateSides();

    private void UpdateSides() 
    {
        orig_UpdateSides();

        int idx = CustomVersusRoundLogic.VersusModes.IndexOf(patch_MainMenu.VersusMatchSettings.CurrentModeName);

        DrawRight = (idx < CustomVersusRoundLogic.VersusModes.Count-1);
    }

    public extern static Subtexture orig_GetModeIcon(Modes mode);

    public static Subtexture GetModeIcon(Modes mode)
    {
        if (patch_MainMenu.VersusMatchSettings.IsCustom) 
        {
            if (FortRise.RiseCore.RoundLogicIdentifiers.TryGetValue(
                patch_MainMenu.VersusMatchSettings.CurrentModeName, out var val))
                    return val.Icon;
        }
        return orig_GetModeIcon(mode);
    }

    public extern static string orig_GetModeName(Modes mode);

    public static string GetModeName(Modes mode)
    {
        if (patch_MainMenu.VersusMatchSettings.IsCustom) 
        {
            if (FortRise.RiseCore.RoundLogicIdentifiers.TryGetValue(
                patch_MainMenu.VersusMatchSettings.CurrentModeName, out var val))
                    return val.Name.ToUpper();
        }
        return orig_GetModeName(mode);
    }
}

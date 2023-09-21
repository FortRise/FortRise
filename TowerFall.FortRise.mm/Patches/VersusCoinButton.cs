using Microsoft.Xna.Framework;
using MonoMod;
using FortRise;

namespace TowerFall;

public class patch_VersusCoinButton : VersusCoinButton
{
    private static string[] LengthNames;
    private bool IsCustomizedGoal => patch_MainMenu.VersusMatchSettings.MatchLength == patch_MatchSettings.patch_MatchLengths.Custom;
    public patch_VersusCoinButton(Vector2 position, Vector2 tweenFrom) : base(position, tweenFrom)
    {
    }

    [MonoModConstructor]
    public static void cctor() 
    {
        LengthNames = new string[5] {"INSTANT MATCH", "QUICK MATCH", "STANDARD MATCH", "EPIC MATCH", "CUSTOM MATCH"};
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

    [MonoModLinkTo("TowerFall.BorderButton", "System.Void Update()")]
    [MonoModIgnore]
    public void base_Update() 
    {
        base.Update();
    }

    [MonoModLinkTo("TowerFall.BorderButton", "System.Void OnConfirm()")]
    [MonoModIgnore]
    public void base_OnConfirm() 
    {
        base.OnConfirm();
    }

    [MonoModReplace]
    public override void Update()
    {
        base_Update();
        this.Visible = MainMenu.VersusMatchSettings.CanPlayThisMode;
        if (base.Selected)
        {
            if (MenuInput.Confirm) 
            {
                Sounds.ui_click.Play(160f, 1f);
                patch_MainMenu.VersusMatchSettings.MatchLength = patch_MatchSettings.patch_MatchLengths.Custom;
                base_OnConfirm();
                this.UpdateSides();
                var customMatch = new UIVersusCustomMatch(patch_MatchSettings.CustomGoal);
                customMatch.OnBack = () => {
                    base.Selected = true;
                    patch_MatchSettings.CustomGoal = customMatch.Count;
                };
                base.Selected = false;
                Scene.Add(customMatch);
                return;
            }
            if (MenuInput.Right && MainMenu.VersusMatchSettings.MatchLength < MatchSettings.MatchLengths.Epic)
            {
                Sounds.ui_move2.Play(160f, 1f);
                MainMenu.VersusMatchSettings.MatchLength++;
                base_OnConfirm();
                this.UpdateSides();
                return;
            }
            if (MenuInput.Left && MainMenu.VersusMatchSettings.MatchLength > MatchSettings.MatchLengths.Instant)
            {
                Sounds.ui_move2.Play(160f, 1f);
                MainMenu.VersusMatchSettings.MatchLength--;
                base_OnConfirm();
                this.UpdateSides();
            }
        }
    }

    [MonoModIgnore]
    private extern void UpdateSides();
}